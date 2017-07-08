using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartControls;
using WebRequest;
using Common;

namespace ChartViewU8
{
    class DataController
    {
        public static DataController Instance
        {
            get;
            private set;
        }

        public static void CreateDataController(string stockServerUri)
        {
            if (Instance == null)
            {
                Instance = new DataController(stockServerUri);
            }
        }

        private DataController()
        {
        }

        private DataController(string stockServerUri)
        {
            StockFetcher.CreateStockFetcher(stockServerUri);
            fetcher = StockFetcher.Instance;
        }

        private StockFetcher fetcher;

        public async Task<List<ChartItem>> GetChartItems(string market, string id, DateTime start, DateTime end)
        {
            var simpleStock = await fetcher.GetStockAsync(FromMarket(market), id, start, end, StockType.Simple, null);

            return FromSimpleStock(simpleStock);
        }

        public async Task<StockVolumnList> GetStockItems(string market, string id, DateTime start, DateTime end)
        {
            var simpleStock = await fetcher.GetStockAsync(FromMarket(market), id, start, end, StockType.Complete, null);

            return FromCompleteStock(simpleStock as CompleteShare);
        }

        private Market FromMarket(string market)
        {
            Market m;
            if (!Enum.TryParse<Market>(market, out m))
            {
                m = Market.SH;
            }

            return m;
        }

        private List<ChartItem> FromSimpleStock(SimpleShare s)
        {
            List<ChartItem> ctList = null;
            if (s.Dates != null)
            {
                ctList = new List<ChartItem>(s.Dates.Count);

                double preClose = 0;
                for (int i = 0; i < s.Dates.Count; i++)
                {
                    ctList.Add(new ChartItem()
                    {
                        Date = s.Dates[i],
                        Value = s.Closes[i],
                        ValueChange = i != 0 ? (s.Closes[i] - preClose) / preClose : 0
                    });

                    preClose = s.Closes[i];
                }
            }

            return ctList;
        }

        private StockVolumnList FromCompleteStock(CompleteShare s)
        {
            StockVolumnList svList = new StockVolumnList();

            if (s.Dates != null)
            {
                svList.Prices = new List<StockItem>(s.Dates.Count);
                svList.Volumns = new List<VolumnItem>(s.Dates.Count);

                double preClose = 0;
                for (int i = 0; i < s.Dates.Count; i++)
                {
                    var sItem = new StockItem()
                    {
                        Date = s.Dates[i],
                        Value = s.Closes[i],
                        ValueChange = i != 0 ? (s.Closes[i] - preClose) / preClose : 0,
                        High = s.Highs[i],
                        Low = s.Lows[i],
                        Open = s.Opens[i]
                    };

                    svList.Prices.Add(sItem);

                    svList.Volumns.Add(new VolumnItem()
                    {
                        Date = s.Dates[i],
                        Value = s.Volumns[i],
                        Turnover = s.Turnovers[i],
                        IsRaise = s.Closes[i] > s.Opens[i] || (s.Closes[i] == s.Opens[i] && sItem.CloseChange > 0)
                    });
                    preClose = s.Closes[i];
                }
            }

            return svList;
        }
    }
}
