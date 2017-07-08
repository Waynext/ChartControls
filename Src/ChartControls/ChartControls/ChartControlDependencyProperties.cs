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
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

#if USINGCANVAS
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
#if USINGCANVAS
    public partial class ChartControl : Canvas
    {
#else
    public partial class ChartControl : FrameworkElement
    {
        /// <summary>
        /// 背景颜色。
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty;
#endif
    
        #region Dependency Properties
        /// <summary>
        /// 边框颜色。
        /// </summary>
        public static readonly DependencyProperty BorderProperty;
        /// <summary>
        /// 边框宽度。
        /// </summary>
        public static readonly DependencyProperty BorderThicknessProperty;
        /// <summary>
        /// 光标线颜色。
        /// </summary>
        public static readonly DependencyProperty CursorLinesProperty;
        /// <summary>
        /// 光标线宽度。
        /// </summary>
        public static readonly DependencyProperty CursorLinesThicknessProperty;
        /// <summary>
        /// 光标线Dash样式。
        /// </summary>
        public static readonly DependencyProperty CursorLinesDashesProperty;
        /// <summary>
        /// y轴刻度位置。
        /// </summary>
        public static readonly DependencyProperty YScaleDockProperty;
        /// <summary>
        /// y轴刻度宽度。
        /// </summary>
        public static readonly DependencyProperty YScaleWidthProperty;
        /// <summary>
        /// x轴刻度位置。
        /// </summary>
        public static readonly DependencyProperty XScaleDockProperty;
        /// <summary>
        /// x轴刻度高度。
        /// </summary>
        public static readonly DependencyProperty XScaleHeightProperty;
        /// <summary>
        /// 选择框颜色。
        /// </summary>
        public static readonly DependencyProperty SelectionBorderColorProperty;
        /// <summary>
        /// 选择框宽度。
        /// </summary>
        public static readonly DependencyProperty SelectionBorderThicknessProperty;
        /// <summary>
        /// 选择框Dash样式。
        /// </summary>
        public static readonly DependencyProperty SelectionBorderDashesProperty;
        /// <summary>
        /// x轴刻度显示格式。
        /// </summary>
        public static readonly DependencyProperty XScaleFormatProperty;
        /// <summary>
        /// y轴刻度显示格式。
        /// </summary>
        public static readonly DependencyProperty YScaleFormatProperty;
        /// <summary>
        /// y轴游标显示格式。
        /// </summary>
        public static readonly DependencyProperty YCursorFormatProperty;
        /// <summary>
        /// y轴刻度线颜色。
        /// </summary>
        public static readonly DependencyProperty YScaleLineColorProperty;
        /// <summary>
        /// y轴刻度线宽度。
        /// </summary>
        public static readonly DependencyProperty YScaleLineThicknessProperty;
        /// <summary>
        /// y轴刻度线Dash样式。
        /// </summary>
        public static readonly DependencyProperty YScaleLineDashesProperty;

        /// <summary>
        /// x轴刻度线颜色。
        /// </summary>
        public static readonly DependencyProperty XScaleLineColorProperty;
        /// <summary>
        /// x轴刻度线宽度。
        /// </summary>
        public static readonly DependencyProperty XScaleLineThicknessProperty;
        /// <summary>
        /// x轴刻度线Dash样式。
        /// </summary>
        public static readonly DependencyProperty XScaleLineDashesProperty;

        /// <summary>
        /// 字体族。
        /// </summary>
        public static readonly DependencyProperty FontFamilyProperty;
        /// <summary>
        /// 字体大小。
        /// </summary>
        public static readonly DependencyProperty FontSizeProperty;
        /// <summary>
        /// 字体拉伸和压缩。
        /// </summary>
        public static readonly DependencyProperty FontStretchProperty;
        /// <summary>
        /// 字体样式。
        /// </summary>
        public static readonly DependencyProperty FontStyleProperty;
        /// <summary>
        /// 字体粗细。
        /// </summary>
        public static readonly DependencyProperty FontWeightProperty;
        /// <summary>
        /// 前景颜色。
        /// </summary>
        public static readonly DependencyProperty ForegroundProperty;

        /// <summary>
        /// 接触开始事件。
        /// </summary>
        public static readonly DependencyProperty PointerStartActionProperty;
        /// <summary>
        /// 坐标类型。
        /// </summary>
        public static readonly DependencyProperty CoordinateTypeProperty;
        /// <summary>
        /// 测量用尺图形。
        /// </summary>
        public static readonly DependencyProperty MeasureGraphicsProperty;

        /// <summary>
        /// 刻度划分是否是取整的。缺省是true，表示是取整的。false，表示是刻度平均分配的。
        /// </summary>
        public static readonly DependencyProperty IsScalesOptimizedProperty;
        /// <summary>
        /// Y轴刻度数量，缺省值是4。
        /// </summary>
        public static readonly DependencyProperty YColumnCountProperty;
        /// <summary>
        /// X轴刻度数量，缺省值是4。
        /// </summary>
        public static readonly DependencyProperty XColumnCountProperty;

#if USINGCANVAS
        private const PointerAction defaultStartAction = PointerAction.None;
#else
        private const PointerAction defaultStartAction = PointerAction.ZoomIn;
#endif
        private const CoordinateType defaultCoordinateType = CoordinateType.Linear;

        static ChartControl()
        {
#if USINGCANVAS
#else
            BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.White));
#endif
            BorderProperty = DependencyProperty.Register("Border", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Black));
            BorderThicknessProperty = DependencyProperty.Register("BorderThickness", typeof(double), typeof(ChartControl), new PropertyMetadata(1.0));

            CursorLinesProperty = DependencyProperty.Register("CursorLines", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Gray));
            CursorLinesThicknessProperty = DependencyProperty.Register("CursorLinesThickness", typeof(double), typeof(ChartControl), new PropertyMetadata(1.0));
            CursorLinesDashesProperty = DependencyProperty.Register("CursorLinesDashes", typeof(DoubleCollection), typeof(ChartControl), new PropertyMetadata(null));

            YScaleDockProperty = DependencyProperty.Register("YScaleDock", typeof(YScaleDock), typeof(ChartControl), new PropertyMetadata(YScaleDock.Right));
            YScaleWidthProperty = DependencyProperty.Register("YScaleWidth", typeof(double), typeof(ChartControl), new PropertyMetadata(60.0));
            XScaleDockProperty = DependencyProperty.Register("XScaleDock", typeof(XScaleDock), typeof(ChartControl), new PropertyMetadata(XScaleDock.Bottom));
            XScaleHeightProperty = DependencyProperty.Register("XScaleHeight", typeof(double), typeof(ChartControl), new PropertyMetadata(15.0));

            SelectionBorderColorProperty = DependencyProperty.Register("SelectionBorderColor", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Black));
            SelectionBorderThicknessProperty = DependencyProperty.Register("SelectionBorderThickness", typeof(double), typeof(ChartControl), new PropertyMetadata(1.0));
            SelectionBorderDashesProperty = DependencyProperty.Register("SelectionBorderDashes", typeof(DoubleCollection), typeof(ChartControl), new PropertyMetadata(null));

            XScaleFormatProperty = DependencyProperty.Register("XScaleFormat", typeof(string), typeof(ChartControl), new PropertyMetadata("yy/MM"));
            YScaleFormatProperty = DependencyProperty.Register("YScaleFormat", typeof(string), typeof(ChartControl), new PropertyMetadata("F2"));
            YCursorFormatProperty = DependencyProperty.Register("YCursorFormat", typeof(string), typeof(ChartControl), new PropertyMetadata("F2"));

            YScaleLineColorProperty = DependencyProperty.Register("YScaleLineColor", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Gray));
            YScaleLineThicknessProperty = DependencyProperty.Register("YScaleLineThickness", typeof(double), typeof(ChartControl), new PropertyMetadata(1.0));
            YScaleLineDashesProperty = DependencyProperty.Register("YScaleLineDashes", typeof(DoubleCollection), typeof(ChartControl), new PropertyMetadata(null));

            XScaleLineColorProperty = DependencyProperty.Register("XScaleLineColor", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Gray));
            XScaleLineThicknessProperty = DependencyProperty.Register("XScaleLineThickness", typeof(double), typeof(ChartControl), new PropertyMetadata(1.0));
            XScaleLineDashesProperty = DependencyProperty.Register("XScaleLineDashes", typeof(DoubleCollection), typeof(ChartControl), new PropertyMetadata(null));

            FontFamilyProperty = DependencyProperty.Register("FontFamily", typeof(FontFamily), typeof(ChartControl), new PropertyMetadata(FontConst.DefaultFontFamily));
            FontSizeProperty = DependencyProperty.Register("FontSize", typeof(double), typeof(ChartControl), new PropertyMetadata(FontConst.DefaultFontSize));
            FontStretchProperty = DependencyProperty.Register("FontStretch", typeof(FontStretch), typeof(ChartControl), new PropertyMetadata(FontStretches.Normal));

            FontStyleProperty = DependencyProperty.Register("FontStyle", typeof(FontStyle), typeof(ChartControl), new PropertyMetadata(FontStyles.Normal));
            FontWeightProperty = DependencyProperty.Register("FontWeight", typeof(FontWeight), typeof(ChartControl), new PropertyMetadata(FontWeights.Normal));
            ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Brush), typeof(ChartControl), new PropertyMetadata(Brushes.Black));

            PointerStartActionProperty = DependencyProperty.Register("PointerStartAction", typeof(PointerAction), typeof(ChartControl), new PropertyMetadata(defaultStartAction));
            CoordinateTypeProperty = DependencyProperty.Register("CoordinateType", typeof(CoordinateType), typeof(ChartControl), new PropertyMetadata(defaultCoordinateType, PropertyChangedCallback));

            MeasureGraphicsProperty = DependencyProperty.Register("MeasureGraphics", typeof(MeasureGraphics), typeof(ChartControl), new PropertyMetadata(null));


            IsScalesOptimizedProperty = DependencyProperty.Register("IsScalesOptimized", typeof(bool), typeof(ChartControl), new PropertyMetadata(true, PropertyChangedCallback));
            YColumnCountProperty = DependencyProperty.Register("YColumnCount", typeof(int), typeof(ChartControl), new PropertyMetadata(4, PropertyChangedCallback));
            XColumnCountProperty = DependencyProperty.Register("XColumnCount", typeof(int), typeof(ChartControl), new PropertyMetadata(4, PropertyChangedCallback));

