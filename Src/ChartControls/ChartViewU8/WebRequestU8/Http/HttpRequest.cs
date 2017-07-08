using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Threading;
using ULog;
using WebRequest.Common;

namespace WebRequest.Http
{
    class HttpProgressEventArgs : EventArgs
    {
        public long Offset { get; private set; }
        public long Total { get; private set; }
        public int LatestSize { get; private set; }
        public HttpProgressEventArgs(long offset, long total, int latestSize)
        {
            Offset = offset;
            Total = total;
            LatestSize = latestSize;
        }
    }

    class HttpReadyEventArgs : EventArgs
    {
        public ReadyState State { get; private set; }

        public HttpReadyEventArgs(ReadyState state)
        {
            State = state;
        }
    }

    class HttpRequest
    {
        private static Random r = new Random(DateTime.Now.Millisecond);

        public int RequestId
        {
            get;
            private set;
        }

        private int chunkSize = 64 * 1024;
        public int ChunkSize
        {
            get { return chunkSize; }
            set
            {
                if (value != chunkSize)
                {
                    chunkSize = value;
                }
            }
        }
        public bool IsStopped { get; set; }
        public HttpWebRequest OriginalRequest
        {
            get;
            private set;
        }

        private bool needDisposeWhenReset;
        private StreamCollection requestStreams;
        public StreamCollection RequestStreams
        {
            get
            {
                return requestStreams;
            }
            private set
            {
                Reset();

                requestStreams = value;
                RequestStreamEnumerator = new StreamBufferEnumerator(RequestId, requestStreams, requestStreams.Length, ChunkSize);
            }
        }

        public void SetRequestStream(Stream requestStream, string contentType, long contentLength = -1, bool disposeWhenReset = true)
        {
            RequestStreams = new StreamCollection(new Stream[] { requestStream });
            RequestStreams.Seek(0);
            needDisposeWhenReset = disposeWhenReset;

            LogHelper.OnlineLogger.Debug(string.Format("Set ContentLength={0},ContentType={1}", requestStream.Length, contentType));
            //OriginalRequest.ContentLength = contentLength == -1 ? RequestStreams.Length : contentLength;
            OriginalRequest.ContentType = contentType;
            
            //OriginalRequest.AllowWriteStreamBuffering = false;
        }

        public void SetRequestStream(StreamCollection requestStreams, string contentType, long contentLength = -1, bool disposeWhenReset = true)
        {
            RequestStreams = requestStreams;

            RequestStreams.Seek(0);

            needDisposeWhenReset = disposeWhenReset;

            LogHelper.OnlineLogger.Debug(string.Format("Set ContentLength={0},ContentType={1}", requestStreams.Length, contentType));
            //OriginalRequest.ContentLength = contentLength == -1 ? RequestStreams.Length : contentLength;
            OriginalRequest.ContentType = contentType;

            //OriginalRequest.AllowWriteStreamBuffering = false;
        }

        public HttpResponse Response
        {
            get;
            set;
        }

        public object Context
        {
            get;
            set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public StreamBufferEnumerator RequestStreamEnumerator
        {
            get;
            private set;
        }

        public List<KeyValuePair<string, string>> Headers
        {
            get;
            set;
        }

        public bool IsTimeout
        {
            get;
            private set;
        }

        public int ProxyId
        {
            get;
            set;
        }

        public HttpRequest(string uri, string method = HttpConst.HttpMethod_Post,
                           bool allowReadBuffering = true, bool allowWriteBuffering = true,
                           int bufferSize = 64 * 1024)
        {
            RequestId = r.Next(1000000);

            OriginalRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            /*try
            {
                OriginalRequest = HttpWebRequest.Create(uri) as HttpWebRequest;
            }
            catch (UriFormatException ex)
            {
                LogHelper.OnlineLogger.Error("Cannot create Http request " + ex.ToString());

                OriginalRequest = HttpWebRequest.Create("http://localhost/biu/error/") as HttpWebRequest;
            }*/
            //OriginalRequest.AllowReadStreamBuffering = allowReadBuffering;

            OriginalRequest.Method = method;
            ChunkSize = bufferSize;

        }

        //Close request stream
        public void Close()
        {
            Reset();
        }

        public void TimeoutAbort()
        {
            IsTimeout = true;
            //We can abort request here, no deadlock would happen
            OriginalRequest.Abort();
            Stop();
        }

