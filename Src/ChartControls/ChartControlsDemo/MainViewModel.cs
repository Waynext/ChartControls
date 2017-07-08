using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows;
using ChartControls;

namespace ChartControlsDemo
{
    class MainViewModel : INotifyPropertyChanged
    {
        public const string priceName = "Price";
        public const string priceClrName = "PriceClr";
        public const string volumnName = "Volumn";
        public const string volumnClrName = "VolumnClr";
        public MainViewModel()
        {
            RaiseBrush = Brushes.Red;
            FallBrush = Brushes.Green;
            ContrastBrush = Brushes.Black;
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
            var property = GetType().GetProperties().FirstOrDefault(p => p.Name == name);
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
            var property = GetType().GetProperties().FirstOrDefault(p => p.Name == priceName);
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

        private DoubleCollection cursorLinesDashes = null;
        public DoubleCollection CursorLinesDashes
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

        private DoubleCollection scaleLineDashes = null;
        public DoubleCollection ScaleLineDashes
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

        private bool isScalesOptimized = true;
        public bool IsScalesOptimized
        {
            get { return isScalesOptimized; }
            set { isScalesOptimized = value;
                ReportPropertyChanged("IsScalesOptimized");
            }

        }

        private int yColumnCount = 4;
        public int YColumnCount
        {
            get { return yColumnCount; }
            set { yColumnCount = value;
                ReportPropertyChanged("YColumnCount");
            }
        }

        private int xColumnCount = 4;
        public int XColumnCount
        {
            get { return xColumnCount; }
            set
            {
                xColumnCount = value;
                ReportPropertyChanged("XColumnCount");
            }

        }
        #endregion
    }
}
