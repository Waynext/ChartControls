using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ChartControls;
using ChartControls.Drawing;

namespace ChartView
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DataLoader loader;
        DataLoader timeLoader;

        public MainWindow()
        {
            InitializeComponent();

            loader = new DataLoader("s.json", true);
            timeLoader = new DataLoader("st.json", true);
            //CreateCurve();
            //CreateCandle();
            CreateTime();
        }

        public void CreateCurve()
        {
            string id = "000001";
            //Load chart items
            IList<ChartItem> chartItems = loader.GetChartItems(id);
            //Create collection id
            CollectionId collId = new CollectionId(id);

            //Create pen
            IPen pen = DrawingObjectFactory.CreatePen(Brushes.Black, 1);
            //Create chart item collection
            ChartItemCollection collection = new ChartItemCollection(collId, chartItems, pen, null);

            //Set main collection
            priceControl.SetMainCollection(collection);
        }

        public void CreateCandle()
        {
            string id = "000001";
            //Load chart items
            StockVolumnList svList = loader.GetStockItems(id);
            //Create collection id
            CollectionId collId = new CollectionId(id);
            //Create pens
            IPen raisePen = DrawingObjectFactory.CreatePen(Brushes.Red, 1);
            IPen fallPen = DrawingObjectFactory.CreatePen(Brushes.Green, 1);
            //Create stock item collection
            StockItemCollection stockColl = new StockItemCollection(collId, svList.Prices, raisePen, fallPen, null);
            //Set main collection
            priceControl.SetMainCollection(stockColl);
            //Create volumn item collection
            VolumnItemCollection volumnColl = new VolumnItemCollection(collId, svList.Volumns, raisePen, fallPen);
            //Set main collection
            volumnControl.SetMainCollection(volumnColl);
            //Connect two controls
            priceControl.AddConnection(volumnControl);
        }

        public void CreateTime()
        {
            string id = "600100";

            StockVolumnList svList = timeLoader.GetStockItems(id);

            //Create collection id
            CollectionId collId = new CollectionId(id);
            //Create pens
            IPen raisePen = DrawingObjectFactory.CreatePen(Brushes.Red, 1);
            IPen fallPen = DrawingObjectFactory.CreatePen(Brushes.Green, 1);
            //Create stock item collection
            SymmetricChartItemCollection stockColl = new SymmetricChartItemCollection(collId, svList.Prices, raisePen, null, SymmetricCommonSettings.CNSettings);
            //Set main collection
            priceControl.SetMainCollection(stockColl);
            //Create volumn item collection
            SymmetricVolumnItemCollection volumnColl = new SymmetricVolumnItemCollection(collId, svList.Volumns, raisePen, fallPen, SymmetricCommonSettings.CNSettings);
            //Set main collection
            volumnControl.SetMainCollection(volumnColl);
            //Connect two controls
            priceControl.AddConnection(volumnControl);
        }
    }
}
