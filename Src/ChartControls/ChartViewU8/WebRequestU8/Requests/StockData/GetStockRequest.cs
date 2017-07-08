using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebRequest.Http;
using ULog;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Common;

namespace WebRequest.Requests.StockData
{
    class GetStockRequest : DataRequest
    {
        private const string UriTemplate = "/Data/GetShareData?market={0}&id={1}&type={2}";

        private StockType shareType;
        public GetStockRequest(Market market, string id, DateTime? start, DateTime? end, StockType shareType, string context)
            : base(RequestPriority.Normal)
        {
            Uri = string.Format(StockWebStatus.WebServerUri + UriTemplate, market, id, shareType);
            if (start != null)
            {
                Uri += ("&start=" + start.Value.ToString("yyyyMMdd"));
            }

            if (end != null)
            {
                Uri += ("&end=" + end.Value.ToString("yyyyMMdd"));
            }

            if (!string.IsNullOrEmpty(context))
            {
                Uri += ("&context=" + context);
            }

            this.shareType = shareType;
        }

        internal override void CreateResponse()
        {
            Response = new GetStockResponse(shareType);
        }
    }

    class GetStockResponse : DataResponse
    {
        public SimpleShare Stock
        {
            get;
            private set;
        }

        public StockType StockType
        {
            get;
            private set;
        }

        public GetStockResponse(StockType stockType)
        {
            StockType = stockType;
        }


        internal override void Decode()
        {
            base.Decode();

            if (IsSucceeded)
            {
                JSonResponse jsonResponse = this.HttpResponse as JSonResponse;
                if (jsonResponse != null && jsonResponse.Object != null)
                {
                    switch(StockType)
                    {
                        case StockType.Simple:
                            Stock = jsonResponse.Object.ToObject<SimpleShare>();
                            break;
                        case StockType.SimpleVolmun:
                            Stock = jsonResponse.Object.ToObject<SimpleShareVolumn>();
                            break;
                        case StockType.Complete:
                            Stock = jsonResponse.Object.ToObject<CompleteShare>();
                            break;
                    }
                }
            }
        }

        
    }
}
