using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Security;
using System.Reflection;
using Windows.System.Threading;
using ULog;
using WebRequest.Common;

namespace WebRequest.Http
{
    class HttpEventArgs : EventArgs
    {
        public HttpRequest Request { get; private set; }

        public HttpEventArgs(HttpRequest request)
        {
            Request = request;
        }
    }

    class ProxyAuthRequiredEventArgs : EventArgs
    {
        public string ProxyUri
        {
            get;
            set;
        }

        public string ProxyName
        {
            get;
            set;
        }

        public string ProxyUserName
        {
            get;
            set;
        }

        public string ProxyPassword
        {
            get;
            set;
        }
    }

    class HttpLayer
    {
        public const string StreamContentType = "application/octet-stream";
        public const string JSonContentType = "application/json";
        public const string JavescriptContentType = "application/x-javascript";
        public const string vndErrorJSonContentType = "application/vnd.error+json";
        public const string formContentType = "application/x-www-form-urlencoded";

        public const string RequestHeaderContentRange = "Content-Range";

        private object cookieLocker = new object();
        private Dictionary<string, CookieCollection> cookieContainer = new Dictionary<string, CookieCollection>();

        public event EventHandler<HttpEventArgs> OnResponsed;
        //public event EventHandler<HttpEventArgs> OnCancelled;
        public event EventHandler<HttpEventArgs> OnFailed;

        private int sendRequestTimeoutInMillseconds = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
        private int streamAccessTimeout = (int)TimeSpan.FromMinutes(30).TotalMilliseconds;
        private int maxStreamAccessTimeout = (int)TimeSpan.FromMinutes(45).TotalMilliseconds;
        private int streamAccessTimeoutIncStep = (int)TimeSpan.FromMinutes(1).TotalMilliseconds;

        static HttpLayer()
        {
            /*ServicePointManager.ServerCertificateValidationCallback = (e, cert, chain, error) =>
            {
                return true;
            };*/
        }

        private enum AuthenticationTarget { None, Request, Proxy };
        private const string httpWWWAuthenticateKey = "WWW-Authenticate";
        
        private AuthenticationTarget authTarget;
        private string authUsername;
        private string authPassword;
        private int proxyId = 0;

        public object proxyAuthReqiredLocker = new object();
        public event EventHandler<ProxyAuthRequiredEventArgs> ProxyAuthorizationRequired;

        public HttpLayer()
        {
            requestTimerDict = new Dictionary<HttpRequest, DateTime>();
            requestTimer = ThreadPoolTimer.CreatePeriodicTimer(new TimerElapsedHandler(RequestTimer), TimeSpan.FromSeconds(10));

            //ServicePointManager.DefaultConnectionLimit = 10;
        }

        private object timerLocker = new object();
        private ThreadPoolTimer requestTimer;
        private Dictionary<HttpRequest, DateTime> requestTimerDict;
        
        private void RequestTimer(ThreadPoolTimer timer)
        {
            lock (timerLocker)
            {
                if (!requestTimerDict.Any())
                    return;

                var now = DateTime.Now;
                var requests = requestTimerDict.Keys.ToArray();
                foreach (var req in requests)
                {
                    var reqTime = now.Subtract(requestTimerDict[req]);
                    if (reqTime.TotalMilliseconds > sendRequestTimeoutInMillseconds)
                    {
                        req.TimeoutAbort();

                        requestTimerDict.Remove(req);

                        LogHelper.OnlineLogger.Warn("Http Request timeout " + LogRequetId(req.RequestId));
                    }
                }
            }
        }

        private void AddRequestTimer(HttpRequest req)
        {
            lock (timerLocker)
            {
                requestTimerDict[req] = DateTime.Now;
            }
        }

        private void RemoveRequestTimer(HttpRequest req)
        {
            lock (timerLocker)
            {
                requestTimerDict.Remove(req);
            }
        }

