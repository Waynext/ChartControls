using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebRequest.Requests;
using WebRequest.Requests.StockData;

namespace WebRequest
{
    public enum Market { SH, SZ, BS };
    public enum StockType { Simple, SimpleVolmun, Complete };

    public class StockFetcher
    {
        private static object instanceLocker = new object();

        public static StockFetcher Instance
        {
            get;
            private set;
        }

        public static void CreateStockFetcher(string serverUri)
        {
            if (Instance == null)
            {
                lock (instanceLocker)
                {
                    if(Instance == null)
                        Instance = new StockFetcher(serverUri);
                }
            }
        }

        private StockFetcher() { }
        private StockFetcher(string serverUri)
        {
            StockWebStatus.WebServerUri = serverUri;
        }

        public async Task<SimpleShare> GetStockAsync(Market market, string id, DateTime start, DateTime end, StockType stockType, string conext)
        {
            GetStockRequest req = new GetStockRequest(market, id, start, end, stockType, conext);
            await RequestQueueFactory.Instance.SendRequestAsync(req);

            var response = req.Response as GetStockResponse;
            return response.Stock;
        }
    }
}
