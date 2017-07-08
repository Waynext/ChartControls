using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using MyParser.Dzh2;
using ChartView.Properties;
using System.Windows.Media;
using NetworkLibrary;
using System.IO;
using ChartControls;
using System.Diagnostics;
using ChartControls.Drawing;
using MyParser.DataManager;

namespace ChartView
{
    enum ChartItemType { Linear, Candle, Volumn, Timely, TimelyVolumn, TimelyAverage };
    class MainViewModel : INotifyPropertyChanged
    {
        private int mvCount = 6;
        private int mvStart = 0;
        private int[] mvUnits = new int[] { 10, 20, 30, 60, 120, 250 };
        //{ 10, 20, 30, 60, 120, 250};
        private Brush[] mvBrushs = new Brush[] { Brushes.Salmon, Brushes.Yellow, Brushes.Orange, Brushes.Blue, Brushes.Orange, Brushes.Purple };

        private DZHFolderHelp helper;
        public MainViewModel()
        {
            helper = new DZHFolderHelp(Settings.Default.DzhFolder, false);
            shareId = "000001";
        }
        public string shareId;
        public string ShareId
        {
            get
            {
                return shareId;
            }
            set
            {
                shareId = value;
                currentStock = null;
                ReportPropertyChange("ShareId");
            }
        }

        public bool isSH = true;
        public bool IsSH
        {
            get
            {
                return isSH;
            }
            set
            {
                isSH = value;
                ReportPropertyChange("IsSH");
            }
        }

        public bool isSZ = false;
        public bool IsSZ
        {
            get
            {
                return isSZ;
            }
            set
            {
                isSZ = value;
                currentStock = null;
                ReportPropertyChange("IsSZ");
            }
        }

        public bool isTime = false;
        public bool IsTime
        {
            get
            {
                return isTime;
            }
            set
            {
                isTime = value;
                currentStock = null;
                ReportPropertyChange("IsTime");
            }
        }

        public bool isDynamic = false;
        public bool IsDynamic
        {
            get
            {
                return isDynamic;
            }
            set
            {
                isDynamic = value;
                ReportPropertyChange("IsDynamic");
            }
        }

        private string statisticStoragePath = @"F:\DataBackup\报告\统计";
        public event PropertyChangedEventHandler PropertyChanged;