#if USINGCANVAS
#else
            CursorMovedEvent = EventManager.RegisterRoutedEvent("CursorMoved", RoutingStrategy.Bubble, typeof(CursorMovedEventHandler), typeof(ChartControl));
            DataQueriedEvent = EventManager.RegisterRoutedEvent("DataQueried", RoutingStrategy.Bubble, typeof(DataQueriedEventHandler), typeof(ChartControl));
            SelectItemsEvent = EventManager.RegisterRoutedEvent("SelectItems", RoutingStrategy.Bubble, typeof(SelectItemsEventHandler), typeof(ChartControl));
#endif
        }

#if USINGCANVAS
#else
        /// <summary>
        /// 背景颜色。
        /// </summary>
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }
#endif
        /// <summary>
        /// 边框颜色。
        /// </summary>
        public Brush Border
        {
            get { return (Brush)GetValue(BorderProperty); }
            set { SetValue(BorderProperty, value); }
        }

        /// <summary>
        /// 边框宽度。
        /// </summary>
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        /// <summary>
        /// 光标线颜色。
        /// </summary>
        public Brush CursorLines
        {
            get { return (Brush)GetValue(CursorLinesProperty); }
            set { SetValue(CursorLinesProperty, value); }
        }

        /// <summary>
        /// 光标线宽度。
        /// </summary>
        public double CursorLinesThickness
        {
            get { return (double)GetValue(CursorLinesThicknessProperty); }
            set { SetValue(CursorLinesThicknessProperty, value); }
        }

        /// <summary>
        /// 光标线Dash样式。
        /// </summary>
        public DoubleCollection CursorLinesDashes
        {
            get { return (DoubleCollection)GetValue(CursorLinesDashesProperty); }
            set { SetValue(CursorLinesDashesProperty, value); }
        }

        /// <summary>
        /// y轴刻度位置。<see cref="ChartControls.YScaleDock"/>
        /// </summary>
        public YScaleDock YScaleDock
        {
            get { return (YScaleDock)GetValue(YScaleDockProperty); }
            set
            {
                if (YScaleDock != value)
                {
                    SetValue(YScaleDockProperty, value);
                }
            }
        }

        /// <summary>
        /// y轴刻度宽度。
        /// </summary>
        public double YScaleWidth
        {
            get { return (double)GetValue(YScaleWidthProperty); }
            set
            {
                if (YScaleWidth != value)
                {
                    SetValue(YScaleWidthProperty, value);
                }
            }
        }

        /// <summary>
        /// x轴刻度位置。<see cref="ChartControls.XScaleDock"/>
        /// </summary>
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

        /// <summary>
        /// x轴刻度高度。
        /// </summary>
        public double XScaleHeight
        {
            get { return (double)GetValue(XScaleHeightProperty); }
            set
            {
                if (XScaleHeight != value)
                {
                    SetValue(XScaleHeightProperty, value);
                }
            }
        }

        /// <summary>
        /// 选择框颜色。
        /// </summary>
        public Brush SelectionBorderColor
        {
            get
            {
                return (Brush)GetValue(SelectionBorderColorProperty);
            }
            set
            {
                SetValue(SelectionBorderColorProperty, value);
            }
        }

        /// <summary>
        /// 选择框宽度。
        /// </summary>
        public double SelectionBorderThickness
        {
            get { return (double)GetValue(SelectionBorderThicknessProperty); }
            set { SetValue(SelectionBorderThicknessProperty, value); }
        }

        /// <summary>
        /// 选择框Dash样式。
        /// </summary>
        public DoubleCollection SelectionBorderDashes
        {
            get { return (DoubleCollection)GetValue(SelectionBorderDashesProperty); }
            set { SetValue(SelectionBorderDashesProperty, value); }
        }
        
        /// <summary>
        /// x轴刻度显示格式。
        /// </summary>
        public string XScaleFormat //= "yy/MM";
        {
            get { return (string)GetValue(XScaleFormatProperty); }
            set { SetValue(XScaleFormatProperty, value); }
        }

        /// <summary>
        /// y轴刻度显示格式。
        /// </summary>
        public string YScaleFormat //= "F2";
        {
            get { return (string)GetValue(YScaleFormatProperty); }
            set { SetValue(YScaleFormatProperty, value); }
        }

        /// <summary>
        /// y轴游标显示格式。
        /// </summary>
        public string YCursorFormat //= "F2";
        {
            get { return (string)GetValue(YCursorFormatProperty); }
            set { SetValue(YCursorFormatProperty, value); }
        }

        /// <summary>
        /// y轴刻度线颜色。
        /// </summary>
        public Brush YScaleLineColor
        {
            get { return (Brush)GetValue(YScaleLineColorProperty); }
            set { SetValue(YScaleLineColorProperty, value); }
        }

        /// <summary>
        /// y轴刻度线宽度。
        /// </summary>
        public double YScaleLineThickness
        {
            get { return (double)GetValue(YScaleLineThicknessProperty); }
            set { SetValue(YScaleLineThicknessProperty, value); }
        }

        /// <summary>
        /// y轴刻度线Dash样式。
        /// </summary>
        public DoubleCollection YScaleLineDashes
        {
            get { return (DoubleCollection)GetValue(YScaleLineDashesProperty); }
            set { SetValue(YScaleLineDashesProperty, value); }
        }

        /// <summary>
        /// x轴刻度线颜色。
        /// </summary>
        public Brush XScaleLineColor
        {
            get { return (Brush)GetValue(XScaleLineColorProperty); }
            set { SetValue(XScaleLineColorProperty, value); }
        }

        /// <summary>
        /// x轴刻度线宽度。
        /// </summary>
        public double XScaleLineThickness
        {
            get { return (double)GetValue(XScaleLineThicknessProperty); }
            set { SetValue(XScaleLineThicknessProperty, value); }
        }

        /// <summary>
        /// x轴刻度线Dash样式。
        /// </summary>
        public DoubleCollection XScaleLineDashes
        {
            get { return (DoubleCollection)GetValue(XScaleLineDashesProperty); }
            set { SetValue(XScaleLineDashesProperty, value); }
        }
        /// <summary>
        /// 字体族。
        /// </summary>
        public FontFamily FontFamily
        {
            get { return (FontFamily)GetValue(FontFamilyProperty); }
            set { SetValue(FontFamilyProperty, value); }
        }

        /// <summary>
        /// 字体大小。
        /// </summary>
        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set { SetValue(FontSizeProperty, value); }
        }

        /// <summary>
        /// 字体拉伸和压缩。
        /// </summary>
        public FontStretch FontStretch
        {
            get { return (FontStretch)GetValue(FontStretchProperty); }
            set { SetValue(FontStretchProperty, value); }
        }

        /// <summary>
        /// 字体样式。
        /// </summary>
        public FontStyle FontStyle
        {
            get { return (FontStyle)GetValue(FontStyleProperty); }
            set { SetValue(FontStyleProperty, value); }
        }

        /// <summary>
        /// 字体粗细。
        /// </summary>
        public FontWeight FontWeight
        {
            get { return (FontWeight)GetValue(FontWeightProperty); }
            set { SetValue(FontWeightProperty, value); }
        }

        /// <summary>
        /// 前景颜色。
        /// </summary>
        public Brush Foreground
        {
            get { return (Brush)GetValue(ForegroundProperty); }
            set { SetValue(ForegroundProperty, value); }
        }

        /// <summary>
        /// 接触开始事件。<see cref="PointerAction"/>
        /// </summary>
        public PointerAction PointerStartAction
        {
            get { return (PointerAction)GetValue(PointerStartActionProperty); }
            set { SetValue(PointerStartActionProperty, value); }
        }

        /// <summary>
        /// 坐标类型。<see cref="ChartControls.CoordinateType"/>
        /// </summary>
        public CoordinateType CoordinateType
        {
            get { return (CoordinateType)GetValue(CoordinateTypeProperty); }
            set { SetValue(CoordinateTypeProperty, value); }
        }

        /// <summary>
        /// 测量用尺图形。<see cref="ChartControls.MeasureGraphics"/>
        /// </summary>
        public MeasureGraphics MeasureGraphics
        {
            get { return (MeasureGraphics)GetValue(MeasureGraphicsProperty); }
            set { SetValue(MeasureGraphicsProperty, value); }
        }

        /// <summary>
        /// 刻度划分是否是取整的。缺省是true，表示是取整的。false，表示是刻度平均分配的。
        /// </summary>
        public bool IsScalesOptimized
        {
            get { return (bool)GetValue(IsScalesOptimizedProperty); }
            set { SetValue(IsScalesOptimizedProperty, value); }

        }
        /// <summary>
        /// Y轴刻度数量，缺省值是4。
        /// </summary>
        public int YColumnCount
        {
            get { return (int)GetValue(YColumnCountProperty); }
            set { SetValue(YColumnCountProperty, value); }
        }

        /// <summary>
        /// X轴刻度数量，缺省值是4。
        /// </summary>
        public int XColumnCount
        {
            get { return (int)GetValue(XColumnCountProperty); }
            set { SetValue(XColumnCountProperty, value); }
        }

        /// <summary>
        /// 额外数据列表。<see cref="IExtraDataGraphics"/>
        /// </summary>
        public IEnumerable<IExtraDataGraphics> ExtraDataGraphics
        {
            get { return _extraDataGraphicsList; }
            set
            {
                _extraDataGraphicsList.Clear();
                foreach (var g in value)
                {
                    AddExtraDataGraphic(g);
                }
            }
        }
        #endregion

        #region Route Events
