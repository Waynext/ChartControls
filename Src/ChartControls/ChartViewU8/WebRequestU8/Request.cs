using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using WebRequest.Http;
using System.IO;
using WebRequest.Common;

namespace WebRequest
{
    public class ProgressEventArgs : EventArgs
    {
        public ProgressEventArgs(long sended, long total, int latestSize)
        {
            Sended = sended;
            Total = total;
            LatestSize = latestSize;
        }

        public long Sended { get; private set; }
        public long Total { get; private set; }
        public int LatestSize { get; private set; }
    }

    public class ReadyEventArgs : EventArgs
    {
        public ReadyState State { get; private set; }

        public ReadyEventArgs(ReadyState state)
        {
            State = state;
        }
    }

    public enum RequestPriority { Realtime = 0, High, Normal, Low }

    abstract class Request
    {
        public Request(RequestPriority priority)
        {
            Id = GenerateId();
            Priority = priority;
        }

        public int Id { get; private set; }

        public string Uri { get; protected set; }

        public RequestPriority Priority { get; private set; }
        public Response Response { get; internal set; }

        public List<KeyValuePair<string, string>> requestHeaders;

        public event EventHandler<ProgressEventArgs> OnProgressing;

        internal HttpRequest HttpRequest { get; set; }
        internal void PrepareLowLevelRequest()
        {
            Encode();
            CreateResponse();

            //Link Request and Response with this Id;
            Response.Id = Id;

            HttpRequest.Context = this;

            HttpRequest.OnProgressing += OnHttpRequestPrgogressing;
            HttpRequest.OnReady += OnHttpRequestReady;
            HttpRequest.OnResponsed += OnHttpResponsed;
        }

        private void OnHttpRequestReady(object sender, HttpReadyEventArgs e)
        {
            OnProgressing = null;

            //Clear HttpRequest 
            HttpRequest.OnProgressing -= OnHttpRequestPrgogressing;
            HttpRequest.OnReady -= OnHttpRequestReady;

            if (e.State != ReadyState.Succeeded)
            {
                Response.SetException(HttpRequest.Exception);
                HttpRequest.OnResponsed -= OnHttpResponsed;
                HttpRequest = null;

                Response.HttpResponse_OnReady(sender, e);
            }
        }

        private void OnHttpRequestPrgogressing(object sender, HttpProgressEventArgs e)
        {
            if (OnProgressing != null)
            {
                OnProgressing(this, new ProgressEventArgs(e.Offset, e.Total, e.LatestSize));
            }
        }

        private void OnHttpResponsed(object sender, EventArgs e)
        {
            Response.SetHttpResponse(HttpRequest.Response);
            HttpRequest.OnResponsed -= OnHttpResponsed;
            HttpRequest = null;
        }

        //Encode request into low level request
        internal virtual void Encode()
        {
            HttpRequest = new HttpRequest(Uri);
            if (!requestHeaders.IsNullOrEmpty())
            {
                HttpRequest.Headers = requestHeaders;
            }
        }

        internal virtual void CreateResponse()
        {
            Response = new Response();
        }

        //True if it's sending, 
        //false if it's waiting
        internal bool IsSending { get; set; }

        private static int idSource = 0;
        private static int GenerateId()
        {
            return Interlocked.Increment(ref idSource);
        }
    }

    class Response
    {
        public Response()
        {
            IsSucceeded = false;
        }

        public int Id { get; internal set; }

        public Exception Exception
        {
            get;
            internal set;
        }
        public bool IsSucceeded { get; internal set; }

        protected Stream ResponseStream
        {
            get
            {
                return HttpResponse != null ? HttpResponse.ResponseStream : null;
            }
        }

        public object Context
        {
            get;
            set;
        }

        protected void CleanResponseStream()
        {
            if (HttpResponse != null)
            {
                HttpResponse.CleanReponseStream();
            }
        }

        internal HttpResponse HttpResponse { get; private set; }
        public event EventHandler<ReadyEventArgs> OnReady;
        public event EventHandler<ProgressEventArgs> OnProgressing;

        internal void SetException(Exception ex)
        {
            Exception = ex;
            IsSucceeded = false;
        }

        internal void SetHttpResponse(HttpResponse response)
        {
            HttpResponse = response;

            HttpResponse.OnProgressing += HttpResponse_OnProgressing;
            HttpResponse.OnReady += HttpResponse_OnReady;

            IsSucceeded = true;
        }

        private void HttpResponse_OnProgressing(object sender, HttpProgressEventArgs e)
        {
            ReportProgressing(e.Offset, e.Total, e.LatestSize);
        }

        internal void HttpResponse_OnReady(object sender, HttpReadyEventArgs e)
        {
            ReadyState state = e.State;
            if (HttpResponse != null)
            {
                if (HttpResponse.Exception != null)
                {
                    Exception = HttpResponse.Exception;
                }
                
                Decode();

                if (!IsSucceeded && state != ReadyState.Cancelled && state != ReadyState.Failed)
                {
                    state = ReadyState.Failed;
                }
            }
            else if(state == ReadyState.Failed)
            {
                ExplainException();
            }

            if (OnReady != null)
                OnReady(this, new ReadyEventArgs(state));
            if (HttpResponse != null)
            {
                HttpResponse.OnProgressing -= HttpResponse_OnProgressing;
                HttpResponse.OnReady -= HttpResponse_OnReady;
                HttpResponse.Dispose();
                HttpResponse = null;
            }

            OnReady = null;
            OnProgressing = null;
        }

        protected virtual void ReportProgressing(long sended, long total, int latestSize)
        {
            if (OnProgressing != null)
            {
                ProgressEventArgs e = new ProgressEventArgs(sended, total, latestSize);
                OnProgressing(this, e);
            }
        }

        //Decode low level response to response
        internal virtual void Decode() { }

        //Get not get repsonse but expcetion
        protected virtual void ExplainException() { }

        protected string GetResponseHeader(string header)
        {
            return HttpResponse.OriginalResponse.Headers[header];
        }
    }
}