        private void ReportPropertyChange(string name)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(name));
            }
        }

        private Stock currentStock;
        public ChartItemCollection CreateCollection(ChartItemType type, IPen pen1, IPen pen2)
        {
            //if (currentStock == null)
            {
                if (type != ChartItemType.Timely && type != ChartItemType.TimelyVolumn && type != ChartItemType.TimelyAverage)
                    currentStock = LoadStock(shareId);
                else
                {
                    currentStock = LoadMinStock(shareId, type == ChartItemType.TimelyAverage);
                }

                if (Stock.IsNullOrEmpty(currentStock))
                {
                    return null;
                }
            }
               

            CollectionId id = new CollectionId(currentStock.id, GetMarketId());

            var chartItems = CreateChartItemFromStock(currentStock, type);
            ChartItemCollection coll = CreateChartItemCollection(type, chartItems, id, pen1, pen2);
            return coll;
        }

        public ChartItemCollection CreateStatisticCollection(IPen pen)
        {
            var mb = GetMarketBoardFromId();
            if (mb == MarketBoard.Unknown)
                return null;

            var statisticList = LoadStatistic(mb);
            var chartItems = CreateChartItemFromSZStatistic(statisticList);
            CollectionId id = new CollectionId(currentStock.id, GetMarketId());
            return new ChartItemCollection(id, chartItems, pen, null, true);
        }

        public MultipleChartItemCollection CreateMultipleStatisticCollection()
        {
            const int size = 4;

            CollectionId id = new CollectionId("AveragePE", "SZ");

            var mItemList = CreateMultipleChartItemList(size);

            Brush[] brushes = new Brush[size] { Brushes.Black, Brushes.Red, Brushes.Blue, Brushes.Yellow };
            IPen[] pens = new IPen[brushes.Length];

            for(int i = 0; i < brushes.Length; i ++)
            {
                pens[i] = DrawingObjectFactory.CreatePen(brushes[i], 1);
            }

            MultipleChartItemCollection coll = new MultipleChartItemCollection(id, mItemList, pens, true);

            return coll;
        }

        public ChartItemCollection CreateEmptyTimeCollection(ChartItemType type, IPen pen1, IPen pen2, out StockTradeDetails std)
        {
            std = null;

            Stock s = null;
            if (type == ChartItemType.Timely)
            {
                s = LoadStock(shareId);
                std = LoadDetailDatas();
                if (Stock.IsNullOrEmpty(s))
                {
                    return null;
                }
            }
                
            CollectionId id = new CollectionId(shareId, GetMarketId());
            ChartItemCollection coll = null;
            switch(type)
            {
                case ChartItemType.Timely:
                    var tColl = new SymmetricChartItemCollection(id, null, pen1, null, SymmetricCommonSettings.CNSettings2);
                    tColl.StartValue = s.Items.Last().close;
                    coll = tColl;
                    break;
                case ChartItemType.TimelyAverage:
                    coll = new SymmetricChartItemCollection(id, null, pen1, null, SymmetricCommonSettings.CNSettings2);
                    break;
                case ChartItemType.TimelyVolumn:
                    coll = new SymmetricVolumnItemCollection(id, null, pen1, pen2, SymmetricCommonSettings.CNSettings2);
                    break;
                default:
                    break;
            }

            return coll;
        }

        private List<MultipleChartItem> CreateMultipleChartItemList(int size)
        {
            
            List<Statistic>[] statisticListArray = new List<Statistic>[size];
            int[] indexArray = new int[size];
            for (int i = 0; i < size; i++)
            {
                indexArray[i] = 0;
                statisticListArray[i] = LoadStatistic((MarketBoard)(i + 1));
            }

            var itemList = FromStatisticList(statisticListArray[0], size - 1);

            for (int j = 0; j < size - 1; j++)
            {
                var tempItemList = FromStatisticList(statisticListArray[j + 1]);

                Combine(itemList, tempItemList, j);
            }
            return itemList;
        }

        private ChartItemCollection CreateChartItemCollection(ChartItemType type, List<ChartItem> chartItems, CollectionId id, IPen pen1, IPen pen2)
        {
            ChartItemCollection coll = null;
            if (type == ChartItemType.Linear)
            {
                coll = new ChartItemCollection(id, chartItems, pen1, null, true, isDynamic);
            }
            else if (type == ChartItemType.Candle)
            {
                coll = new StockItemCollection(id, chartItems != null ? chartItems.OfType<StockItem>() : null, pen1, pen2, null, isDynamic);
            }
            else if (type == ChartItemType.Volumn)
            {
                coll = new VolumnItemCollection(id, chartItems != null ? chartItems.OfType<VolumnItem>() : null, pen1, pen2, isDynamic);
            }
            else if(type == ChartItemType.Timely || type == ChartItemType.TimelyAverage)
            {
                coll = new SymmetricChartItemCollection(id, chartItems, pen1, null, SymmetricCommonSettings.CNSettings2);
            }
            else if (type == ChartItemType.TimelyVolumn)
            {
                coll = new SymmetricVolumnItemCollection(id, chartItems.OfType<VolumnItem>(), pen1, pen2, SymmetricCommonSettings.CNSettings2);
            }
            return coll;
        }

        private const string shMarketId = "SH";
        private const string szMarketId = "SZ";
        private string GetMarketId()
        {
            string makget = isSH ? shMarketId : szMarketId;
            return makget;
        }

        public Stock LoadFullStock(string sId)
        {
            if (string.IsNullOrEmpty(sId))
                return null;
            string market = GetMarketId();

            var parser = helper.GetDayParser(market);

            DateTime start = DateTime.Now.AddYears(-10);
            var currentStock = parser.GetOneStock(sId, start);

            return currentStock;
        }

        private Stock LoadStock(string sId)
        {
            if (string.IsNullOrEmpty(sId))
                return null;
            string market = GetMarketId();

            var parser = helper.GetDayParser(market);

            var dateEnd = DateTime.Now;
            var dateStart = dateEnd.AddDays(-5500);
            var currentStock = parser.GetOneStock(sId, dateStart, dateEnd);

            return currentStock;
        }

        private Stock LoadMinStock(string sId, bool isMa)
        {
            if (!File.Exists(Settings.Default.PrpDataFile))
                return null;

            PrpTradeDetailParser parser = new PrpTradeDetailParser(Settings.Default.PrpDataFile);
            Stock s = null;

            var std = parser.GetOneStockTradeDetail(sId);
            if (!StockTradeDetails.IsNullOrEmpty(std) && std.DataCount > 10)
            {
                if(!isMa)
                    s = TradeDetails2Stock.Convert(std, CandlePeriod.min1);
                else
                    s = TradeDetails2Stock.ConvertMA(std, CandlePeriod.min1);
            }

            return s;
        }

        public StockTradeDetails LoadDetailDatas()
        {
            PrpTradeDetailParser parser = new PrpTradeDetailParser(Settings.Default.PrpDataFile);
            var std = parser.GetOneStockTradeDetail(shareId);

            return std;
        }

        public ChartItem FromTradeDetailData(TradeDetailData tdd, ChartItemType type)
        {
            ChartItem item = null;

            switch(type)
            {
                case ChartItemType.Linear:
                    item = new ChartItem()
                    {
                        Value = tdd.current,
                        Date = tdd.DateTime
                    };
                    break;
                case ChartItemType.Candle:
                    item = new StockItem()
                    {
                        Close = tdd.current,
                        Date = tdd.DateTime,
                        High = tdd.current,
                        Low = tdd.current,
                        Open = tdd.current
                    };
                    break;
                case ChartItemType.Volumn:
                    item = new VolumnItem()
                    {
                        Volumn = tdd.volume,
                        Turnover = tdd.amount,
                        Date = tdd.DateTime,

                    };
                    break;
                default:
                    break;
            }

            return item;
        }

        public ChartItem MergeChartItemBase(ChartItem source, ChartItem target)
        {
            source.Date = target.Date;
            source.Value = target.Value;
           
            return source;
        }

        public StockItem MergeChartItem(StockItem source, StockItem target)
        {
            source.Date = target.Date;
            source.Close = target.Close;
            if(source.High < target.High)
            {
                source.High = target.High;
            }
            if(source.Low > target.Low)
            {
                source.Low = target.Low;
            }

            return source;
        }

        public VolumnItem MergeChartItem(VolumnItem source, VolumnItem target)
        {
            source.Date = target.Date;
            source.Volumn = target.Volumn;
            source.Turnover = target.Turnover;
            return source;
        }

        private ChartItem CandleData2Item(CandleData cd, CandleData? cdPre, ChartItemType type)
        {
            if (type == ChartItemType.Linear)
            {
                return new ChartItem()
                {
                    Value = cd.close,
                    Date = cd.DateTime,
                    ValueChange = cdPre != null ? (cd.close - cdPre.Value.close) / cdPre.Value.close : 0
                };
            }
            else if (type == ChartItemType.Candle)
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
            else if (type == ChartItemType.Volumn || type == ChartItemType.TimelyVolumn)
            {
                return new VolumnItem()
                {
                    Date = cd.DateTime,
                    Volumn = cd.amount,
                    Turnover = cd.money,
                    IsRaise = cd.open <= cd.close
                };
            }
            else if( type == ChartItemType.Timely)
            {
                return new ChartItem()
                {
                    Value = cd.close,
                    Date = cd.DateTime,
                    ValueChange = cdPre != null ? (cd.close - cdPre.Value.close) / cdPre.Value.close : 0
                };
            }
            else if (type == ChartItemType.TimelyAverage)
            {
                return new ChartItem()
                {
                    Value = cd.close,
                    Date = cd.DateTime
                };
            }
            else
            {
                return null;
            }
        }

        private List<ChartItem> CreateChartItemFromStock(Stock s, ChartItemType type)
        {
            if (Stock.IsNullOrEmpty(s))
                return null;

            List<ChartItem> items = new List<ChartItem>();

            CandleData? cdPre = null;

            if (type == ChartItemType.Timely)
                cdPre = s.Data[0];

            int iCount = 30;
            int i = iCount;

            foreach (var cd in s.Data)
            {
                var cItem = CandleData2Item(cd, cdPre, type);
                if(type != ChartItemType.Timely)
                    cdPre = cd;
                items.Add(cItem);

                i--;
                if (i == 0)
                {
                    cItem.ExtraData = new ExtraData();
                    //cItem.ExtraData.Add(ExtraDataNames.XRName, new ExitRight() { Dividen = cItem.ValueChange});
                    i = iCount;
                }
            }
            //Duplicate(items);
            return items;
        }

        private void Duplicate(List<ChartItem> items)
        {
            var firstItem = items.FirstOrDefault();
            if (firstItem == null)
                return;

            foreach (var item in items)
            {
                var sItem = item as StockItem;
                var sFItem = firstItem as StockItem;
                if (sItem != null)
                {
                    sItem.High = sFItem.High;
                    sItem.Low = sFItem.Low;
                    sItem.Close = sFItem.Close;
                    sItem.Open = sFItem.Open;
                }
                else
                {
                    item.Value = firstItem.Value;
                }
            }
        }
        private List<Statistic> LoadStatistic(MarketBoard mb)
        {
            StatisticStorage storage = new StatisticStorage(statisticStoragePath, mb);

            return storage.StatisticCollection;
        }

        private List<ChartItem> CreateChartItemFromSZStatistic(List<Statistic> statistic)
        {
            return statistic.Select(s => 
            {
                SZStatistic szS = s as SZStatistic;
                if (szS != null)
                {
                    return new ChartItem()
                    {
                        Date = szS.Date,
                        Value = szS.AveragePE
                    };
                }
                else
                {
                    SHStatistic shS = s as SHStatistic;
                    if (shS != null)
                    {
                        return new ChartItem()
                        {
                            Date = shS.Date,
                            Value = shS.AveragePE
                        };
                    }
                }

                return null;
            }).ToList();
        }

        private const string shGeneralIndex = "000001";

        private const string szGeneralIndex = "399106";
        private const string szMainIndex = "399001";
        private const string szSMEIndex = "399005";
        private const string szSMEGneralIndex = "399101";

        private const string szGemIndex = "399006";
        private const string szGemGeneralIndex = "399102";

        private const string shMainIndex = "000001";
        private MarketBoard GetMarketBoardFromId()
        {
            if (isSZ)
            {
                if (shareId == szMainIndex)
                {
                    return MarketBoard.SZMain;
                }
                else if (shareId == szGeneralIndex)
                {
                    return MarketBoard.SZGeneral;
                }
                else if (shareId == szSMEIndex || shareId == szSMEGneralIndex)
                {
                    return MarketBoard.SZSME;
                }
                else if (shareId == szGemIndex || shareId == szGemGeneralIndex)
                {
                    return MarketBoard.SZGem;
                }
            }
            else if (isSH)
            {
                if (shareId == shGeneralIndex)
                {
                    return MarketBoard.SHGeneral;
                }
            }
            return MarketBoard.Unknown;
            

        }
        double[] invalidValues = null;
        private List<MultipleChartItem> FromStatisticList(List<Statistic> statisticList, int valueCount = 0)
        {
            if (valueCount != 0)
            {
                invalidValues = new double[valueCount];
                for (int i = 0; i < valueCount; i++)
                {
                    invalidValues[i] = ChartItemCollection.valueNA;
                }
            }

            List<MultipleChartItem> mItems = new List<MultipleChartItem>(statisticList.Count);

            foreach (var s in statisticList)
            {
                var szS = s as SZStatistic;
                mItems.Add(new MultipleChartItem()
                {
                    Value = szS.AveragePE,
                    Date = szS.Date,
                    Values = valueCount != 0 ? new List<double>(invalidValues) : null,
                    ValueChanges = valueCount != 0 ? new List<double>(invalidValues) : null
                });
            }

            return mItems;
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

        public StockValuesItemCollection CreateSingleStockCollection(IPen pen1, IPen pen2, IPen contrastPen)
        {
            
            var itemList = CreateSingleStockItemList();

            if (itemList != null)
            {
                CollectionId id = new CollectionId(shareId, GetMarketId());
                StockValuesItemCollection collection = new StockValuesItemCollection(id, itemList, pen1, pen2, null, new IPen[] { contrastPen });
                return collection;
            }

            return null;
        }


        private List<StockValuesItem> CreateSingleStockItemList()
        {
            if (currentStock == null)
            {
                currentStock = LoadStock(shareId);

                if (Stock.IsNullOrEmpty(currentStock))
                {
                    return null;
                }
            }

            invalidValues = new double[1];
            for (int i = 0; i < invalidValues.Length; i++)
            {
                invalidValues[i] = ChartItemCollection.valueNA;
            }

            var cItems = CreateMultiItemListFrom(currentStock, ChartItemType.Candle);
            
            List<ChartItem> contrastItems = null;
            string contrastId = null;
            var marketId = GetMarketId();
            if (marketId == shMarketId)
            {
                contrastId = shMainIndex;
            }
            else
            {
                contrastId = szMainIndex;
            }

            if (contrastId != ShareId)
            {
                var contrastStock = LoadStock(contrastId);
                contrastItems = CreateChartItemFromStock(contrastStock, ChartItemType.Linear);

                

                CombineSingleStockItems(cItems, contrastItems, 0);
            }

            var ssItems = cItems.Select(item => item as StockValuesItem).ToList();
            return ssItems;
        }

        private List<ChartItem> CreateMultiItemListFrom(Stock s, ChartItemType type)
        {
            if (Stock.IsNullOrEmpty(s))
                return null;

            List<ChartItem> items = new List<ChartItem>();

            CandleData? cdPre = null;
            foreach (var cd in s.Data)
            {
                var cItem = CandleData2MultiItem(cd, cdPre, type);
                cdPre = cd;
                items.Add(cItem);
            }

            return items;
        }

        private ChartItem CandleData2MultiItem(CandleData cd, CandleData? cdPre, ChartItemType type)
        {
            if (type == ChartItemType.Linear)
            {
                return new MultipleChartItem()
                {
                    Value = cd.close,
                    Date = cd.DateTime,
                    ValueChange = (cd.close - cdPre.Value.close) / cdPre.Value.close,
                    Values = new List<double>(invalidValues),
                    ValueChanges = new List<double>(invalidValues)
                };
            }
            else if (type == ChartItemType.Candle)
            {
                return new StockValuesItem()
                {
                    Date = cd.DateTime,
                    High = cd.high,
                    Low = cd.low,
                    Open = cd.open,
                    Close = cd.close,
                    CloseChange = cdPre != null ? (cd.close - cdPre.Value.close) / cdPre.Value.close : 0,
                    Values = new List<double>(invalidValues),
                    ValueChanges = new List<double>(invalidValues)
                };
            }
            else
            {
                return null;
            }
        }

        private List<ChartItem> CombineSingleStockItems(List<ChartItem> one, List<ChartItem> other, int j)
        {
            ChartItemComparer comparar = new ChartItemComparer();
            int iOne = 0;
            int iOther = 0;

            StockValuesItem zeroOneItem = one[0] as StockValuesItem;

            for (; iOther < other.Count; iOther++)
            {
                StockValuesItem iOneItem = one[iOne] as StockValuesItem;
                if (iOne >= one.Count)
                {
                    one.AddRange(other.Skip(iOther).Select(mItem =>
                    {
                        var tempItem = new StockValuesItem()
                        {
                            Value = ChartItemCollection.valueNA,
                            High = ChartItemCollection.valueNA,
                            Low = ChartItemCollection.valueNA,
                            Open = ChartItemCollection.valueNA,
                            Date = other[iOther].Date,
                            ValueChange = ChartItemCollection.valueNA,
                            Values = zeroOneItem.Values != null ? new List<double>(invalidValues) : null,
                            ValueChanges = zeroOneItem.ValueChanges != null ? new List<double>(invalidValues) : null
                        };

                        tempItem.Values[j] = mItem.Value;
                        tempItem.ValueChanges[j] = mItem.ValueChange;

                        return tempItem;
                    }));
                    break;
                }

                if (one[iOne].Date == other[iOther].Date)
                {
                    iOneItem.Values[j] = other[iOther].Value;
                    iOneItem.ValueChanges[j] = other[iOther].ValueChange;
                    iOne++;
                }
                else if (one[iOne].Date > other[iOther].Date)
                {
                    var tempItem = new StockValuesItem()
                    {
                        Value = ChartItemCollection.valueNA,
                        High = ChartItemCollection.valueNA,
                        Low = ChartItemCollection.valueNA,
                        Open = ChartItemCollection.valueNA,
                        Date = other[iOther].Date,
                        ValueChange = ChartItemCollection.valueNA,
                        Values = zeroOneItem.Values != null ? new List<double>(invalidValues) : null,
                        ValueChanges = zeroOneItem.ValueChanges != null ? new List<double>(invalidValues) : null
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
                                var tempItem = new StockValuesItem()
                                {
                                    Value = ChartItemCollection.valueNA,
                                    High = ChartItemCollection.valueNA,
                                    Low = ChartItemCollection.valueNA,
                                    Open = ChartItemCollection.valueNA,
                                    Date = other[iOther].Date,
                                    ValueChange = ChartItemCollection.valueNA,
                                    Values = zeroOneItem.Values != null ? new List<double>(invalidValues) : null,
                                    ValueChanges = zeroOneItem.ValueChanges != null ? new List<double>(invalidValues) : null
                                };

                                tempItem.Values[j] = mItem.Value;
                                tempItem.ValueChanges[j] = mItem.ValueChange;

                                return tempItem;
                            }));
                            break;
                        }
                        else
                        {
                            var tempItem = new StockValuesItem()
                            {
                                Value = ChartItemCollection.valueNA,
                                High = ChartItemCollection.valueNA,
                                Low = ChartItemCollection.valueNA,
                                Open = ChartItemCollection.valueNA,
                                Date = other[iOther].Date,
                                ValueChange = ChartItemCollection.valueNA,
                                Values = zeroOneItem.Values != null ? new List<double>(invalidValues) : null,
                                ValueChanges = zeroOneItem.ValueChanges != null ? new List<double>(invalidValues) : null
                            };
                            tempItem.Values[j] = other[iOther].Value;
                            tempItem.ValueChanges[j] = other[iOther].ValueChange;

                            one.Insert(iOneTemp, tempItem);
                            iOne = iOneTemp + 1;
                        }
                    }
                    else
                    {
                        StockValuesItem iOneItemTemp = one[iOneTemp] as StockValuesItem;

                        iOneItemTemp.Values[j] = other[iOther].Value;
                        iOneItemTemp.ValueChanges[j] = other[iOther].ValueChange;
                        iOne = iOneTemp + 1;
                    }
                }
            }

            return one;
        }

        public IEnumerable<ChartItemCollection> CreateMVCollections()
        {
            for (int i = mvStart; i < mvStart + mvCount; i++)
            {
                var maPeriod = mvUnits[i];
                var ma60 = currentStock.MA(maPeriod);

                List<ChartItem> mv60Items = new List<ChartItem>(ma60.Count);
                for (int j = 0; j < ma60.Count; j++)
                {
                    mv60Items.Add(new ChartItem() { Value = ma60[j], Date = currentStock.Data[j + maPeriod - 1].DateTime });
                }

                var id = new CollectionId(currentStock.id, GetMarketId());
                ChartItemCollection collMa60 = new ChartItemCollection(id, mv60Items, DrawingObjectFactory.CreatePen(mvBrushs[i], 1), null, true, false);
                yield return collMa60;
            }
        }
    }
}
