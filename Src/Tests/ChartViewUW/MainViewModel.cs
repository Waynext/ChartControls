using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows;
using Windows.UI.Xaml.Media;
using Windows.UI;
using System.Reflection;
using ChartControls;

namespace ChartViewUW
{
    class MainViewModel : INotifyPropertyChanged
    {
        public const string priceName = "Price";
        public const string priceClrName = "PriceClr";
        public const string volumnName = "Volumn";
        public const string volumnClrName = "VolumnClr";
        public MainViewModel()
        {
            RaiseBrush = new SolidColorBrush(Colors.Red);
            FallBrush = new SolidColorBrush(Colors.Green);
            ContrastBrush = new SolidColorBrush(Colors.Black);
        }

        #region ChartControls properties
        public Brush RaiseBrush
        {
            get;
            set;
        }
    
        public Brush FallBrush
        {
            get;
            set;
        }

        public Brush ContrastBrush
        {
            get;
            set;
        }

        #endregion

        private string date;
        public string Date
        {
            get
            { return date; }
            set
            {
                date = value;
                ReportPropertyChanged("Date");
            }
        }
        public string Price1
        {
            get;
            set;
        }

        public string Price2
        {
            get;
            set;
        }

        public string Price3
        {
            get;
            set;
        }

        public string Price4
        {
            get;
            set;
        }

        public string Price5
        {
            get;
            set;
        }

        public string Price6
        {
            get;
            set;
        }

        public string Price7
        {
            get;
            set;
        }

        public string Price8
        {
            get;
            set;
        }

        public Brush PriceClr1
        {
            get;
            set;
        }

        public Brush PriceClr2
        {
            get;
            set;
        }

        public Brush PriceClr3
        {
            get;
            set;
        }

        public Brush PriceClr4
        {
            get;
            set;
        }

        public Brush PriceClr5
        {
            get;
            set;
        }

        public Brush PriceClr6
        {
            get;
            set;
        }

        public Brush PriceClr7
        {
            get;
            set;
        }

        public Brush PriceClr8
        {
            get;
            set;
        }

        public string Volumn1
        {
            get;
            set;
        }

        public string Volumn2
        {
            get;
            set;
        }

        public string Volumn3
        {
            get;
            set;
        }

        public Brush VolumnClr1
        {
            get;
            set;
        }

        public Brush VolumnClr2
        {
            get;
            set;
        }