#if USINGCANVAS
        /// <summary>
        /// 光标移动事件。<see cref="CursorMovedRoutedEventArgs"/>
        /// </summary>
        public event EventHandler<CursorMovedRoutedEventArgs> CursorMoved;

        /// <summary>
        /// 动态数据请求事件。 <see cref="QueryDataEventArgs"/>
        /// </summary>
        public event EventHandler<QueryDataEventArgs> DataQueried;

        /// <summary>
        /// 选择事件。<see cref="SelectItemsEventArgs"/>
        /// </summary>
        public event EventHandler<SelectItemsEventArgs> SelectItems;

        // This method raises the NewColor event
        private void RaiseCursorMovedEvent(IEnumerable<CurrentChartItem> items)
        {
            var eventArgs = new CursorMovedRoutedEventArgs(items);
            if (CursorMoved != null)
            {
                CursorMoved(this, eventArgs);
            }
        }

        private void RaiseDataQueriedEvent(QueryData qData)
        {
            var eventArgs = new QueryDataEventArgs() { QueryData = qData };
            if (DataQueried != null)
            {
                DataQueried(this, eventArgs);
            }
        }

        private void RaiseSelectItemsEvent(IList<ChartItem> items)
        {
            var eventArgs = new SelectItemsEventArgs() { Items = items };
            if (SelectItems != null)
            {
                SelectItems(this, eventArgs);
            }
        }
