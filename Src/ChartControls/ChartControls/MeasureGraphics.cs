#region License
// Copyright (c) 2015 Wayne Gu
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion
using ChartControls.Drawing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;


#if USINGCANVAS
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// 尺子
    /// </summary>
    public class MeasureGraphics : FrameworkElement, ICustomGraphics
    {
        public static readonly DependencyProperty MeasureColorProperty;
        public static readonly DependencyProperty MeasureThicknessProperty;
        public static readonly DependencyProperty MeasureDashesProperty;

        public static readonly DependencyProperty FontFamilyProperty;
        public static readonly DependencyProperty FontSizeProperty;
        public static readonly DependencyProperty FontStretchProperty;
        public static readonly DependencyProperty FontStyleProperty;
        public static readonly DependencyProperty FontWeightProperty;
        public static readonly DependencyProperty ForegroundProperty;

        static MeasureGraphics()
        {
            MeasureColorProperty = DependencyProperty.Register("MeasureColor", typeof(Brush), typeof(MeasureGraphics), new PropertyMetadata(Brushes.Black));
            MeasureThicknessProperty = DependencyProperty.Register("MeasureThickness", typeof(double), typeof(MeasureGraphics), new PropertyMetadata(1.0));
            MeasureDashesProperty = DependencyProperty.Register("MeasureDashes", typeof(DoubleCollection), typeof(MeasureGraphics), new PropertyMetadata(null));

            FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(MeasureGraphics), new PropertyMetadata(FontConst.DefaultFontFamily, MeasureGraphic_PropertyChangedCallback));
            FontStretchProperty = DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(MeasureGraphics), new PropertyMetadata(FontStretches.Normal, MeasureGraphic_PropertyChangedCallback));
            FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(MeasureGraphics), new PropertyMetadata(FontStyles.Normal, MeasureGraphic_PropertyChangedCallback));
            FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(MeasureGraphics), new PropertyMetadata(FontWeights.Normal, MeasureGraphic_PropertyChangedCallback));
            FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(MeasureGraphics), new PropertyMetadata(FontConst.DefaultFontSize));
            ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(MeasureGraphics), new PropertyMetadata(Brushes.Black));
        }

        public Brush MeasureColor
        {
            get { return (Brush)GetValue(MeasureColorProperty); }
            set { SetValue(MeasureColorProperty, value); }
        }

        public double MeasureThickness
        {
            get { return (double)GetValue(MeasureThicknessProperty); }
            set { SetValue(MeasureThicknessProperty, value); }
        }

        public DoubleCollection MeasureDashes
        {
            get { return (DoubleCollection)GetValue(MeasureDashesProperty); }
            set { SetValue(MeasureDashesProperty, value); }
        }

        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        protected double measureValue;

        public MeasureGraphics()
        {
            PointCount = 2;
            Points = new Point[PointCount];
        }

        public int PointCount
        {
            get;
            set;
        }

        public Point[] Points
        {
            get;
            set;
        }

        public ValuePoint[] ValuePoints
        {
            get;
            set;
        }

        public virtual void UpdateValue(double value)
        {
            measureValue = value;
        }

        public void StartDraw(Point pt)
        {
            Points[0] = pt;
        }

        public bool NextDraw(Point pt, IDrawingContext dc, bool isFinal)
        {
            Points[1] = pt;

            double radius = Math.Atan((Points[1].X - Points[0].X) / (Points[0].Y - Points[1].Y));
            double degree = radius * 180 / Math.PI;
            if (degree < 0)
            {
                if (Points[1].X > Points[0].X)
                {
                    degree = 180 + degree;
                }
            }
            else
            {
                if (Points[1].X < Points[0].X)
                {
                    degree = 180 + degree;
                }
            }

            IPen pen = DrawingObjectFactory.CreatePen(MeasureColor, MeasureThickness);
            if(MeasureDashes != null)
            {
                pen.Dashes = MeasureDashes;
            }
            //Trace.WriteLine(radius + "," + degree);
            RotateTransform transferm = new RotateTransform()
            {
                Angle = degree,
                CenterX = Points[0].X,
                CenterY = Points[0].Y
            };

            dc.PushTransform(transferm);
            Point pt1 = new Point(Points[0].X + 3, Points[0].Y);
            Point pt2 = new Point(Points[0].X - 3, Points[0].Y);
            dc.DrawLine(pen, pt1, pt2);

            double value = -Math.Sqrt(Math.Abs(Points[0].X - Points[1].X) * Math.Abs(Points[0].X - Points[1].X) + Math.Abs(Points[0].Y - Points[1].Y) * Math.Abs(Points[0].Y - Points[1].Y));
            Point pt3 = new Point(Points[0].X, Points[0].Y + value);
            dc.DrawLine(pen, Points[0], pt3);

            pt1 = new Point(Points[0].X + 3, Points[0].Y + value);
            pt2 = new Point(Points[0].X - 3, Points[0].Y + value);
            dc.DrawLine(pen, pt1, pt2);

            dc.Pop();

            ITextFormat format = DrawingObjectFactory.CreateTextFormat(measureValue.ToString("P2"), FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
            
            Point ptText = Points[1];
            ptText.Y -= format.Height;
            dc.DrawText(format, ptText);
            return false;
        }

        public void Draw(IDrawingContext dc) { }

        public void Recalculate() { }

        public static void MeasureGraphic_PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //(d as MeasureGraphics).RecreateTypeface();
        }

    }
}
