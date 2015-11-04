using ChartControls;
using ChartControls.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ChartViewUW
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private const string id1 = "000001";
        private const string id2 = "399001";
        private const string id3 = "600100";

        private const string priceFormat = "F2";
        private const string percentFormat = "P2";
        private const string volumnFormat = "N2";
        private const string dateFormat = "yy/MM/dd";
        private const string timeFormat = "HH:mm";
        private MainViewModel model;
        private DataLoader loader;
        private DataLoader Timeloader;

        private TimeSpan tradingPeriod = TimeSpan.FromSeconds(0.5);
        private DispatcherTimer tradingTimer;

        public MainPage()
        {
            this.InitializeComponent();
            DataContext = model = new MainViewModel();
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            loader = new DataLoader(DataLoader.sFileName);
            Timeloader = new DataLoader(DataLoader.stFileName, true);
            AdjustViews(true);
            await QueryChartCollection(id1);
        }

        private void price_CursorMoved(object sender, CursorMovedRoutedEventArgs e)
        {
            if (e != null && e.CurrentItems != null)
            {
                int iIndex = 1;
                foreach(var item in e.CurrentItems)
                {
                    var collection = price.FindCollection(item.Id);
                    if (collection == null)
                        continue;
                    model.Date = item.Item.Date.ToString(collection is SymmetricChartItemCollection ? timeFormat : dateFormat);
                    var realItem = item.Item as StockItem;
                    if (realItem != null)
                    {
                        var sRealItem = item.Item as StockValuesItem;

                        model.SetValue<string>(iIndex, MainViewModel.priceName, realItem.Close.ToString(priceFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        model.SetValue<string>(iIndex, MainViewModel.priceName, realItem.CloseChange.ToString(percentFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        model.SetValue<string>(iIndex, MainViewModel.priceName, realItem.Open.ToString(priceFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        model.SetValue<string>(iIndex, MainViewModel.priceName, realItem.High.ToString(priceFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        model.SetValue<string>(iIndex, MainViewModel.priceName, realItem.Low.ToString(priceFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        if (sRealItem != null)
                        {
                            var sCollection = collection as StockValuesItemCollection;
                            for (int i = 0; i < sRealItem.Values.Count; i ++)
                            {
                                model.SetValue<string>(iIndex, MainViewModel.priceName, sRealItem.Values[i].ToString(priceFormat));
                                model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, sCollection.Pens[i].Brush);

                                iIndex++;

                                model.SetValue<string>(iIndex, MainViewModel.priceName, sRealItem.ValueChanges[i].ToString(percentFormat));
                                model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, sCollection.Pens[i].Brush);

                                iIndex++;
                            }
                        }
                    }
                    else
                    {
                        model.SetValue<string>(iIndex, MainViewModel.priceName, item.Item.Value.ToString(priceFormat));
                        model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                        iIndex++;

                        if (iIndex == 2)
                        {
                            model.SetValue<string>(iIndex, MainViewModel.priceName, item.Item.ValueChange.ToString(percentFormat));
                            model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, collection.Pen.Brush);
                            iIndex++;
                        }

                        var mRealItem = item.Item as MultipleChartItem;
                        if (mRealItem != null)
                        {
                            var mCollection = collection as MultipleChartItemCollection;
                            for (int i = 0; i < mRealItem.Values.Count; i ++)
                            {
                                model.SetValue<string>(iIndex, MainViewModel.priceName, mRealItem.Values[i].ToString(priceFormat));
                                model.SetValue<Brush>(iIndex, MainViewModel.priceClrName, mCollection.Pens[i].Brush);
                                iIndex++;
                            }
                        }
                    }
                }

                
            }
        }

        private void volumn_CursorMoved(object sender, CursorMovedRoutedEventArgs e)
        {
            if (e != null && e.CurrentItems != null)
            {
                foreach (var item in e.CurrentItems)
                {
                    var collection = volumn.FindCollection(item.Id) as VolumnItemCollection;
                    if (collection != null && item.Item != null)
                    {
                        var realItem = item.Item as VolumnItem;
                        if (realItem != null)
                        {
                            model.SetValue<string>(1, MainViewModel.volumnName, realItem.Date.ToString(collection is SymmetricVolumnItemCollection ? timeFormat : dateFormat));
                            model.SetValue<Brush>(1, MainViewModel.volumnClrName, realItem.IsRaise ? collection.RaisePen.Brush : collection.FallPen.Brush);

                            model.SetValue<string>(2, MainViewModel.volumnName, realItem.Volumn.ToString(volumnFormat));
                            model.SetValue<Brush>(2, MainViewModel.volumnClrName, realItem.IsRaise ? collection.RaisePen.Brush : collection.FallPen.Brush);

                            model.SetValue<string>(3, MainViewModel.volumnName, realItem.Turnover.ToString(volumnFormat));
                            model.SetValue<Brush>(3, MainViewModel.volumnClrName, realItem.IsRaise ? collection.RaisePen.Brush : collection.FallPen.Brush);

                        }
                    }
                }
               
            }
        }

        private void AdjustViews(bool isHideVolumView)
        {
            if (isHideVolumView)
            {
                price.SetValue(Grid.RowSpanProperty, 3);
                volumn.Visibility = Visibility.Collapsed;
                volumnTitle.Visibility = Visibility.Collapsed;
            }
            else
            {
                if(volumn.Visibility != Visibility.Visible)
                {
                    volumn.Visibility = Visibility.Visible;
                    volumnTitle.Visibility = Visibility.Visible;
                    price.SetValue(Grid.RowSpanProperty, 1);
                }
            }

            if (tradingTimer != null)
            {
                tradingTimer.Stop();
                tradingTimer = null;
            }
        }

        private async Task QueryChartCollection(string id)
        {
            var chartItems = await loader.GetChartItems(id);
            if (chartItems == null || !chartItems.Any())
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var collection = CreateCollection(id, chartItems, ChartItemType.Linear, DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), null);
            price.SetMainCollection(collection);
            SetAttritues();
            price.ForceDraw(true);
        }

        private async Task QueryCandleCollection(string id)
        {
            var svItems = await loader.GetStockItems(id);
            if (svItems == null || svItems.Prices == null || svItems.Volumns == null)
            {
                Debug.WriteLine("Can not load candle items");
                return;
            }

            var collection = CreateCollection(id, svItems.Prices, ChartItemType.Candle, 
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1));
            price.SetMainCollection(collection);
            SetAttritues();

            var maCollections = CreateMACollections(svItems.Prices);
            foreach (var maColl in maCollections)
            {
                price.AddAssistCollection(maColl);
            }

            var vCollection = CreateCollection(id, svItems.Volumns, ChartItemType.Volumn, 
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1));
            volumn.SetMainCollection(vCollection);
            volumn.AddConnection(price);
            price.ForceDraw(true);
        }

        private async Task QueryMultipleCollection(string id)
        {
            var chartItems = await loader.GetStockItems(id);
            if (chartItems == null || chartItems.Prices == null)
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var maColls = CreateMACollections(chartItems.Prices).ToArray();
            IEnumerable<ChartItem>[] cItemsList = new IEnumerable<ChartItem>[maColls.Count() + 1];
            IPen[] pens = new IPen[maColls.Count() + 1];

            cItemsList[0] = chartItems.Prices;
            pens[0] = DrawingObjectFactory.CreatePen(model.RaiseBrush, 1);

            for(int i = 1; i < cItemsList.Length; i ++)
            {
                cItemsList[i] = maColls[i - 1].Items;
                pens[i] = maColls[i - 1].Pen;
            }

            var collection = CreateCollection(id, cItemsList, pens);
            price.SetMainCollection(collection);
            SetAttritues();
            price.ForceDraw(true);
        }

        private async Task QueryCandleComparisonCollection(string id, string id2)
        {
            var svItems = await loader.GetStockItems(id);
            if (svItems == null || svItems.Prices == null)
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var chartItems2 = await loader.GetChartItems(id2);
            if (chartItems2 == null || !chartItems2.Any())
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var collection = CreateCollection(id, svItems.Prices, chartItems2, ChartItemType.Candle, 
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1), 
                new IPen[] { DrawingObjectFactory.CreatePen(model.ContrastBrush, 1) }
                );
            price.SetMainCollection(collection);
            SetAttritues();
            price.CoordinateType = CoordinateType.Percentage;
            price.ForceDraw(true);
        }

        private async Task QueryChartOverlapCollection(string id, string id2)
        {
            var chartItems = await loader.GetChartItems(id);
            if (chartItems == null || !chartItems.Any())
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var chartItems2 = await loader.GetChartItems(id2);
            if (chartItems2 == null || !chartItems2.Any())
            {
                Debug.WriteLine("Can not load chart items");
                return;
            }

            var collection = CreateCollection(id, chartItems, ChartItemType.Linear, DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), null);
            var collection2 = CreateCollection(id2, chartItems2, ChartItemType.Linear, DrawingObjectFactory.CreatePen(model.FallBrush, 1), null);
            price.SetMainCollection(collection);
            SetAttritues();
            price.AddAssistCollection(collection2, true);
            price.ForceDraw(true);
        }

        private async Task QueryTimeCollection(string id)
        {
            var svItems = await Timeloader.GetStockItems(id);
            if (svItems == null || svItems.Prices == null || svItems.Volumns == null)
            {
                Debug.WriteLine("Can not load candle items");
                return;
            }

            var collection = CreateCollection(id, svItems.Prices, ChartItemType.Time,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), null);
            price.SetMainCollection(collection);
            SetAttritues(false);
            var idAverage = id + "average";
            var cItems = await Timeloader.GetChartItems(idAverage);
            if (cItems != null && cItems.Any())
            {
                var maCollection = CreateTimeMACollection(cItems, idAverage);
                price.AddAssistCollection(maCollection);
            }

            var vCollection = CreateCollection(id, svItems.Volumns, ChartItemType.TimeVolumn,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1));
            volumn.SetMainCollection(vCollection);
            volumn.AddConnection(price);
            price.ForceDraw(true);
        }

        private async Task QueryCandleTrading(string id)
        {
            var svItems = await loader.GetStockItems(id);
            if (svItems == null || svItems.Prices == null || svItems.Volumns == null)
            {
                Debug.WriteLine("Can not load candle items");
                return;
            }

            var collection = CreateCollection(id, svItems.Prices, ChartItemType.Candle,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1));
            price.SetMainCollection(collection);
            SetAttritues();

            var maCollections = CreateMACollections(svItems.Prices);
            foreach (var maColl in maCollections)
            {
                price.AddAssistCollection(maColl);
            }

            var vCollection = CreateCollection(id, svItems.Volumns, ChartItemType.Volumn,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1));
            volumn.SetMainCollection(vCollection);
            volumn.AddConnection(price);

            price.ForceDraw();

            CandleTrading(id, collection, vCollection, price.AssistCollections, svItems);


        }

        private void CandleTrading(string id, ChartItemCollection priceColl, ChartItemCollection volumnColl, IEnumerable<ChartItemCollection> maColls, StockVolumnList svList, int mvStart = 0)
        {
            tradingTimer = new DispatcherTimer();
            
            bool isStarted = false;
            int i = 0;
            tradingTimer.Interval = tradingPeriod;
            tradingTimer.Tick += async (s, e) => {
                var tvList = await Timeloader.GetStockItems(id);

                if (i == tvList.Prices.Count)
                {
                    tradingTimer.Stop();
                    return;
                }

                var candle = tvList.Prices[i];
                var volumn = tvList.Volumns[i];

                if (!isStarted)
                {
                    priceColl.AddLatestChartItem(candle);
                    volumnColl.AddLatestChartItem(volumn);

                    UpdateMACollections(maColls, svList.Prices, candle, mvStart, isStarted);
                    isStarted = true;
                }
                else
                {
                    var lastestCandle = priceColl.Items.Last() as StockItem;
                    var lastestVolumn = volumnColl.Items.Last() as VolumnItem;

                    candle = MergeChartItem(lastestCandle, candle);
                    volumn = MergeChartItem(lastestVolumn, volumn);
                    volumn.IsRaise = candle.Open <= candle.Close;

                    priceColl.UpdateLatestChartItem(candle);
                    volumnColl.UpdateLatestChartItem(volumn);

                    UpdateMACollections(maColls, svList.Prices, candle, mvStart, isStarted);
                }

                price.ForceDraw(false, false);
                i++;
            };

            tradingTimer.Start();
        }

        public StockItem MergeChartItem(StockItem source, StockItem target)
        {
            source.Date = target.Date;
            source.Close = target.Close;
            if (source.High < target.High)
            {
                source.High = target.High;
            }
            if (source.Low > target.Low)
            {
                source.Low = target.Low;
            }

            return source;
        }

        public VolumnItem MergeChartItem(VolumnItem source, VolumnItem target)
        {
            source.Date = target.Date;
            source.Volumn += target.Volumn;
            source.Turnover += target.Turnover;
            return source;
        }

        private async Task QueryTimeTrading(string id)
        {
            var cOldItems = await loader.GetChartItems(id);
            if (cOldItems == null || !cOldItems.Any())
            {
                Debug.WriteLine("Can not load old candle items");
                return;
            }

            var oldItem = cOldItems.Last();

            var collection = CreateCollection(id, null, ChartItemType.Time,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), null) as SymmetricChartItemCollection;
            collection.StartDate = oldItem.Date.Date;
            collection.StartValue = oldItem.Value;


            price.SetMainCollection(collection);
            SetAttritues(false);

            var maColl = CreateCollection(id, null, ChartItemType.Time,
                DrawingObjectFactory.CreatePen(model.ContrastBrush, 1), null) as SymmetricChartItemCollection;
            maColl.StartDate = oldItem.Date.Date;
            maColl.StartValue = oldItem.Value;

            price.AddAssistCollection(maColl);

            var vCollection = CreateCollection(id, null, ChartItemType.TimeVolumn,
                DrawingObjectFactory.CreatePen(model.RaiseBrush, 1), DrawingObjectFactory.CreatePen(model.FallBrush, 1)) as SymmetricVolumnItemCollection;

            vCollection.StartDate = oldItem.Date.Date;

            volumn.SetMainCollection(vCollection);
            price.AddConnection(volumn);
            price.ForceDraw();

            model.Date = oldItem.Date.ToString(dateFormat);

            await TimeTrading(id);
        }

        private async Task TimeTrading(string id)
        {
            var svItems = await Timeloader.GetStockItems(id);
            if (svItems == null || svItems.Prices == null || svItems.Volumns == null)
            {
                Debug.WriteLine("Can not load candle items");
                return;
            }

            var idAverage = id + "average";
            var cItems = await Timeloader.GetChartItems(idAverage);
            var maCollection = CreateTimeMACollection(cItems, idAverage);
            tradingTimer = new DispatcherTimer();

            int i = 0;
            tradingTimer.Interval = tradingPeriod;
            tradingTimer.Tick += (s, e) =>
            {
                if (i == svItems.Prices.Count)
                {
                    tradingTimer.Stop();
                    return;
                }
                price.MainCollection.AddLatestChartItem(svItems.Prices[i]);
                price.AssistCollections.First().AddLatestChartItem(cItems[i]);
                volumn.MainCollection.AddLatestChartItem(svItems.Volumns[i]);

                price.ForceDraw(false, false);
                i++;
            };

            tradingTimer.Start();

        }

        private void SetAttritues(bool isReset = true)
        {
            if (isReset)
            {
                price.XScaleFormat = "yyMM";
                volumn.XScaleFormat = "yyMM";

                price.CoordinateType = CoordinateType.Linear;
            }
            else
            {
                price.XScaleFormat = "HH:mm";
                volumn.XScaleFormat = "HH:mm";
            }
        }
        private ChartItemCollection CreateTimeMACollection(List<ChartItem> cItems, string id)
        {
            CollectionId cId = new CollectionId(id);
            return CreateChartItemCollection(ChartItemType.Time, cItems, cId, DrawingObjectFactory.CreatePen(model.FallBrush, 1), null);
        }

        enum ChartItemType { Linear, Candle, Volumn, Time, TimeVolumn };

        private ChartItemCollection CreateCollection(string id, IEnumerable<ChartItem> chartItems, ChartItemType type, IPen pen1, IPen pen2)
        {
            CollectionId cId = new CollectionId(id);
            ChartItemCollection coll = CreateChartItemCollection(type, chartItems, cId, pen1, pen2);
            return coll;
        }

        private ChartItemCollection CreateCollection(string id, IEnumerable<ChartItem> chartItems, IEnumerable<ChartItem> chartItems2, ChartItemType type, IPen pen1, IPen pen2, IPen[] otherPens)
        {
            CollectionId cId = new CollectionId(id);
            ChartItemCollection coll = CreateSingleStockCollection(type, chartItems, chartItems2, cId, pen1, pen2, otherPens);
            return coll;
        }

        private ChartItemCollection CreateCollection(string id, IEnumerable<ChartItem>[] chartItemsList, IPen[] pens)
        {
            CollectionId cId = new CollectionId(id);
            ChartItemCollection coll = CreateMultipleItemCollection(chartItemsList, cId, pens);
            return coll;
        }

        private ChartItemCollection CreateChartItemCollection(ChartItemType type, IEnumerable<ChartItem> chartItems, CollectionId id, IPen pen1, IPen pen2)
        {
            ChartItemCollection coll = null;
            if (type == ChartItemType.Linear)
            {
                coll = new ChartItemCollection(id, chartItems, pen1, null, true);
            }
            else if (type == ChartItemType.Candle)
            {
                coll = new StockItemCollection(id, chartItems.OfType<StockItem>(), pen1, pen2, null);
            }
            else if (type == ChartItemType.Volumn)
            {
                coll = new VolumnItemCollection(id, chartItems.OfType<VolumnItem>(), pen1, pen2);
            }
            else if (type == ChartItemType.Time)
            {
                coll = new SymmetricChartItemCollection(id, chartItems, pen1, null);
            }
            else if (type == ChartItemType.TimeVolumn)
            {
                coll = new SymmetricVolumnItemCollection(id, chartItems != null ? chartItems.OfType<VolumnItem>() : null, pen1, pen2);
            }
            return coll;
        }

        private ChartItemCollection CreateMultipleItemCollection(IEnumerable<ChartItem>[] chartItemsList, CollectionId id, IPen[] pens)
        {
            List<MultipleChartItem> mItems = new List<MultipleChartItem>(chartItemsList[0].Count());

            IEnumerator<ChartItem>[] its = new IEnumerator<ChartItem>[chartItemsList.Length];

            for (int i = 1; i < chartItemsList.Length; i++)
            {
                its[i] = chartItemsList[i].GetEnumerator();
                its[i].MoveNext();
            }

            double[] invalidValues = new double[chartItemsList.Length - 1];
            for(int i = 0; i < invalidValues.Length; i ++)
            {
                invalidValues[i] = ChartItemCollection.valueNA;
            }
            foreach (var cItem in chartItemsList[0])
            {
                MultipleChartItem mItem = new MultipleChartItem()
                {
                    Date = cItem.Date,
                    Value = cItem.Value,
                    ValueChange = cItem.ValueChange,
                    Values = new List<double>(invalidValues),
                    ValueChanges = new List<double>(invalidValues)
                };

                for(int i = 1; i < chartItemsList.Length; i ++)
                {
                    if (its[i].Current.Date > cItem.Date)
                    {
                        continue;
                    }

                    mItem.Values[i - 1] = its[i].Current.Value;
                    mItem.ValueChanges[i - 1] = its[i].Current.ValueChange;
                    its[i].MoveNext();
                }
                mItems.Add(mItem);
            }

            return new MultipleChartItemCollection(id, mItems, pens);    
        }

        private ChartItemCollection CreateSingleStockCollection(ChartItemType type, IEnumerable<ChartItem> chartItems, IEnumerable<ChartItem> chartItems2, CollectionId id, IPen pen1, IPen pen2, IPen[] otherPens)
        {
            List<StockValuesItem> ssItems = new List<StockValuesItem>(chartItems.Count());

            var it = chartItems2.GetEnumerator();
            
            foreach (var cItem in chartItems)
            {
                it.MoveNext();
                var sItem = cItem as StockItem;
                StockValuesItem ssItem = new StockValuesItem()
                {
                    Date = sItem.Date,
                    High = sItem.High,
                    Low = sItem.Low,
                    Open = sItem.Open,
                    Close = sItem.Close,
                    CloseChange = sItem.CloseChange,
                    Values = new List<double>(new double[] { it.Current.Value }),
                    ValueChanges = new List<double>(new double[] { it.Current.ValueChange })
                };

                ssItems.Add(ssItem);
            }
            var coll = new StockValuesItemCollection(id, ssItems, pen1, pen2, null, otherPens);
            
                
            return coll;
        }

        public List<double> MA(int period, List<StockItem> chartItems)
        {
            int count = chartItems.Count - period + 1;
            if (count < 0)
                count = 0;

            List<double> maResult = new List<double>(count);
            if (count == 0)
                return maResult;

            double total = 0;

            for (int i = 0; i < period; i++)
            {
                total += chartItems[i].Value;
            }
            maResult.Add(total / period);

            for (int i = 0; i < count - 1; i++)
            {
                total -= chartItems[i].Value;
                total += chartItems[i + period].Value;
                maResult.Add(total / (float)period);
            }

            return maResult;
        }

        public double MAOne(int period, List<StockItem> chartItems, double newValue)
        {
            var sum = chartItems.Skip(chartItems.Count - period + 1).Take(period - 1).Sum(s => s.Close) + newValue;
            return sum / period;

        }
        private int mvCount = 3;
        private int[] mvUnits = new int[] { 10, 20, 30, 60, 120, 250 };
        //{ 10, 20, 30, 60, 120, 250};
        private Brush[] mvBrushs = new Brush[] { Brushes.Salmon, Brushes.Purple, Brushes.Orange, Brushes.Salmon, Brushes.Purple, Brushes.Orange };

        public IEnumerable<ChartItemCollection> CreateMACollections(List<StockItem> chartItems, int mvStart = 0)
        {
            for (int i = mvStart; i < mvStart + mvCount; i++)
            {
                var maPeriod = mvUnits[i];
                var maData = MA(maPeriod, chartItems);

                List<ChartItem> mvItems = new List<ChartItem>(maData.Count);
                for (int j = 0; j < maData.Count; j++)
                {
                    mvItems.Add(new ChartItem() { Value = maData[j], Date = chartItems[j + maPeriod - 1].Date });
                }

                var id = new CollectionId(maPeriod.ToString());
                ChartItemCollection collMa = new ChartItemCollection(id, mvItems, DrawingObjectFactory.CreatePen(mvBrushs[i], 1), null, true, false);
                yield return collMa;
            }
        }

        public void UpdateMACollections(IEnumerable<ChartItemCollection> maColls, List<StockItem> chartItems, ChartItem newItem, int mvStart = 0, bool isStarted = false)
        {
            for (int i = mvStart; i < mvStart + mvCount; i++)
            {
                var maPeriod = mvUnits[i];
                var maOne = MAOne(maPeriod, chartItems, newItem.Value);
                ChartItem item = new ChartItem()
                {
                    Value = maOne,
                    Date = newItem.Date
                };

                if (!isStarted)
                    maColls.ElementAt(i).AddLatestChartItem(item);
                else
                    maColls.ElementAt(i).UpdateLatestChartItem(item);
            }
        }

        private async void OnViewCurve(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;

            model.Reset();
            AdjustViews(true);
            await QueryChartCollection(id1);

        }

        private void OnViewCandle(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;

            model.Reset();
            AdjustViews(false);
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Low, () =>
            {
                QueryCandleCollection(id1);
            });
        }

        private async void OnViewMultiple(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;

            model.Reset();
            AdjustViews(true);
            await QueryMultipleCollection(id1);
        }

        private async void OnViewComparison(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;
            model.Reset();
            AdjustViews(true);
            await QueryCandleComparisonCollection(id1, id2);
        }

        private async void OnViewOverlap(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;

            model.Reset();
            AdjustViews(true);
            await QueryChartOverlapCollection(id1, id2);
        }

        private async void OnViewTimeLine(object sender, RoutedEventArgs e)
        {
            if (model == null)
                return;

            model.Reset();
            AdjustViews(false);
            await QueryTimeCollection(id3);
        }

        private async void OnViewCandleTrading(object sender, RoutedEventArgs e)
        {
            model.Reset();
            AdjustViews(false);
            await QueryCandleTrading(id3);
        }

        private async void OnViewTimeTrading(object sender, RoutedEventArgs e)
        {
            model.Reset();
            AdjustViews(false);
            await QueryTimeTrading(id3);
        }

        private void OnActionsNone(object sender, RoutedEventArgs e)
        {
            price.PointerStartAction = PointerAction.None;
            volumn.PointerStartAction = PointerAction.None;
        }

        private void OnActionsMeasure(object sender, RoutedEventArgs e)
        {
            price.PointerStartAction = PointerAction.Measure;
            volumn.PointerStartAction = PointerAction.Measure;
        }

        private void OnActionsZoomIn(object sender, RoutedEventArgs e)
        {
            price.PointerStartAction = PointerAction.ZoomIn;
            volumn.PointerStartAction = PointerAction.ZoomIn;
        }

        private void OnActionsSelect(object sender, RoutedEventArgs e)
        {
            price.PointerStartAction = PointerAction.Select;
            volumn.PointerStartAction = PointerAction.Select;
        }

        private async void price_SelectItems(object sender, SelectItemsEventArgs e)
        {
            MessageDialog dialog = new MessageDialog(string.Format("Select items range {0}-{1}",
                    e.Items.First().Date.ToString("yyyyMMdd"), e.Items.Last().Date.ToString("yyyyMMdd")), "Selection");
            await dialog.ShowAsync();
        }
        private void OnClickSettings(object sender, RoutedEventArgs e)
        {
            settingsGrid.Visibility = Visibility.Visible;
        }

        private void settingsEditor_ResultReturned(object sender, ResultEventArgs e)
        {
            settingsGrid.Visibility = Visibility.Collapsed;

            if(e.IsOk)
            {
                price.ForceDraw(true);
            }
        }
    }
}