#else
        /// <summary>
        /// 光标移动事件处理函数。<see cref="CursorMovedRoutedEventArgs"/>
        /// </summary>
        public delegate void CursorMovedEventHandler(object sender, CursorMovedRoutedEventArgs e);

        /// <summary>
        /// 动态数据请求事件。 <see cref="QueryDataEventArgs"/>
        /// </summary>
        public delegate void DataQueriedEventHandler(object sender, QueryDataEventArgs e);

        /// <summary>
        /// 选择事件处理函数。<see cref="SelectItemsEventArgs"/>
        /// </summary>
        public delegate void SelectItemsEventHandler(object sender, SelectItemsEventArgs e);

        /// <summary>
        /// 光标移动事件。<see cref="CursorMovedRoutedEventArgs"/>
        /// </summary>
        public static readonly RoutedEvent CursorMovedEvent;
        /// <summary>
        /// 动态数据请求事件处理函数。 <see cref="QueryDataEventArgs"/>
        /// </summary>
        public static readonly RoutedEvent DataQueriedEvent;
        /// <summary>
        /// 选择事件。<see cref="SelectItemsEventArgs"/>
        /// </summary>
        public static readonly RoutedEvent SelectItemsEvent;

        /// <summary>
        /// 光标移动事件。<see cref="CursorMovedRoutedEventArgs"/>
        /// </summary>
        public event CursorMovedEventHandler CursorMoved
        {
            add { AddHandler(CursorMovedEvent, value); }
            remove { RemoveHandler(CursorMovedEvent, value); }
        }

        /// <summary>
        /// 动态数据请求事件。 <see cref="QueryDataEventArgs"/>
        /// </summary>
        public event DataQueriedEventHandler DataQueried
        {
            add { AddHandler(DataQueriedEvent, value); }
            remove { RemoveHandler(DataQueriedEvent, value); }
        }

        /// <summary>
        /// 选择事件。<see cref="SelectItemsEventArgs"/>
        /// </summary>
        public event SelectItemsEventHandler SelectItems
        {
            add { AddHandler(SelectItemsEvent, value); }
            remove { RemoveHandler(SelectItemsEvent, value); }
        }
        // This method raises the NewColor event
        private void RaiseCursorMovedEvent(IEnumerable<CurrentChartItem> items)
        {
            var eventArgs = new CursorMovedRoutedEventArgs(items);
            eventArgs.RoutedEvent = ChartControl.CursorMovedEvent;
            RaiseEvent(eventArgs);
        }

        private void RaiseDataQueriedEvent(QueryData qData)
        {
            var eventArgs = new QueryDataEventArgs() { QueryData = qData };
            eventArgs.RoutedEvent = ChartControl.DataQueriedEvent;
            RaiseEvent(eventArgs);
        }

        private void RaiseSelectItemsEvent(IList<ChartItem> items)
        {
            var eventArgs = new SelectItemsEventArgs() { Items = items };
            eventArgs.RoutedEvent = ChartControl.SelectItemsEvent;
            RaiseEvent(eventArgs);
        }
