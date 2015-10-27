using ChartControls;
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

namespace ChartControlsDemo
{
    public class ResultEventArgs : EventArgs
    {
        public ResultEventArgs(bool isOk)
        {
            IsOk = isOk;
        }

        public bool IsOk
        {
            set;
            get;
        }
    }
    /// <summary>
    /// Interaction logic for SettingsEditor.xaml
    /// </summary>
    public partial class SettingsEditor : UserControl
    {
        public static readonly DependencyProperty BorderThicknessProperty;
        public static readonly DependencyProperty BorderProperty;
        public static readonly DependencyProperty YScaleDockProperty;
        public static readonly DependencyProperty XScaleDockProperty;
        public static readonly DependencyProperty ControlBackgroundProperty;
        public static readonly DependencyProperty YScaleWidthProperty;
        public static readonly DependencyProperty XScaleHeightProperty;
        public static readonly DependencyProperty CoordinateTypeProperty;
        public static readonly DependencyProperty CursorLinesProperty;
        public static readonly DependencyProperty CursorLinesThicknessProperty;
        public static readonly DependencyProperty CursorLinesDashesProperty;
        public static readonly DependencyProperty ScaleLineColorProperty;
        public static readonly DependencyProperty ScaleLineThicknessProperty;
        public static readonly DependencyProperty ScaleLineDashesProperty;
        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty FontSizeProperty;

        static SettingsEditor()
        {
            YScaleWidthProperty = DependencyProperty.Register("YScaleWidth", typeof(int), typeof(SettingsEditor), new PropertyMetadata(60));
            XScaleHeightProperty = DependencyProperty.Register("XScaleHeight", typeof(int), typeof(SettingsEditor), new PropertyMetadata(15));
            BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(int), typeof(SettingsEditor), new PropertyMetadata(1));
            BorderProperty = DependencyProperty.Register("Border", typeof(string), typeof(SettingsEditor), new PropertyMetadata("Black"));
            YScaleDockProperty = DependencyProperty.Register("YScaleDock", typeof(YScaleDock), typeof(SettingsEditor), new PropertyMetadata(YScaleDock.Right));
            ControlBackgroundProperty = DependencyProperty.Register("ControlBackground", typeof(string), typeof(SettingsEditor), new PropertyMetadata("White"));
            XScaleDockProperty = DependencyProperty.Register("XScaleDock", typeof(XScaleDock), typeof(SettingsEditor), new PropertyMetadata(XScaleDock.Bottom));
            CoordinateTypeProperty = DependencyProperty.Register("CoordinateType", typeof(CoordinateType), typeof(SettingsEditor), new PropertyMetadata(CoordinateType.Linear));
            CursorLinesProperty = DependencyProperty.Register("CursorLines", typeof(string), typeof(SettingsEditor), new PropertyMetadata("Gray"));
            CursorLinesThicknessProperty = DependencyProperty.Register("CursorLinesThickness", typeof(int), typeof(SettingsEditor), new PropertyMetadata(1));
            CursorLinesDashesProperty = DependencyProperty.Register("CursorLinesDashes", typeof(DoubleCollection), typeof(SettingsEditor), new PropertyMetadata(null));

            ScaleLineColorProperty = DependencyProperty.Register("ScaleLineColor", typeof(string), typeof(SettingsEditor), new PropertyMetadata("Gray"));
            ScaleLineThicknessProperty = DependencyProperty.Register("ScaleLineThickness", typeof(int), typeof(SettingsEditor), new PropertyMetadata(1));
            ScaleLineDashesProperty = DependencyProperty.Register("ScaleLineDashes", typeof(DoubleCollection), typeof(SettingsEditor), new PropertyMetadata(null));

            FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(string), typeof(SettingsEditor), new PropertyMetadata("Arial"));
            FontSizeProperty = DependencyProperty.Register("FontSize", typeof(int), typeof(SettingsEditor), new PropertyMetadata(10));
        }

        public SettingsEditor()
        {
            InitializeComponent();
        }

        public int BorderThickness
        {
            get
            {
                return (int)GetValue(BorderThicknessProperty);
            }
            set
            {
                SetValue(BorderThicknessProperty, value);
            }
        }

        public string Border
        {
            get
            {
                return (string)GetValue(BorderProperty);
            }
            set
            {
                SetValue(BorderProperty, value);
            }
        }

        public YScaleDock YScaleDock
        {
            get
            {
                return (YScaleDock)GetValue(YScaleDockProperty);
            }
            set
            {
                SetValue(YScaleDockProperty, value);
            }
        }

        public XScaleDock XScaleDock
        {
            get
            {
                return (XScaleDock)GetValue(XScaleDockProperty);
            }
            set
            {
                SetValue(XScaleDockProperty, value);
            }
        }

        public CoordinateType CoordinateType
        {
            get
            {
                return (CoordinateType)GetValue(CoordinateTypeProperty);
            }
            set
            {
                SetValue(CoordinateTypeProperty, value);
            }
        }
        public string ControlBackground
        {
            get
            {
                return (string)GetValue(ControlBackgroundProperty);
            }
            set
            {
                SetValue(ControlBackgroundProperty, value);
            }
        }

        public int YScaleWidth
        {
            get
            {
                return (int)GetValue(YScaleWidthProperty);
            }
            set
            {
                SetValue(YScaleWidthProperty, value);
            }
        }

        public int XScaleHeight
        {
            get
            {
                return (int)GetValue(XScaleHeightProperty);
            }
            set
            {
                SetValue(XScaleHeightProperty, value);
            }
        }

        public string CursorLines
        {
            get
            {
                return (string)GetValue(CursorLinesProperty);
            }
            set
            {
                SetValue(CursorLinesProperty, value);
            }
        }

        public double CursorLinesThickness
        {
            get { return (int)GetValue(CursorLinesThicknessProperty); }
            set { SetValue(CursorLinesThicknessProperty, value); }
        }

        public DoubleCollection CursorLinesDashes
        {
            get { return (DoubleCollection)GetValue(CursorLinesDashesProperty); }
            set { SetValue(CursorLinesDashesProperty, value); }
        }

        public string ScaleLineColor
        {
            get
            {
                return (string)GetValue(ScaleLineColorProperty);
            }
            set
            {
                SetValue(ScaleLineColorProperty, value);
            }
        }

        public double ScaleLineThickness
        {
            get { return (int)GetValue(ScaleLineThicknessProperty); }
            set { SetValue(ScaleLineThicknessProperty, value); }
        }

        public DoubleCollection ScaleLineDashes
        {
            get { return (DoubleCollection)GetValue(ScaleLineDashesProperty); }
            set { SetValue(ScaleLineDashesProperty, value); }
        }

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public int FontSize
        {
            get { return (int)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }
        public event EventHandler<ResultEventArgs> ResultReturned;

        private void RevokeOk()
        {
            if (ResultReturned != null)
                ResultReturned(this, new ResultEventArgs(true));
        }

        private void RevokeCancel()
        {
            if (ResultReturned != null)
                ResultReturned(this, new ResultEventArgs(false));
        }

        private void OnClickOk(object sender, RoutedEventArgs e)
        {
            RevokeOk();
            
        }

        private void OnClickCancel(object sender, RoutedEventArgs e)
        {
            RevokeCancel();
        }
    }
}
