using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using MyParser;
using MyParser.Dzh2;
using System.Diagnostics;
using ChartView.Properties;
using Newtonsoft.Json;
using ChartControls;
using ChartControls.Drawing;
using System.Threading;

namespace ChartView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IPen raisePen, fallPen, contrastPen;
        private MainViewModel ViewModel;
        public MainWindow()
        {
            InitializeComponent();

            raisePen = DrawingObjectFactory.CreatePen(Brushes.Red, 1);
            fallPen = DrawingObjectFactory.CreatePen(Brushes.Green, 1);

            contrastPen = DrawingObjectFactory.CreatePen(Brushes.Black, 1);

            DataContext = ViewModel = new MainViewModel();

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        int mvCount = 6;
        int mvStart = 0;
        int[] mvUnits = new int[] { 10, 20, 30, 60, 120, 250 };
        //{ 10, 20, 30, 60, 120, 250};
        Brush[] mvBrushs = new Brush[] { Brushes.Salmon, Brushes.Yellow, Brushes.Orange, Brushes.Blue, Brushes.Orange, Brushes.Purple };
        //{ Colors.Black, Colors.Yellow, Colors.Red, Colors.Blue, Colors.Orange, Colors.Purple };
        const string shSzMarket = "shsz";
        const string shMarket = "sh";
        const string szMarket = "sz";
        const string customMarket = "custom";

        private DZHFolderHelp helper;
        private Stock currentStock;
        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            helper = new DZHFolderHelp(Settings.Default.DzhFolder, false);
            //ViewModel.IsTime = true;
            //QueryTime();
            //QueryCandleRunning();
            //QueryTimeRunning2();
            QueryStock();

        }

        JsonSerializerSettings settings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore
        };

        private void price_DataQueried(object sender, QueryDataEventArgs eArgs)
        {
            Debug.WriteLine("price\r\n" + JsonConvert.SerializeObject(eArgs.QueryData, settings));
            Notify(sender, eArgs, true);
        }

        private async void volumn_DataQueried(object sender, QueryDataEventArgs eArgs)
        {
            Debug.WriteLine("volumn\r\n" + JsonConvert.SerializeObject(eArgs.QueryData, settings));

            Notify(sender, eArgs, false);
        }

        private ChartItem CandleData2Item(CandleData cd, CandleData? cdPre, bool isStock)
        {
            if (isStock)
            {
                return new StockItem()
                {
                    Date = cd.DateTime,
                    High = cd.high,
                    Low = cd.low,
                    Open = cd.open,
                    Close = cd.close,
                    CloseChange = cdPre != null ? (cd.close - cdPre.Value.close) / cdPre.Value.close : 0
                };
            }
            else
            {
                return new VolumnItem()
                {
                    Date = cd.DateTime,
                    Volumn = cd.amount,
                    Turnover = cd.money,
                    IsRaise = cd.open < cd.close
                };
            }
        }

        private void Notify(object sender, QueryDataEventArgs eArgs, bool isStock)
        {
            ChartControl host = sender as ChartControl;
            QueryDataResult<ChartItem> qResult = new QueryDataResult<ChartItem>()
            {
                QueryId = eArgs.QueryData.QueryId
            };

            if (eArgs.QueryData.HeadDate != null)
            {
                bool isHeadEnd = false;
                var dataList = CopyStock(eArgs.QueryData.HeadCount.Value, eArgs.QueryData.HeadDate.Value, out isHeadEnd);


                qResult.CollectionId = eArgs.QueryData.CollectionId;
                qResult.IsHeadIncluded = true;
                qResult.HeadItems = dataList != null ? new List<ChartItem>(dataList.Count) : null;
                qResult.IsHeadEnd = isHeadEnd;

                if (dataList != null)
                {
                    CandleData? cdPre = null;
                    foreach (var cd in dataList)
                    {
                        var cItem = CandleData2Item(cd, cdPre, isStock);
                        cdPre = cd;
                        qResult.HeadItems.Add(cItem);
                    }
                }
            }

            if (eArgs.QueryData.TailDate != null)
            {
                bool isTailEnd = false;
                var dataList = CopyStock(eArgs.QueryData.TailDate.Value, eArgs.QueryData.TailCount.Value, out isTailEnd);

                qResult.CollectionId = eArgs.QueryData.CollectionId;
                qResult.IsTailIncluded = true;
                qResult.TailItems = dataList != null ? new List<ChartItem>(dataList.Count) : null;
                qResult.IsTailEnd = isTailEnd;

                CandleData? cdPre = null;

                if (dataList != null)
                {
                    foreach (var cd in dataList)
                    {
                        var cItem = CandleData2Item(cd, cdPre, isStock);
                        cdPre = cd;
                        qResult.TailItems.Add(cItem);
                    }
                }
            }


            qResult.IsSucceeded = true;

            host.QueryFinished(qResult);
        }

        private Task<Stock> LoadStock(string id)
        {
            return Task<Stock>.Run(() =>
            {
                var parser = helper.GetDayParser("SH");

                currentStock = parser.GetOneStock(id, new DateTime(2015, 5, 29).AddYears(-30));

                return currentStock;
            });
        }

        private IList<CandleData> CopyStock(DateTime start, int count, out bool isEnd)
        {
            isEnd = true;
            var stock = ViewModel.LoadFullStock(ViewModel.ShareId);
            int index = stock.GetIndex(start);
            if (index == -1)
                return null;

            if (stock.Data[index].DateTime == start)
            {
                index++;
            }

            if (index + count >= stock.DataCount)
            {
                count = stock.DataCount - index;
                isEnd = true;
            }
            else
            {
                isEnd = false;
            }

            if (count != 0)
                return stock.GetRange(index, count).ToList();
            else
                return null;
        }

        private IList<CandleData> CopyStock(int count, DateTime end, out bool isEnd)
        {
            isEnd = true;

            var stock = ViewModel.LoadFullStock(ViewModel.ShareId);
            int index = stock.GetIndex(end);
            if (index == -1)
                return null;

            index = index - count;
            if (index <= 0)
            {
                count += index;
                index = 0;

                isEnd = true;
            }
            else
            {
                isEnd = false;
            }

            if (count != 0)
                return stock.GetRange(index, count).ToList();
            else
                return null;
        }

        private bool isDynamic = false;
        private void CreateChartView(Stock stock, string marketId)
        {
            if (stock != null)
            {
                var closeList = stock.Items.Select(cd => new ChartItem() { Value = cd.close, Date = cd.DateTime }).ToList();
                CollectionId id = new CollectionId(stock.id, marketId);
                ChartItemCollection coll = new ChartItemCollection(id, closeList, raisePen, null, true, isDynamic);

                price.SetMainCollection(coll);

                for (int i = mvStart; i < mvStart + mvCount; i++)
                {
                    var maPeriod = mvUnits[i];
                    var ma60 = stock.MA(maPeriod);

                    List<ChartItem> mv60Items = new List<ChartItem>(ma60.Count);
                    for (int j = 0; j < ma60.Count; j++)
                    {
                        mv60Items.Add(new ChartItem() { Value = ma60[j], Date = stock.Data[j + maPeriod - 1].DateTime });
                    }

                    id = new CollectionId(stock.id, marketId);
                    ChartItemCollection collMa60 = new ChartItemCollection(id, mv60Items,
                        DrawingObjectFactory.CreatePen(mvBrushs[i], 1), null, true, isDynamic);

                    price.AddAssistCollection(collMa60);
                }

                price.ForceDraw();
            }
        }

        private void CreateChartView2(IEnumerable<Stock> stocks, string marketId)
        {
            ChartItemCollection main = null;
            foreach (var stock in stocks)
            {
                var closeList = new List<ChartItem>();
                CandleData cdPre = stock.Items.FirstOrDefault();
                foreach (var cd in stock.Items)
                {
                    closeList.Add(new ChartItem()
                    {
                        Value = cd.close,
                        Date = cd.DateTime,
                        ValueChange = (cd.close - cdPre.close) / cdPre.close
                    });

                    cdPre = cd;
                }

                IPen pen;
                if (main == null)
                {
                    pen = raisePen;
                }
                else
                {
                    pen = fallPen;
                }

                CollectionId id = new CollectionId(stock.id, marketId);
                ChartItemCollection coll = new ChartItemCollection(id, closeList, pen, null, true, isDynamic);

                if (main == null)
                {
                    main = coll;

                    price.SetMainCollection(coll);
                }
                else
                {

                    price.AddAssistCollection(coll, true);
                }
            }

            price.YScaleDock = YScaleDock.Left;
            volumn.YScaleDock = YScaleDock.Left;
            price.AddConnection(volumn);

            price.ForceDraw();
        }

        private void CreateCandleView2(IEnumerable<Stock> stocks, string marketId)
        {
            ChartItemCollection main = null;
            foreach (var stock in stocks)
            {
                var closeList = new List<StockItem>();
                var volList = new List<VolumnItem>();
                CandleData cdPre = stock.Items.FirstOrDefault();
                foreach (var cd in stock.Items)
                {
                    closeList.Add(new StockItem()
                    {
                        Value = cd.close,
                        Date = cd.DateTime,
                        High = cd.high,
                        Low = cd.low,
                        Open = cd.open,
                        ValueChange = (cd.close - cdPre.close) / cdPre.close
                    });

                    cdPre = cd;

                    if (main == null)
                    {
                        volList.Add(new VolumnItem()
                        {
                            Date = cd.DateTime,
                            IsRaise = cd.open <= cd.close,
                            Volumn = cd.amount,
                            Turnover = cd.money
                        });
                    }
                }

                IPen pen;
                if (main == null)
                {
                    pen = raisePen;
                }
                else
                {
                    pen = fallPen;
                }

                CollectionId id = new CollectionId(stock.id, marketId);
                if (main == null)
                {

                    StockItemCollection coll = new StockItemCollection(id, closeList, pen, pen, null, isDynamic);
                    coll.ItemStyle = StockItemStyle.Candle;
                    main = coll;

                    price.SetMainCollection(coll);

                    VolumnItemCollection volColl = new VolumnItemCollection(id, volList, pen, pen, isDynamic);

                    volumn.SetMainCollection(volColl);
                }
                else {
                    ChartItemCollection coll = new ChartItemCollection(id, closeList, pen, null, true, isDynamic);

                    price.AddAssistCollection(coll, true);
                }
            }

            price.YScaleDock = YScaleDock.InnerLeft;
            volumn.YScaleDock = YScaleDock.InnerLeft;
            price.AddConnection(volumn);

            price.ForceDraw();


        }

        private void CreateCandleView(Stock stock, string marketId)
        {
            if (stock != null)
            {
                var closeList = new List<StockItem>();
                var volList = new List<VolumnItem>();
                CandleData cdPre = stock.Items.FirstOrDefault();
                foreach (var cd in stock.Items)
                {
                    closeList.Add(new StockItem()
                    {
                        Value = cd.close,
                        Date = cd.DateTime,
                        High = cd.high,
                        Low = cd.low,
                        Open = cd.open,
                        CloseChange = (cd.close - cdPre.close) / cdPre.close
                    });

                    cdPre = cd;

                    volList.Add(new VolumnItem()
                    {
                        Date = cd.DateTime,
                        IsRaise = cd.open <= cd.close,
                        Volumn = cd.amount,
                        Turnover = cd.money
                    });
                }

                CollectionId id = new CollectionId(stock.id, marketId);
                StockItemCollection coll = new StockItemCollection(id, closeList, raisePen, fallPen, null, isDynamic);
                //coll.ItemStyle = StockItemStyle.Linear;

                price.SetMainCollection(coll);

                for (int i = mvStart; i < mvStart + mvCount; i++)
                {
                    var maPeriod = mvUnits[i];
                    var ma60 = stock.MA(maPeriod);

                    List<ChartItem> mv60Items = new List<ChartItem>(ma60.Count);
                    for (int j = 0; j < ma60.Count; j++)
                    {
                        mv60Items.Add(new ChartItem() { Value = ma60[j], Date = stock.Data[j + maPeriod - 1].DateTime });
                    }

                    id = new CollectionId(stock.id, marketId);
                    ChartItemCollection collMa60 = new ChartItemCollection(id, mv60Items, DrawingObjectFactory.CreatePen(mvBrushs[i], 1), null, true);

                    price.AddAssistCollection(collMa60);
                }

                id = new CollectionId(stock.id, marketId);
                VolumnItemCollection volColl = new VolumnItemCollection(id, volList, raisePen, fallPen, isDynamic);
                //volColl.VolumnItemStyle = VolumnItemStyle.Linear;
                volumn.SetMainCollection(volColl);


            }
            else
            {
                price.SetMainCollection(null);
                volumn.SetMainCollection(null);
            }
            price.YScaleDock = YScaleDock.Left;
            volumn.YScaleDock = YScaleDock.Left;
            price.AddConnection(volumn);

            price.ForceDraw();
        }

        /*private void CreateCandleView()
        {
            Stock stock = ...;

            var closeList = new List<ChartItem>();
            var volList = new List<ChartItem>();
            CandleData cdPre = stock.Items.First();
            foreach (var cd in stock.Items)
            {
                closeList.Add(new StockItem()
                {
                    Close = cd.close,
                    Date = cd.DateTime,
                    High = cd.high,
                    Low = cd.low,
                    Open = cd.open,
                    CloseChange = (cd.close - cdPre.close) / cdPre.close
                });

                cdPre = cd;

                volList.Add(new VolumnItem()
                {
                    Date = cd.DateTime,
                    IsRaise = cd.open <= cd.close,
                    Volumn = cd.amount,
                    Turnover = cd.money
                });
            }

            string stockId = "000001";
            string marketId = "SH";

            CollectionId id = new CollectionId(stockId, marketId);
            StockItemCollection coll = new StockItemCollection(id, closeList, raisePen, fallPen, null, false);
            price.SetMainCollection(coll);

            id = new CollectionId(stockId, marketId);
            VolumnItemCollection volColl = new VolumnItemCollection(id, volList, raisePen, fallPen, false);
            volumn.SetMainCollection(volColl);
            price.AddConnection(volumn);
            price.ForceDraw();
        }*/
        private void CreateCandleViewInMultipleChartItemCollection(Stock stock, string marketId)
        {
            if (stock != null)
            {
                var multItemList = new List<MultipleChartItem>();
                var volList = new List<VolumnItem>();

                CandleData cdPre = stock.Items.FirstOrDefault();
                foreach (var cd in stock.Items)
                {
                    multItemList.Add(new MultipleChartItem()
                    {
                        Value = cd.close,
                        Date = cd.DateTime,
                        ValueChange = (cd.close - cdPre.close) / cdPre.close,
                        Values = new List<double>(mvCount)
                    });

                    cdPre = cd;

                    volList.Add(new VolumnItem()
                    {
                        Date = cd.DateTime,
                        IsRaise = cd.open <= cd.close,
                        Volumn = cd.amount,
                        Turnover = cd.money
                    });
                }



                for (int i = mvStart; i < mvStart + mvCount; i++)
                {
                    var maPeriod = mvUnits[i];
                    var maN = stock.MA(maPeriod);

                    int iInvalidCount = maPeriod - 1;

                    for (int j = 0; j < iInvalidCount; j++)
                    {
                        var mItem = multItemList[j];
                        mItem.Values.Add(ChartItemCollection.valueNA);
                    }

                    List<ChartItem> mv60Items = new List<ChartItem>(maN.Count);
                    for (int j = 0; j < maN.Count; j++)
                    {
                        var mItem = multItemList[j + iInvalidCount];
                        mItem.Values.Add(maN[j]);
                    }
                }
                CollectionId id = new CollectionId(stock.id, marketId);
                List<IPen> pens = new List<IPen>();
                pens.Add(DrawingObjectFactory.CreatePen(Brushes.Black, 1));
                pens.AddRange(mvBrushs.Skip(mvStart).Take(mvCount).Select(mvBrush => DrawingObjectFactory.CreatePen(mvBrush, 1)));
                MultipleChartItemCollection multipleColl = new MultipleChartItemCollection(id, multItemList, pens.ToArray(), true, false);
                price.SetMainCollection(multipleColl);

                VolumnItemCollection volColl = new VolumnItemCollection(id, volList, raisePen, fallPen, isDynamic);
                //volColl.VolumnItemStyle = VolumnItemStyle.Linear;
                volumn.SetMainCollection(volColl);
            }
            else
            {
                price.SetMainCollection(null);
                volumn.SetMainCollection(null);
            }
            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();
        }

        double[] invalidValues = null;
        private List<MultipleChartItem> FromStock(Stock stock, int valueCount = 0)
        {
            if (valueCount != 0)
            {
                invalidValues = new double[valueCount];
                for (int i = 0; i < valueCount; i++)
                {
                    invalidValues[i] = ChartItemCollection.valueNA;
                }
            }

            List<MultipleChartItem> cItems = new List<MultipleChartItem>(stock.DataCount);
            CandleData cdPre = stock.Items.FirstOrDefault();
            foreach (var cd in stock.Items)
            {
                cItems.Add(new MultipleChartItem()
                {
                    Value = cd.close,
                    Date = cd.DateTime,
                    ValueChange = (cd.close - cdPre.close) / cdPre.close,
                    Values = valueCount != 0 ? new List<double>(invalidValues) : null,
                    ValueChanges = valueCount != 0 ? new List<double>(invalidValues) : null
                });

                cdPre = cd;
            }

            return cItems;
        }

        private List<MultipleChartItem> Combine(List<MultipleChartItem> one, List<MultipleChartItem> other, int j)
        {
            ChartItemComparer comparar = new ChartItemComparer();
            int iOne = 0;
            int iOther = 0;
            for (; iOther < other.Count; iOther++)
            {
                if (iOne >= one.Count)
                {
                    one.AddRange(other.Skip(iOther).Select(mItem =>
                    {
                        var tempItem = new MultipleChartItem()
                        {
                            Value = ChartItemCollection.valueNA,
                            Date = other[iOther].Date,
                            ValueChange = ChartItemCollection.valueNA,
                            Values = one[0].Values != null ? new List<double>(invalidValues) : null,
                            ValueChanges = one[0].ValueChanges != null ? new List<double>(invalidValues) : null
                        };

                        tempItem.Values[j] = mItem.Value;
                        tempItem.ValueChanges[j] = mItem.ValueChange;

                        return tempItem;
                    }));
                    break;
                }

                if (one[iOne].Date == other[iOther].Date)
                {
                    one[iOne].Values[j] = other[iOther].Value;
                    one[iOne].ValueChanges[j] = other[iOther].ValueChange;
                    iOne++;
                }
                else if (one[iOne].Date > other[iOther].Date)
                {
                    var tempItem = new MultipleChartItem()
                    {
                        Value = ChartItemCollection.valueNA,
                        Date = other[iOther].Date,
                        ValueChange = ChartItemCollection.valueNA,
                        Values = one[0].Values != null ? new List<double>(invalidValues) : null,
                        ValueChanges = one[0].ValueChanges != null ? new List<double>(invalidValues) : null
                    };

                    tempItem.Values[j] = other[iOther].Value;
                    tempItem.ValueChanges[j] = other[iOther].ValueChange;

                    one.Insert(iOne, tempItem);
                    iOne++;
                }
                else
                {
                    int iOneTemp = one.BinarySearch(iOne, one.Count - iOne, other[iOther], comparar);
                    if (iOneTemp < 0)
                    {
                        iOneTemp = ~iOneTemp;

                        if (iOneTemp == one.Count)
                        {
                            one.AddRange(other.Skip(iOther).Select(mItem =>
                            {
                                var tempItem = new MultipleChartItem()
                                {
                                    Value = ChartItemCollection.valueNA,
                                    Date = other[iOther].Date,
                                    ValueChange = ChartItemCollection.valueNA,
                                    Values = one[0].Values != null ? new List<double>(invalidValues) : null,
                                    ValueChanges = one[0].ValueChanges != null ? new List<double>(invalidValues) : null
                                };

                                tempItem.Values[j] = mItem.Value;
                                tempItem.ValueChanges[j] = mItem.ValueChange;

                                return tempItem;
                            }));
                            break;
                        }
                        else
                        {
                            var tempItem = new MultipleChartItem()
                            {
                                Value = ChartItemCollection.valueNA,
                                Date = other[iOther].Date,
                                ValueChange = ChartItemCollection.valueNA,
                                Values = one[0].Values != null ? new List<double>(invalidValues) : null,
                                ValueChanges = one[0].ValueChanges != null ? new List<double>(invalidValues) : null
                            };
                            tempItem.Values[j] = other[iOther].Value;
                            tempItem.ValueChanges[j] = other[iOther].ValueChange;

                            one.Insert(iOneTemp, tempItem);
                            iOne = iOneTemp + 1;
                        }
                    }
                    else
                    {
                        one[iOneTemp].Values[j] = other[iOther].Value;
                        one[iOneTemp].ValueChanges[j] = other[iOther].ValueChange;
                        iOne = iOneTemp + 1;
                    }
                }
            }

            return one;
        }
        private List<MultipleChartItem> CreateFrom(List<Stock> stocks)
        {
            var firstStock = stocks[0];
            List<MultipleChartItem> cItems = new List<MultipleChartItem>(FromStock(firstStock, stocks.Count - 1));

            for (int i = 1; i < stocks.Count; i++)
            {
                var tempItems = FromStock(stocks[i]);

                Combine(cItems, tempItems, i - 1);
            }

            return cItems;
        }

        private void CreateChartViewInMultipleChartItemCollectionPercentage(List<Stock> stocks, string marketId)
        {
            if (stocks != null)
            {
                var multItemList = CreateFrom(stocks);

                CollectionId id = new CollectionId(stocks.FirstOrDefault().id, marketId);
                List<IPen> pens = new List<IPen>();
                pens.Add(DrawingObjectFactory.CreatePen(Brushes.Black, 1));
                pens.AddRange(mvBrushs.Skip(mvStart).Take(mvCount).Select(mvBrush => DrawingObjectFactory.CreatePen(mvBrush, 1)));
                MultipleChartItemCollection multipleColl = new MultipleChartItemCollection(id, multItemList, pens.ToArray(), true, false);
                price.SetMainCollection(multipleColl);
            }
            else
            {
                price.SetMainCollection(null);
                volumn.SetMainCollection(null);
            }
            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();
        }

        private void CreateCandleViewInSignleStockItemCollection(Stock stock, string marketId)
        {
            if (stock != null)
            {
                var multItemList = new List<StockValuesItem>();
                var volList = new List<VolumnItem>();
                CandleData cdPre = stock.Items.FirstOrDefault();
                foreach (var cd in stock.Items)
                {
                    multItemList.Add(new StockValuesItem()
                    {
                        Value = cd.close,
                        Date = cd.DateTime,
                        High = cd.high,
                        Low = cd.low,
                        Open = cd.open,
                        CloseChange = (cd.close - cdPre.close) / cdPre.close,
                        Values = new List<double>(mvCount)
                    });

                    volList.Add(new VolumnItem()
                    {
                        Date = cd.DateTime,
                        IsRaise = cd.open <= cd.close,
                        Volumn = cd.amount,
                        Turnover = cd.money
                    });

                    cdPre = cd;
                }

                for (int i = mvStart; i < mvStart + mvCount; i++)
                {
                    var maPeriod = mvUnits[i];
                    var maN = stock.MA(maPeriod);

                    if (!maN.Any())
                        continue;

                    int iInvalidCount = maPeriod - 1;

                    for (int j = 0; j < iInvalidCount; j++)
                    {
                        var mItem = multItemList[j];
                        mItem.Values.Add(ChartItemCollection.valueNA);
                    }

                    List<ChartItem> mv60Items = new List<ChartItem>(maN.Count);
                    for (int j = 0; j < maN.Count; j++)
                    {
                        var mItem = multItemList[j + iInvalidCount];
                        mItem.Values.Add(maN[j]);
                    }
                }
                CollectionId id = new CollectionId(stock.id, marketId);
                List<IPen> pens = new List<IPen>(mvBrushs.Skip(mvStart).Take(mvCount).Select(mvBrush => DrawingObjectFactory.CreatePen(mvBrush, 1)));
                StockValuesItemCollection multipleColl = new StockValuesItemCollection(id, multItemList, raisePen, fallPen, null, pens.ToArray(), false);
                price.SetMainCollection(multipleColl);

                VolumnItemCollection volColl = new VolumnItemCollection(id, volList, raisePen, fallPen, isDynamic);
                //volColl.VolumnItemStyle = VolumnItemStyle.Linear;
                volumn.SetMainCollection(volColl);
            }
            else
            {
                price.SetMainCollection(null);
                volumn.SetMainCollection(null);
            }
            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();
        }

        private void price_CursorMoved(object sender, CursorMovedRoutedEventArgs e)
        {
            if (e != null && e.CurrentItems != null)
            {
                priceOutput.Text = string.Join(",", e.CurrentItems.Select(item =>
                {
                    if (item.Item != null)
                    {
                        var realItem = item.Item as StockItem;
                        if (realItem != null)
                        {
                            return string.Format("{0}|{1}", item.Id, item.Item);
                        }
                        else
                        {
                            var mRealItem = item.Item as MultipleChartItem;
                            if (mRealItem == null)
                            {
                                if (ViewModel.IsTime)
                                {
                                    return string.Format("{0}|{1}|{2:F2}|{3:P2}", item.Id, item.Item.Date.ToString("hh:mm:ss"), item.Item.Value, item.Item.ValueChange);
                                }
                                else
                                    return string.Format("{0}|{1}", item.Id, item.Item);
                            }
                            else
                            {
                                return string.Format("{0}|{1}", item.Id, item.Item);
                            }
                        }
                    }
                    else
                    {
                        return "null";
                    }

                }));
            }
        }

        private void volumn_CursorMoved(object sender, CursorMovedRoutedEventArgs e)
        {
            if (e != null && e.CurrentItems != null)
            {
                volumnOutput.Text = string.Join(",", e.CurrentItems.Select(item =>
                {
                    if (item.Item != null)
                    {
                        var realItem = item.Item as VolumnItem;
                        if (realItem != null)
                        {
                            return string.Format("{0}|{1}", item.Id, item.Item);
                        }
                    }
                    return "null";
                }));
            }
        }

        private async void OnClickQuery(object sender, RoutedEventArgs e)
        {
            QueryStock();
            //QueryTimeRunning();
            //QueryTime();
            //QueryTimeRunning2();
        }

        public void QueryStock()
        {
            if (ViewModel.IsTime)
            {
                QueryTime();
                return;
            }

            /*ViewModel.IsDynamic = true;

            StockItemCollection stockCollection = ViewModel.CreateCollection(ChartItemType.Candle, raisePen, fallPen) as StockItemCollection;
            //stockCollection.ItemStyle = StockItemStyle.Linear;
            ChartItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.Volumn, raisePen, fallPen);
            price.SetMainCollection(stockCollection);

            //var collections = ViewModel.CreateMVCollections();
            //foreach(var coll in collections)
                //price.AddAssistCollection(coll);

            volumn.SetMainCollection(volumnCollection);*/

            StockItemCollection stockCollection = ViewModel.CreateCollection(ChartItemType.Candle, raisePen, fallPen) as StockItemCollection;
            if (stockCollection == null)
                return;

            stockCollection.MaxItemXDistance = 50;
            

            //stockCollection.MinItemXDistance = 1.2;
            //stockCollection.ItemStyle = StockItemStyle.Linear;
            ChartItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.Volumn, raisePen, fallPen);
            volumnCollection.MaxItemXDistance = 50;
            //volumnCollection.MinItemXDistance = 1.2;

            price.SetMainCollection(stockCollection);

            var statisticCollection = ViewModel.CreateStatisticCollection(contrastPen);
            if (statisticCollection != null)
            {
                price.AddAssistCollection(statisticCollection, true);
                statisticCollection.MaxItemXDistance = 50;
            }
            //statisticCollection.MinItemXDistance = 1.2;

            volumn.SetMainCollection(volumnCollection);

            /*SingleStockItemCollection ssCollection = ViewModel.CreateSingleStockCollection(raisePen, fallPen, contrastPen);
            price.SetMainCollection(ssCollection);

            ChartItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.Volumn, raisePen, fallPen);
            volumn.SetMainCollection(volumnCollection);*/

            /*var msc = ViewModel.CreateMultipleStatisticCollection();
            price.SetMainCollection(msc);*/

            /*ChartItemCollection stockCollection = ViewModel.CreateCollection(ChartItemType.Linear, raisePen, fallPen);
            stockCollection.IsSymmetric = true;
            stockCollection.StartValue = 4500.0;
            //stockCollection.ItemStyle = StockItemStyle.Linear;
            VolumnItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.Volumn, raisePen, fallPen) as VolumnItemCollection;
            volumnCollection.IsSymmetric = true;

            price.SetMainCollection(stockCollection);
            volumn.SetMainCollection(volumnCollection);*/

            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            XRHistoryGraphics xrGraphic = new XRHistoryGraphics();
            price.AddExtraDataGraphic(xrGraphic);

            price.ForceDraw();
        }

        private void QueryTime()
        {
            ChartItemCollection timeCollection = ViewModel.CreateCollection(ChartItemType.Timely, raisePen, null);
            price.SetMainCollection(timeCollection);
            price.XScaleFormat = "HH:mm";

            ChartItemCollection atimeCollection = ViewModel.CreateCollection(ChartItemType.TimelyAverage, contrastPen, null);
            price.AddAssistCollection(atimeCollection);

            ChartItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.TimelyVolumn, raisePen, fallPen);
            volumn.XScaleFormat = "HH:mm";
            volumn.YCursorFormat = volumn.YScaleFormat = "N0";
            volumn.SetMainCollection(volumnCollection);

            price.AddConnection(volumn);

            price.ForceDraw();
        }

        DispatcherTimer timer;
        private TimeSpan interval = TimeSpan.FromSeconds(0.1);
        private void QueryCandleRunning()
        {
            if(timer != null)
            {
                timer.Stop();
            }

            StockItemCollection stockCollection = ViewModel.CreateCollection(ChartItemType.Candle, raisePen, fallPen) as StockItemCollection;
            stockCollection.MaxItemXDistance = 50;

            ChartItemCollection volumnCollection = ViewModel.CreateCollection(ChartItemType.Volumn, raisePen, fallPen);
            volumnCollection.MaxItemXDistance = 50;
            volumn.SetMainCollection(volumnCollection);

            price.SetMainCollection(stockCollection);

            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();

            bool isStarted = false;
            var std = ViewModel.LoadDetailDatas();
            var tdds = std.Data;

            if (tdds != null && tdds.Count > 10)
            {

                timer = new DispatcherTimer();
                timer.Interval = interval;
                int i = 0;
                timer.Tick += (s, e) =>
                {
                    if (i == tdds.Count)
                    {
                        timer.Stop();
                        return;
                    }
                
                    if (!isStarted)
                    {
                        var candle = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Candle) as StockItem;
                        var volumn = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Volumn) as VolumnItem;

                        stockCollection.AddLatestChartItem(candle);
                        volumnCollection.AddLatestChartItem(volumn);
                        isStarted = true;
                    }
                    else
                    {
                        var candle = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Candle) as StockItem;
                        var volumn = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Volumn) as VolumnItem;

                        var lastestCandle = stockCollection.Items.Last();
                        var lastestVolumn = volumnCollection.Items.Last();

                        candle = ViewModel.MergeChartItem(lastestCandle as StockItem, candle);
                        volumn = ViewModel.MergeChartItem(lastestVolumn as VolumnItem, volumn);

                        Debug.WriteLine(candle.Date.ToString("HH:mm:ss ") + candle + "" + volumn);

                        stockCollection.UpdateLatestChartItem(candle);
                        volumnCollection.UpdateLatestChartItem(volumn);
                    }

                    price.ForceDraw(false, false);
                    i++;
                };

                Thread t = new Thread(new ThreadStart(() => {
                    Thread.Sleep(1000);
                    timer.Start(); }));

                t.Start();

            }
        }

        private void QueryTimeRunning()
        {
            if (timer != null)
            {
                timer.Stop();
            }

            StockTradeDetails std, stdEmpty;
            IList<TradeDetailData> tdds;
            var timeCollection = ViewModel.CreateEmptyTimeCollection(ChartItemType.Timely, raisePen, null, out std) as SymmetricChartItemCollection;

            if(timeCollection == null )
            {
                return;
            }
            tdds = std.Data;

            if ( tdds == null || tdds.Count < 10)
            {
                return;
            }
            timeCollection.StartDate = tdds[0].DateTime;
            price.XScaleFormat = "HH:mm";
            price.SetMainCollection(timeCollection);

            var volumnCollection = ViewModel.CreateEmptyTimeCollection(ChartItemType.TimelyVolumn, raisePen, fallPen, out stdEmpty) as SymmetricVolumnItemCollection;
            volumnCollection.StartDate = tdds[0].DateTime;
            volumn.XScaleFormat = "HH:mm";
            volumn.SetMainCollection(volumnCollection);

            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();

            bool isStarted = false;

            timer = new DispatcherTimer();
            timer.Interval = interval;
            int i = 0;
            double preVolumn = 0, preTurnover = 0;
            double preClose = timeCollection.StartValue;
            timer.Tick += (s, e) =>
            {
                if (i == tdds.Count)
                {
                    timer.Stop();
                    return;
                }

                var time = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Linear) as ChartItem;
                time.ValueChange = (tdds[i].current - timeCollection.StartValue) / timeCollection.StartValue;
                
                var volumn = ViewModel.FromTradeDetailData(tdds[i], ChartItemType.Volumn) as VolumnItem;
                
                var tLastItem = timeCollection.Items.LastOrDefault();
                var vLastItem = volumnCollection.Items.LastOrDefault() as VolumnItem;

                if (tLastItem == null || tLastItem.Date.Minute != time.Date.Minute)
                {
                    volumn.Volumn = volumn.Volumn - preVolumn;
                    volumn.Turnover = volumn.Turnover - preTurnover;

                    if(tLastItem != null)
                        preClose = timeCollection.Items.Last().Value;

                    timeCollection.AddLatestChartItem(time);
                    volumnCollection.AddLatestChartItem(volumn);

                    volumn.IsRaise = time.Value > preClose;
                    
                }
                else
                {
                    volumn.Volumn = volumn.Volumn - preVolumn + vLastItem.Volumn;
                    volumn.Turnover = volumn.Turnover - preTurnover + vLastItem.Turnover;

                    time = ViewModel.MergeChartItemBase(tLastItem, time);
                    volumn = ViewModel.MergeChartItem(vLastItem, volumn);
                    Debug.WriteLine(time.Date.ToString("HH:mm:ss ") + time + "" + volumn);
                    timeCollection.UpdateLatestChartItem(time);
                    volumnCollection.UpdateLatestChartItem(volumn);

                    volumn.IsRaise = time.Value > preClose;
                }

                preVolumn = tdds[i].volume;
                preTurnover = tdds[i].amount;

                price.ForceDraw();
                i++;
            };

            timer.Start();
            
        }

        private void QueryTimeRunning2()
        {
            if (timer != null)
            {
                timer.Stop();
            }

            StockTradeDetails std, stdEmpty;
            IList<TradeDetailData> tdds;

            var timeCollection = ViewModel.CreateEmptyTimeCollection(ChartItemType.Timely, raisePen, null, out std) as SymmetricChartItemCollection;
            if (timeCollection == null)
            {
                return;
            }

            tdds = std.Data;

            if (tdds == null || tdds.Count < 10)
            {
                return;
            }

            timeCollection.StartDate = tdds[0].DateTime;
            price.XScaleFormat = "HH:mm";
            price.SetMainCollection(timeCollection);

            var timeAVCollection = ViewModel.CreateEmptyTimeCollection(ChartItemType.TimelyAverage, contrastPen, null, out stdEmpty) as SymmetricChartItemCollection;
            timeAVCollection.StartDate = tdds[0].DateTime;
            timeAVCollection.StartValue = timeCollection.StartValue;
            price.AddAssistCollection(timeAVCollection);

            var volumnCollection = ViewModel.CreateEmptyTimeCollection(ChartItemType.TimelyVolumn, raisePen, fallPen, out stdEmpty) as SymmetricVolumnItemCollection;
            volumnCollection.StartDate = tdds[0].DateTime;
            volumn.XScaleFormat = "HH:mm";
            volumn.SetMainCollection(volumnCollection);

            price.YScaleDock = YScaleDock.Right;
            volumn.YScaleDock = YScaleDock.Right;
            price.AddConnection(volumn);

            price.ForceDraw();

            var timeBy = ViewModel.CreateCollection(ChartItemType.Timely, raisePen, null);
            var timeAverageBy = ViewModel.CreateCollection(ChartItemType.TimelyAverage, raisePen, null);
            var volumnBy = ViewModel.CreateCollection(ChartItemType.TimelyVolumn, raisePen, fallPen);

            timer = new DispatcherTimer();
            timer.Interval = interval;
            int i = 0;
            timer.Tick += (s, e) =>
            {
                if (i == timeBy.Items.Count)
                {
                    timer.Stop();
                    return;
                }

                timeCollection.AddLatestChartItem(timeBy.Items[i]);
                timeAVCollection.AddLatestChartItem(timeAverageBy.Items[i]);
                volumnCollection.AddLatestChartItem(volumnBy.Items[i]);
                price.ForceDraw(false, false);
                i++;
            };
            timer.Start();
            //var timer2 = new DispatcherTimer();
            //timer2.Interval = TimeSpan.FromSeconds(5);
            //timer2.Tick += (s, e) => { timer.Start(); };
            //timer2.Start();

        }

        private string GetMarketId(string id)
        {
            if (id.StartsWith("60"))
                return shMarket;
            else if (id.StartsWith("30") || id.StartsWith("39"))
            {
                return szMarket;
            }
            else if (id.StartsWith("99"))
            {
                return customMarket;
            }
            else if (id.StartsWith("00"))
            {
                return shSzMarket;
            }
            else
            {
                return "";
            }
        }
        private void OnChecked(object sender, RoutedEventArgs e)
        {
            price.CoordinateType = CoordinateType.Log10;
        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            price.CoordinateType = CoordinateType.Linear;
        }

        public void Test()
        {
            StockItem item = new StockItem();

        }

        private void OnClickSettings(object sender, RoutedEventArgs e)
        {
            ChartSettings settings = new ChartSettings();
            settings.ViewModel.CoordinateType = price.CoordinateType;
            settings.ViewModel.YDock = price.YScaleDock;
            settings.Owner = this;

            settings.ShowDialog();

            price.CoordinateType = settings.ViewModel.CoordinateType;
            price.YScaleDock = settings.ViewModel.YDock;

            //volumn.CoordinateType = settings.ViewModel.CoordinateType;
            volumn.YScaleDock = settings.ViewModel.YDock;

            price.ForceDraw();
            volumn.ForceDraw();
        }

        private void OnDrawLine(object sender, RoutedEventArgs e)
        {
            LineGraphics lG = new LineGraphics()
            {
                Pen = contrastPen
            };
            price.StartDrawCustomGraphics(lG);
        }

        private void OnDrawParallelLine(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("OnDrawParallelLine");
            ParallelLineGraphics plG = new ParallelLineGraphics()
            {
                Pen = contrastPen
            };
            price.StartDrawCustomGraphics(plG);
        }

        private void OnDelete(object sender, RoutedEventArgs e)
        {
            if (deleteGraphic.IsChecked)
                price.StartRemoveCustomGraphics();
            else
            {
                price.StopRemoveCustomGraphics();
            }
        }
    }
}
