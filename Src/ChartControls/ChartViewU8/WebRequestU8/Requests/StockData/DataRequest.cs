using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRequest.Common;
using WebRequest.Http;
using ULog;

namespace WebRequest.Requests.StockData
{
    class DataRequest : Request
    {
        protected string httpMethod;

        public Stream RequestStream { get; protected set; }

        public DataRequest(RequestPriority prio, string method = HttpConst.HttpMethod_Get)
            : base(prio)
        {
            httpMethod = method;
            //Set uri in child class
            RequestStream = null;
        }

        internal override void Encode()
        {
            if (httpMethod == HttpConst.HttpMethod_Get)
            {
                this.HttpRequest = new HttpRequest(Uri, httpMethod);
            }
            else if (httpMethod == HttpConst.HttpMethod_Post || httpMethod == HttpConst.HttpMethod_Put)
            {
                if (RequestStream != null)
                {
                    this.HttpRequest = new HttpRequest(Uri, httpMethod, true, false);

                    HttpRequest.SetRequestStream(RequestStream, HttpLayer.StreamContentType);
                }
                else
                {
                    var jObj = CreateRequestBody();
                    this.HttpRequest = new JSonRequest(Uri, jObj, httpMethod);
                }
            }
            else
            {
                LogHelper.OnlineLogger.Error("Unsupported http method");
            }

            if (requestHeaders != null && HttpRequest != null)
            {
                this.HttpRequest.Headers = requestHeaders;
            }

        }

        protected virtual JObject CreateRequestBody() { return new JObject(); }

        internal override void CreateResponse()
        {
            Response = new DataResponse();
        }
    }

    class DataResponse : Response
    {
        public int Code
        {
            get;
            private set;
        }

        public Error Error
        {
            get;
            private set;
        }

        protected void GetIsSucceed()
        {
            Code = (int)HttpResponse.StatusCode;
            IsSucceeded = Code < 300 && Exception == null;
        }

        internal override void Decode()
        {
            try
            {
                JSonResponse jsonResponse = HttpResponse as JSonResponse;

                GetIsSucceed();

                if(jsonResponse != null && jsonResponse.Object != null)
                {
                    LogHelper.OnlineLogger.Debug(jsonResponse.Object.ToString());
                    if (!IsSucceeded)
                    {
                        Error = jsonResponse.Object.ToObject<Error>();
                    }
                }
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
        }
    }
}