        public void SendRequest(HttpRequest request)
        {
            DiagnoseHelper.CheckArgument(request, "Can not cancel empty request");
            DiagnoseHelper.CheckReference(request.Context, "You should set context");

            HttpWebRequest originalReq = request.OriginalRequest;
            originalReq.UseDefaultCredentials = true;
            //originalReq.ReadWriteTimeout = streamAccessTimeout;

            string additinalCookie = null;
            if (request.Headers != null)
            {
                if (originalReq.Headers == null)
                    originalReq.Headers = new WebHeaderCollection();

                foreach (var pairHeader in request.Headers)
                {
                    if (pairHeader.Key == HttpHeaderHelper.cookieKey)
                    {
                        additinalCookie = pairHeader.Value;
                        continue;
                    }
                    originalReq.Headers[pairHeader.Key] = pairHeader.Value;
                }
            }
            SetAuthentication(originalReq);
            SaveCookies(originalReq, additinalCookie);
            //SetUserAgent(originalReq);
            if (request.RequestStreams != null)
            {
                try
                {
                    LogHelper.OnlineLogger.Info(string.Format("Send Http Request Stream to {0}. {1}", originalReq.RequestUri.ToString(), LogRequetId(request.RequestId)));
                    var result = originalReq.BeginGetRequestStream(new AsyncCallback(OnRequestStreamReady), request);
                    LogHelper.OnlineLogger.Info(string.Format("Sent {0}. {1}", originalReq.RequestUri.ToString(), LogRequetId(request.RequestId)));
                    AddRequestTimer(request);
                    
                }
                catch (Exception ex)
                {
                    LogHelper.OnlineLogger.Error(string.Format("Can not send http request \n{0}" + LogRequetId(request.RequestId), ex.ToString()));
                    request.Exception = ex;
                    ReportFailed(request);
                }
            }
            else
            {
                try
                {
                    LogHelper.OnlineLogger.Info(string.Format("Send Http Request to {0}. {1}", originalReq.RequestUri.ToString(), LogRequetId(request.RequestId)));
                    var result = originalReq.BeginGetResponse(new AsyncCallback(OnResponseReady), request);
                    LogHelper.OnlineLogger.Info(string.Format("Sent {0}. {1}", originalReq.RequestUri.ToString(), LogRequetId(request.RequestId)));
                    AddRequestTimer(request);
                    
                }
                catch (Exception ex)
                {
                    LogHelper.OnlineLogger.Error(string.Format("Can not begin get http response. {0}" + LogRequetId(request.RequestId), ex.ToString()));

                    request.Exception = ex;
                    ReportFailed(request);
                }
            }
        }