        public void Stop()
        {
            IsStopped = true;
        }

        private void Reset()
        {
            if (RequestStreamEnumerator != null)
            {
                RequestStreamEnumerator.Dispose();
                RequestStreamEnumerator = null;
            }
            if (needDisposeWhenReset && requestStreams != null)
            {
                requestStreams.Dispose();
                requestStreams = null;
            }
        }

        public void RecreateHttpRequest()
        {
            var tempReq = OriginalRequest;
            OriginalRequest = HttpWebRequest.Create(tempReq.RequestUri) as HttpWebRequest;

            //OriginalRequest.AllowReadStreamBuffering = tempReq.AllowReadStreamBuffering;

            OriginalRequest.Method = tempReq.Method;
        }
        #region Events

        public event EventHandler<HttpProgressEventArgs> OnProgressing;
        public event EventHandler<HttpReadyEventArgs> OnReady;
        public event EventHandler<EventArgs> OnResponsed;

        internal void ReportProgress(long offset, long total, int latestSize)
        {
            if (OnProgressing != null)
                OnProgressing(this, new HttpProgressEventArgs(offset, total, latestSize));
        }

        internal void ReportReady(ReadyState state)
        {
            if (OnReady != null)
                OnReady(this, new HttpReadyEventArgs(state));
        }

        internal void ReportResponsed()
        {
            if (OnResponsed != null)
                OnResponsed(this, null);
        }
        #endregion
    }

    class HttpResponse : IDisposable
    {
        public int ResponseId
        {
            get;
            private set;
        }

        //protected static StreamAccessor streamCache = new StreamAccessor("HttpRequest", true);
        //private const int CacheValveSize = 1024 * 100;
        public bool IsStopped { get; set; }

        public HttpWebResponse OriginalResponse
        {
            get;
            private set;
        }

        //private string streamKey;
        public Stream ResponseStream
        {
            get;
            protected set;
        }

        public Exception Exception
        {
            get;
            set;
        }

        public HttpStatusCode StatusCode
        {
            get;
            internal set;
        }

        internal HttpResponse(HttpWebResponse response, int responseId)
        {
            ResponseId = responseId;
            OriginalResponse = response;
            StatusCode = response.StatusCode;

            CreateResponseStream();
        }

        internal virtual void DeseralizeFromStream()
        {
        }

        public void Dispose()
        {
            if (OriginalResponse != null)
            {
                OriginalResponse.Dispose();
                OriginalResponse = null;
            }

            if (ResponseStream != null)
            {
                ResponseStream.Dispose();
                ResponseStream = null;
                /*if (streamKey != null)
                {
                    streamCache.ClearAsync(streamKey, StreamType.File).Wait();
                }*/
            }
        }

        private void CreateResponseStream()
        {
            ResponseStream = new MemoryStream();
            /*if (OriginalResponse.ContentLength > CacheValveSize)
            {
                streamCache.CreateStream(GetResponseKey(), StreamType.File, (byte[])null, (int)OriginalResponse.ContentLength).Wait();
                var loadStreamTask = streamCache.LoadStreamAsync(streamKey, StreamType.File);
                loadStreamTask.Wait();
                ResponseStream = loadStreamTask.Result;
            }
            else
            {
                ResponseStream = new MemoryStream((int)OriginalResponse.ContentLength);
            }*/
        }

        /*private string GetResponseKey()
        {
            int hash = OriginalResponse.ResponseUri.AbsoluteUri.GetHashCode();
            string name = Path.GetFileName(OriginalResponse.ResponseUri.AbsolutePath);
            string key = name + hash.ToString("X");
            return key;
        }*/

        public void CleanReponseStream()
        {
            if (ResponseStream != null)
                ResponseStream.Dispose();

            CreateResponseStream();
        }

        #region Events
        public event EventHandler<HttpProgressEventArgs> OnProgressing;
        public event EventHandler<HttpReadyEventArgs> OnReady;

        internal void ReportProgress(long offset, long total, int latestSize)
        {
            if (OnProgressing != null)
                OnProgressing(this, new HttpProgressEventArgs(offset, total, latestSize));
        }

        internal void ReportReady(ReadyState state)
        {
            if (OnReady != null)
                OnReady(this, new HttpReadyEventArgs(state));
        }
        #endregion
    }
}