        public Brush VolumnClr3
        {
            get;
            set;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ReportPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        public void SetValue<T>(int i, string namePrefix, T value)
        {
            string name = namePrefix + i;

            var type = typeof(MainViewModel).GetTypeInfo();
            var property = type.DeclaredProperties.FirstOrDefault(p => p.Name == name);
            if (property != null)
            {
                var oldValue = (T)property.GetValue(this, null);
                if(oldValue == null || !oldValue.Equals(value))
                {
                    property.SetValue(this, value, null);
                    ReportPropertyChanged(name);
                }
                
            }
        }

        public T GetValue<T>(int i, string namePrefix)
        {
            string priceName = namePrefix + i;
            var type = typeof(MainViewModel).GetTypeInfo();
            var property = type.DeclaredProperties.FirstOrDefault(p => p.Name == priceName);
            if (property != null)
            {
                return (T)property.GetValue(this, null);
            }
            else
            {
                return default(T);
            }
        }

        public void Reset()
        {
            for(int i = 0; i <= 8; i ++)
            {
                SetValue<string>(i, priceName, null);
                SetValue<Brush>(i, priceClrName, null);
            }
        }

        #region ChartControls Properties
        private int borderThickness = 1;
        public int BorderThickness
        {
            get
            {
                return borderThickness;
            }
            set
            {
                borderThickness = value;
                ReportPropertyChanged("BorderThickness");
            }
        }

        private string border = "Black";
        public string Border
        {
            get
            {
                return border;
            }
            set
            {
                border = value;
                ReportPropertyChanged("Border");
            }
        }

        private YScaleDock yScaleDock = YScaleDock.Right;
        public YScaleDock YScaleDock
        {
            get
            {
                return yScaleDock;
            }
            set
            {
                yScaleDock = value;
                ReportPropertyChanged("YScaleDock");
            }
        }

        private XScaleDock xScaleDock = XScaleDock.Bottom;
        public XScaleDock XScaleDock
        {
            get
            {
                return xScaleDock;
            }
            set
            {
                xScaleDock = value;
                ReportPropertyChanged("XScaleDock");
            }
        }

        private const string whiteColor = "White";
        private const string blackColor = "Black";
        private const string grayColor = "Gray";

        private string background = whiteColor;
        public string Background
        {
            get
            {
                return background;
            }
            set
            {
                background = value;
                ReportPropertyChanged("Background");
                if(background == whiteColor)
                {
                    Border = Foreground = blackColor;
                }
                else if(background == blackColor)
                {
                    Border = Foreground = whiteColor;
                }
            }
        }

        private string foreground = blackColor;
        public string Foreground
        {
            get
            {
                return foreground;
            }
            set
            {
                foreground = value;
                ReportPropertyChanged("Foreground");

            }
        }

        private int yScaleWidth = 60;
        public int YScaleWidth
        {
            get
            {
                return yScaleWidth;
            }
            set
            {
                yScaleWidth = value;
                ReportPropertyChanged("YScaleWidth");
            }
        }
        private int xScaleHeight = 15;
        public int XScaleHeight
        {
            get
            {
                return xScaleHeight;
            }
            set
            {
                xScaleHeight = value;
                ReportPropertyChanged("XScaleHeight");
            }
        }

        private CoordinateType coordinateType = CoordinateType.Linear;
        public CoordinateType CoordinateType
        {
            get
            {
                return coordinateType;
            }
            set
            {
                coordinateType = value;
                ReportPropertyChanged("CoordinateType");
            }
        }

        private string cursorLines = grayColor;
        public string CursorLines
        {
            get
            {
                return cursorLines;
            }
            set
            {
                cursorLines = value;
                ReportPropertyChanged("CursorLines");

            }
        }

        private int cursorLinesThickness = 1;
        public int CursorLinesThickness
        {
            get
            {
                return cursorLinesThickness;
            }
            set
            {
                cursorLinesThickness = value;
                ReportPropertyChanged("CursorLinesThickness");

            }
        }

        private string cursorLinesDashes = null;
        public string CursorLinesDashes
        {
            get { return cursorLinesDashes; }
            set {
                cursorLinesDashes = value;
                ReportPropertyChanged("CursorLinesDashes");
            }
        }

        private string scaleLineColor = grayColor;
        public string ScaleLineColor
        {
            get
            {
                return scaleLineColor;
            }
            set
            {
                scaleLineColor = value;
                ReportPropertyChanged("ScaleLineColor");
            }
        }

        private int scaleLineThickness = 1;
        public int ScaleLineThickness
        {
            get
            {
                return scaleLineThickness;
            }
            set
            {
                scaleLineThickness = value;
                ReportPropertyChanged("ScaleLineThickness");

            }
        }

        private string scaleLineDashes = null;
        public string ScaleLineDashes
        {
            get { return scaleLineDashes; }
            set
            {
                scaleLineDashes = value;
                ReportPropertyChanged("ScaleLineDashes");
            }
        }

        private string fontFamily = "Arial";
        public string FontFamily
        {
            get
            {
                return fontFamily;
            }
            set
            {
                fontFamily = value;
                ReportPropertyChanged("FontFamily");
            }
        }

        private int fontSize = 10;
        public int FontSize
        {
            get { return fontSize; }
            set {
                fontSize = value;
                ReportPropertyChanged("FontSize");
            }
        }

        #endregion

        #region array
        public static readonly int[] widthArrayDefault = new int[] { 0, 1, 2, 3, 4, 5 };
        public static readonly string[] brushesDefault = new string[] { "Red", "Green", "White", "Blue", "Orange", "Purple", "Black", "Gray" };
        public static readonly YScaleDock[] yScaleDocksDefault = new YScaleDock[] { YScaleDock.Left, YScaleDock.Right, YScaleDock.InnerLeft, YScaleDock.InnerRight, YScaleDock.None };
        public static readonly XScaleDock[] xScaleDocksDefault = new XScaleDock[] { XScaleDock.None, XScaleDock.Bottom };
        public static readonly string[] bgBrushesDefault = new string[] { "White", "Black" };
        public static readonly int[] dockWidthArrayDefault = new int[] { 15, 20, 30, 40, 50, 60, 80, 100 };
        public static readonly CoordinateType[] coordinateTypesDefault = new CoordinateType[] { CoordinateType.Linear, CoordinateType.Log10, CoordinateType.Percentage };
        public static readonly string[] dashArrayDefault = new string[] { "1,2", "2,2", "2,4", "4,4" };
        public static readonly string[] fontNamesDefault = new string[] { "Arial", "sans-serif", "Comic Sans MS", "Courier New", "Georgia", "Lucida Console" };
        public static readonly int[] fontSizesDefault = new int[] { 6, 8, 10, 12, 14, 16, 18, 20, 30 };

        public int[] WidthArray
        {
            get
            {
                return widthArrayDefault;
            }
        }

        public string[] Brushes
        {
            get
            {
                return brushesDefault;
            }
        }
        public YScaleDock[] YScaleDocks
        {
            get
            {
                return yScaleDocksDefault;
            }
        }
        public XScaleDock[] XScaleDocks
        {
            get
            {
                return xScaleDocksDefault;
            }
        }
        public string[] BgBrushes
        {
            get
            {
                return bgBrushesDefault;
            }
        }
        public int[] DockWidthArray
        {
            get
            {
                return dockWidthArrayDefault;
            }
        }
        public CoordinateType[] CoordinateTypes
        {
            get
            {
                return coordinateTypesDefault;
            }
        }
        public string[] DashArray
        {
            get
            {
                return dashArrayDefault;
            }
        }
        public string[] FontNames
        {
            get
            {
                return fontNamesDefault;
            }
        }
        public int[] FontSizes
        {
            get
            {
                return fontSizesDefault;
            }
        }
        #endregion
    }
}