        private bool IsProxyAuthenticationRequired(WebException ex, HttpWebRequest req)
        {
            bool isProxyAuthRequired = false;

            if (ex.Response != null)
            {
                var httpResp = ex.Response as HttpWebResponse;
                if (httpResp != null)
                {

                    if (httpResp.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                    {
                        isProxyAuthRequired = true;

                        lock (proxyAuthReqiredLocker)
                            authTarget = AuthenticationTarget.Proxy;
                    }
                    else
                    {
                        var headers = httpResp.Headers[httpWWWAuthenticateKey];
                        if (!string.IsNullOrEmpty(headers) &&
                            (httpResp.ResponseUri == null || !httpResp.ResponseUri.Host.Equals(req.RequestUri.Host, StringComparison.OrdinalIgnoreCase)))
                        {

                            isProxyAuthRequired = true;
                            lock (proxyAuthReqiredLocker)
                                authTarget = AuthenticationTarget.Request;
                        }
                    }
                        
                }
            }

            return isProxyAuthRequired;
        }

        private void SetAuthentication(HttpWebRequest req)
        {
            lock (proxyAuthReqiredLocker)
            {
                if (authTarget == AuthenticationTarget.Proxy)
                {
                    if (req.Proxy != null && req.Proxy.Credentials == null && authUsername != null && authPassword != null)
                    {
                        req.Proxy.Credentials = new NetworkCredential(authUsername, authPassword);
                    }
                }
                else if (authTarget == AuthenticationTarget.Request && authUsername != null && authPassword != null)
                {
                    req.Credentials = new NetworkCredential(authUsername, authPassword);
                }
                
            }
        }

        /// <summary>
        /// Set Proxy Properties
        /// </summary>
        /// <param name="uriProxy"></param>
        /// <param name="name"></param>
        /// <param name="password"></param>
        public void SetProxy(string name, string password)
        {
            lock (proxyAuthReqiredLocker)
            {
                //proxyUri = uriProxy;
                authUsername = name;
                authPassword = password;

                proxyId++;
            }
        }

        private void HandleProxyRequired(HttpRequest req, string proxyName)
        {
            if (ProxyAuthorizationRequired != null)
            {
                lock (proxyAuthReqiredLocker)
                {
                    if (req.ProxyId == proxyId)
                    {
                        ProxyAuthRequiredEventArgs eArgs = null;

                        if (authTarget == AuthenticationTarget.Proxy)
                        {
                            var url = req.OriginalRequest.Proxy.GetProxy(req.OriginalRequest.RequestUri);

                            req.OriginalRequest.Proxy.Credentials = null;

                            eArgs = new ProxyAuthRequiredEventArgs()
                            {
                                ProxyUri = url.AbsoluteUri,
                                ProxyName = proxyName
                            };
                        }
                        else if(authTarget == AuthenticationTarget.Request)
                        {
                            eArgs = new ProxyAuthRequiredEventArgs()
                            {
                                ProxyName = proxyName
                            };
                        }

                        ProxyAuthorizationRequired(this, eArgs);
                       
                        if (eArgs.ProxyUserName != null && eArgs.ProxyPassword != null)
                            SetProxy(eArgs.ProxyUserName, eArgs.ProxyPassword);
                        else
                            return;
                    }

                    req.ProxyId = proxyId;

                }

                req.RecreateHttpRequest();
                SendRequest(req);
            }
        }

        private void HandleProxyAuthPassed()
        {
            lock (proxyAuthReqiredLocker)
            {
                proxyId = 0;
            }
        }

        private const string proxyAuthenticateHead = "Proxy-Authenticate";
        private static readonly char[] authSplitor = { '=' };
        private const char quotation = '\"';
        private string GetProxyAuthenticate(HttpWebResponse resp)
        {
            string proxyName = null;
            var auth = resp.Headers[proxyAuthenticateHead];
            if (auth != null)
            {
                var authParts = auth.Split(authSplitor, StringSplitOptions.RemoveEmptyEntries);
                if (authParts.Length == 2)
                {
                    proxyName = authParts[1].Trim(quotation);
                }
            }
            else
            {
                var authName = resp.Headers[httpWWWAuthenticateKey];
                //var authName = auths.FirstOrDefault(a => a.StartsWith("basic", StringComparison.OrdinalIgnoreCase));
                if (auth != null)
                {
                    var basicParts = authName.Split(authSplitor);
                    if (basicParts.Length == 2)
                    {
                        proxyName = basicParts[1].Trim(quotation);
                    }
                }

            }

            return proxyName;
        }

        public void CancelRequest(HttpRequest request)
        {
            DiagnoseHelper.CheckArgument(request, "Can not cancel empty request");
            request.Stop();
        }

        public void CancelRequest(HttpResponse response)
        {
            DiagnoseHelper.CheckArgument(response, "Can not cancel empty response");
            response.IsStopped = true;
        }

        private void OnRequestStreamReady(IAsyncResult ar)
        {
            var req = ar.AsyncState as HttpRequest;
            RemoveRequestTimer(req);
            try
            {
                bool isUpload = false;
                Stream reqStream = req.OriginalRequest.EndGetRequestStream(ar);

                HandleProxyAuthPassed();
                try
                {
                    isUpload = SendRequestStreamInBlocks(reqStream, req);

                    if (isUpload)
                    {
                        req.OriginalRequest.BeginGetResponse(new AsyncCallback(OnResponseReady), req);
                        AddRequestTimer(req);
                    }
                    else
                    {
                        throw new WebException("Canceled by user", WebExceptionStatus.RequestCanceled);
                    }
                }
                finally
                {
                    try
                    {
                        reqStream.Dispose();
                        if (!isUpload)
                            req.OriginalRequest.Abort();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.OnlineLogger.Error(string.Format("Close http request stream failed. {0}" + LogRequetId(req.RequestId), ex.ToString()));
                        try
                        {
                            req.OriginalRequest.Abort();
                        }
                        catch (Exception ex2)
                        {
                            LogHelper.OnlineLogger.Error(string.Format("Abort http request stream failed. {0}" + LogRequetId(req.RequestId), ex2.ToString()));
                        }
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    if (IsProxyAuthenticationRequired(ex, req.OriginalRequest))
                    {
                        HandleProxyRequired(req, GetProxyAuthenticate(ex.Response as HttpWebResponse));
                        return;
                    }

                }

                HandleWebException(ex, req);
            }
            catch (Exception ex)
            {
                LogHelper.OnlineLogger.Error(string.Format("Can not get http response. {0}" + LogRequetId(req.RequestId), ex.ToString()));

                req.Exception = ex;
                ReportFailed(req);
            }
        }

        /// <summary>
        /// Sent stream to cloud by chunk
        /// </summary>
        /// <param name="target"></param>
        /// <param name="req"></param>
        /// <returns>Is send succeed</returns>
        private bool SendRequestStreamInBlocks(Stream target, HttpRequest req)
        {
            bool result = false;
            try
            {
                var reqBufEnumerator = req.RequestStreamEnumerator;

                var range = req.OriginalRequest.Headers[RequestHeaderContentRange];
                ContentRangeHelper helper = ContentRangeHelper.Parse(range);
                if (helper != null && helper.Range != null)
                {
                    reqBufEnumerator.Skip(helper.Range.Start);
                }

                int targetHash = target.GetHashCode();
                LogHelper.OnlineLogger.Debug(string.Format("Start to send request stream, id={0}. {1}", targetHash, LogRequetId(req.RequestId)));

                long fileLen = -1;
                try
                {
                    fileLen = req.RequestStreams.Length;
                }
                catch (ObjectDisposedException ex)
                {
                    LogHelper.OnlineLogger.Warn(string.Format("Can not get file length, stream is disposed {0}. {1}", ex.ToString(), LogRequetId(req.RequestId)));
                }

                if (fileLen != -1)
                {
                    req.ReportProgress(reqBufEnumerator.Offset, fileLen, 0);

                    while (!req.IsStopped && reqBufEnumerator.MoveNext())
                    {
                        try
                        {
                            target.Write(reqBufEnumerator.Current, 0, reqBufEnumerator.CurrentLength);
                            target.Flush();
                        }
                        catch (WebException ex)
                        {
                            LogHelper.OnlineLogger.Error(string.Format("Send stream is failed in the middle, id = {0}, ex = {1}. {2}", targetHash, ex.ToString(), LogRequetId(req.RequestId)));
                            /*if (ex.Status == WebExceptionStatus.Timeout)
                            {
                                LogHelper.OnlineLogger.WarnFormat("Need to increase stream access timeout: {0}, id = {1}. {2}", streamAccessTimeout, targetHash, LogRequetId(req.RequestId));
                                if (streamAccessTimeout < maxStreamAccessTimeout)
                                {
                                    streamAccessTimeout += streamAccessTimeoutIncStep;
                                }
                            }*/

                            throw;
                        }
                        catch (NotSupportedException ex)
                        {
                            LogHelper.OnlineLogger.Warn(string.Format("Can not write stream, {0}. {1}", ex.ToString(), LogRequetId(req.RequestId)));
                            throw new WebException(ex.Message, ex);
                        }

                        req.ReportProgress(reqBufEnumerator.Offset, fileLen, reqBufEnumerator.CurrentLength);
                        LogHelper.OnlineLogger.Debug(string.Format("Sending at {0}/{1}, id={2}. {3}", reqBufEnumerator.Offset, fileLen, targetHash, LogRequetId(req.RequestId)));
                    }

                    result = reqBufEnumerator.Offset == fileLen;
                }
            }
            finally
            {
                req.Close();
            }
            return result;
        }

        private void OnResponseReady(IAsyncResult ar)
        {
            var req = ar.AsyncState as HttpRequest;
            RemoveRequestTimer(req);
            HttpWebResponse httpResponse = null;

            bool isRequiringProxyAuth = false;
            try
            {
                try
                {
                    httpResponse = req.OriginalRequest.EndGetResponse(ar) as HttpWebResponse;
                    HandleProxyAuthPassed();
                }
                catch (WebException ex)
                {
                    LogHelper.OnlineLogger.Warn(string.Format("Recevie respose exception {0}\n{1}. {2}", ex.ToString(), ex.Status, LogRequetId(req.RequestId)));
                    if (ex.Response != null)
                    {
                        httpResponse = ex.Response as HttpWebResponse;
                        if (httpResponse == null)
                        {
                            HandleWebException(ex, req);
                            return;
                        }
                        else if (IsProxyAuthenticationRequired(ex, req.OriginalRequest))
                        {
                            isRequiringProxyAuth = true;
                        }
                    }
                    else
                    {
                        HandleWebException(ex, req);
                        return;
                    }
                }
                LogHelper.OnlineLogger.Info(string.Format("Receive http response from {0}. {1}", httpResponse.ResponseUri.ToString(), LogRequetId(req.RequestId)));

                if (!isRequiringProxyAuth)
                {
                    req.Response = CreateResponse(httpResponse, req.RequestId);
                    req.ReportReady(ReadyState.Succeeded);

                    GetCookies(httpResponse);

                    req.ReportResponsed();

                    ReceiveResponseStreamInBlocks(httpResponse.GetResponseStream(), req.Response, req.ChunkSize);

                    req.Response.DeseralizeFromStream();

                    ReportResponsed(req);
                }

            }
            catch (WebException ex)
            {
                HandleWebException(ex, req);
            }
            catch (Exception ex)
            {
                LogHelper.OnlineLogger.Error(string.Format("Can not get http response stream. {0}", LogRequetId(req.RequestId)));
                LogHelper.OnlineLogger.Error(ex.ToString());

                if (req.Response == null)
                    req.Exception = ex;
                else
                    req.Response.Exception = ex;
                ReportFailed(req);
            }
            finally
            {
                if (httpResponse != null)
                    httpResponse.Dispose();

                if (isRequiringProxyAuth)
                {
                    HandleProxyRequired(req, GetProxyAuthenticate(httpResponse));
                }
            }
        }

        private void ReceiveResponseStreamInBlocks(Stream source, HttpResponse resp, int receiveBlockSize)
        {
            /*if (resp.OriginalResponse.ContentLength < 0)
            {
                throw new WebException("HttpLayer error: Invalid ContentLength=" + resp.OriginalResponse.ContentLength);
            }*/

            int sourceHash = source.GetHashCode();
            LogHelper.OnlineLogger.Debug(string.Format("Start to receive response stream, id={0}. {1}", sourceHash, LogRequetId(resp.ResponseId)));
            StreamBufferEnumerator se = new StreamBufferEnumerator(resp.ResponseId, source, resp.OriginalResponse.ContentLength, receiveBlockSize);

            while (true)
            {
                try
                {
                    if(!se.MoveNext())
                        break;
                }
                catch (WebException ex)
                {
                    LogHelper.OnlineLogger.Error(string.Format("Receive stream is failed in the middle, id={0}, ex={1}. {2}", 
                        sourceHash, ex.ToString(), LogRequetId(resp.ResponseId))); 
                    /*if (ex.Status == WebExceptionStatus.Timeout)
                    {
                        LogHelper.OnlineLogger.WarnFormat("Need to increase stream access timeout: {0}, id={1}. {2}", 
                            streamAccessTimeout, sourceHash, LogRequetId(resp.ResponseId));
                        if (streamAccessTimeout < maxStreamAccessTimeout)
                        {
                            streamAccessTimeout += streamAccessTimeoutIncStep;
                        }
                    }*/

                    throw;
                }

                if (resp.IsStopped)
                {
                    throw new WebException("Response cancel by user", WebExceptionStatus.RequestCanceled);
                }

                resp.ResponseStream.Write(se.Current, 0, se.CurrentLength);
                resp.ReportProgress(se.Offset, resp.OriginalResponse.ContentLength, se.CurrentLength);
                LogHelper.OnlineLogger.Debug(string.Format("Data {0}/{1}, id={2}. {3}", se.Offset, resp.OriginalResponse.ContentLength, sourceHash, LogRequetId(resp.ResponseId)));
            }

            /*if (resp.OriginalResponse.ContentLength != se.Offset)
            {
                throw new WebException(string.Format("HttpLayer error: ContentLength({0}) != StreamLength({1})", resp.OriginalResponse.ContentLength, se.Offset));
            }*/
        }

        private HttpResponse CreateResponse(HttpWebResponse response, int requestId)
        {
            if (IsContentTypeJSon(response.ContentType))
            {
                return new JSonResponse(response, requestId);
            }
            else
            {
                return new HttpResponse(response, requestId);
            }
        }

        private void HandleWebException(WebException ex, HttpRequest request)
        {
            LogHelper.OnlineLogger.Error(string.Format("Handle web exception {0}. {1}", ex.Status, LogRequetId(request.RequestId)));
            LogHelper.OnlineLogger.Error(ex.ToString());

            if (ex.Status == WebExceptionStatus.RequestCanceled)
            {
                if(!request.IsTimeout)
                    ReportCancelled(request);
                else
                {
                    request.Exception = new TimeoutException("Can not get response");
                    ReportFailed(request);
                }
            }
            else
            {
                if (request.Response == null)
                    request.Exception = ex;
                else
                {
                    if (ex.Response != null)
                    {
                        var webResp = ex.Response as HttpWebResponse;
                        if(webResp != null)
                            request.Response.StatusCode = webResp.StatusCode;
                    }
                    request.Response.Exception = ex;
                }

                ReportFailed(request);
            }
        }

        private void ReportResponsed(HttpRequest request)
        {
            request.Response.ReportReady(ReadyState.Succeeded);

            if (OnResponsed != null)
            {
                OnResponsed(this, new HttpEventArgs(request));
            }
        }

        private void ReportCancelled(HttpRequest request)
        {
            if (request.Response == null)
                request.ReportReady(ReadyState.Cancelled);
            else
                request.Response.ReportReady(ReadyState.Cancelled);

            if (OnFailed != null)
            {
                OnFailed(this, new HttpEventArgs(request));
            }
        }

        private void ReportFailed(HttpRequest request)
        {
            if (request.Response == null)
                request.ReportReady(ReadyState.Failed);
            else
                request.Response.ReportReady(ReadyState.Failed);

            if (OnFailed != null)
            {
                OnFailed(this, new HttpEventArgs(request));
            }
        }

        private void GetCookies(HttpWebResponse httpResponse)
        {
            if (httpResponse.Cookies != null && httpResponse.Cookies.Count != 0)
            {
                lock (cookieLocker)
                {
                    cookieContainer[httpResponse.ResponseUri.Host] = httpResponse.Cookies;
                }
            }
        }

        private void SaveCookies(HttpWebRequest httpRequest, string additionalCookie = null)
        {
            lock (cookieLocker)
            {
                httpRequest.CookieContainer = new CookieContainer();
                CookieCollection cookies = null;
                cookieContainer.TryGetValue(httpRequest.RequestUri.Host, out cookies);
                if(additionalCookie != null)
                {
                    if(cookies == null)
                        cookies = new CookieCollection();

                    var addCookie = HttpHeaderHelper.CreateCookie(additionalCookie);
                    if (addCookie != null)
                    {
                        cookies.Add(addCookie);
                    }
                }
                if (cookies != null && cookies.Count != 0)
                {
                    httpRequest.CookieContainer.Add(httpRequest.RequestUri, cookies);
                    DebugDumpCookie(cookies);
                }

            }
        }

        private void DebugDumpCookie(CookieCollection cookies)
        {
            StringBuilder debugText = new StringBuilder("Cookies:");
            foreach (var obj in cookies)
            {
                var cookie = obj as Cookie;
                debugText.Append(cookie.ToString());
                debugText.Append(";");
            }

            LogHelper.OnlineLogger.Debug(debugText.ToString());
        }

        public static bool IsContentTypeJSon(string contentType)
        {
            var ct = contentType.ToLower();
            return ct.Contains(JSonContentType) || ct.Contains(JavescriptContentType) || ct.Contains(vndErrorJSonContentType);
        }

        public static string LogRequetId(int reqId)
        {
            return "ReqId=" + reqId;
        }
    }
}