#endif
        #endregion

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var chartHost = d as ChartControl;
            if (e.Property == CoordinateTypeProperty)
            {
                var newCoordiateType = (CoordinateType)e.NewValue;
                var oldCoordiateType = (CoordinateType)e.OldValue;
                chartHost.ChangeCoordinateType(newCoordiateType, oldCoordiateType);
            }
            else if(e.Property == IsScalesOptimizedProperty)
            {
                chartHost.SetIsScalesOptimized((bool)e.NewValue);
            }
            else if (e.Property == YColumnCountProperty)
            {
                chartHost.SetYColumnCount((int)e.NewValue);
            }
            else if (e.Property == XColumnCountProperty)
            {
                chartHost.SetXColumnCount((int)e.NewValue);
            }
        }

        private void SetIsScalesOptimized(bool isOptimized)
        {
            if(MainCollection != null)
            {
                MainCollection.IsScalesOptimized = isOptimized;
            }

            foreach(var coll in _assistCollections)
            {
                coll.IsScalesOptimized = isOptimized;
            }
        }

        private void SetYColumnCount(int yColumnCount)
        {
            if (MainCollection != null)
            {
                MainCollection.YColumnCount = yColumnCount;
            }

            foreach (var coll in _assistCollections)
            {
                coll.YColumnCount = yColumnCount;
            }
        }

        private void SetXColumnCount(int xColumnCount)
        {
            if (MainCollection != null)
            {
                MainCollection.XColumnCount = xColumnCount;
            }

            foreach (var coll in _assistCollections)
            {
                coll.XColumnCount = xColumnCount;
            }
        }
    }

    /// <summary>
    /// 光标移动事件参数。
    /// </summary>
    public class CursorMovedRoutedEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 构造函数。
        /// </summary>
        /// <param name="currentItems">数据项列表。<see cref="CurrentChartItem"/></param>
        public CursorMovedRoutedEventArgs(IEnumerable<CurrentChartItem> currentItems)
        {
            CurrentItems = currentItems;
        }

        /// <summary>
        /// 相关数据项列表。<see cref="CurrentChartItem"/>
        /// </summary>
        public IEnumerable<CurrentChartItem> CurrentItems
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 动态数据请求事件参数。
    /// </summary>
    public class QueryDataEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 请求数据。<see cref="ChartControls.QueryData"/>
        /// </summary>
        public QueryData QueryData
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 选择事件参数。
    /// </summary>
    public class SelectItemsEventArgs : RoutedEventArgs
    {
        /// <summary>
        /// 选择的数据项列表。<see cref="ChartItem"/>
        /// </summary>
        public IList<ChartItem> Items
        {
            get;
            set;
        }
    }
}
