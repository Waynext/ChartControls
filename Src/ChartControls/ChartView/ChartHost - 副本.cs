using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ChartView
{
    public enum YScaleDock { Left, Right, InnerLeft, InnerRight, None };
    public enum XScaleDock { None, Bottom }

    public class ChartHost : FrameworkElement
    {
        #region Private members
        private VisualCollection _children;

        private DrawingVisual _cursorVisual;

        private DrawingVisual _mainVisual;
        private ChartItemCollection _mainCollection;
        private List<DrawingVisual> _assistVisuals = new List<DrawingVisual>();
        private List<ChartItemCollection> _assistCollections = new List<ChartItemCollection>();

        private Rect _collectionRect;
        enum MouseAction
        {
            None,
            Select,
            Measure
        }

        private MouseAction mouseAction = MouseAction.None;
        private Brush mouseSelectRectColor = Brushes.Black;
        private double mouseSelectRectThickness = 1;

        private Brush mouseRulerColor = Brushes.Black;
        private double mouseRulerThickness = 1;
        private string rulerFontFamily = "Verdana";
        private double rulerFontSize = 10;

        private Brush scaleColor = Brushes.Black;
        private double scaleThickness = 1;
        private string scaleFontFamily = "Verdana";
        private double scaleFontSize = 10;
        private Thickness ChartMargin = new Thickness(0, 0, 60, 15);
        private string scaleNumberFormat = "F2";
        private string cursorFontFamily = "Verdana";
        private double cursorFontSize = 10;
        private Brush cursorTextColor = Brushes.Black;

        private string timeScaleFormat = "yy/MM";
        private YScaleDock yScaleDock = YScaleDock.Right;
        private XScaleDock xScaleDock = XScaleDock.Bottom;

        private MouseAction startAction = MouseAction.Measure;

        private const int HorizationMargin = 15;
        private const int BottomMargin = 15;
        #endregion

        #region Dependency Properties
        public static readonly DependencyProperty BackgroundProperty;
        public static readonly DependencyProperty CursorColorProperty;
        public static readonly DependencyProperty CursorThicknessProperty;
        public static readonly DependencyProperty CursorDashStyleProperty;
        public static readonly DependencyProperty BorderColorProperty;
        public static readonly DependencyProperty BorderThicknessProperty;
        public static readonly DependencyProperty YAxisFontProperty;
        public static readonly DependencyProperty YAxisColorProperty;
        public static readonly DependencyProperty YScaleDockProperty;
        public static readonly DependencyProperty XScaleDockProperty;

        static ChartHost()
        {
            BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(ChartHost), new UIPropertyMetadata(null));
            CursorColorProperty = DependencyProperty.Register("CursorColor", typeof(Brush), typeof(ChartHost), new UIPropertyMetadata(null));
            CursorThicknessProperty = DependencyProperty.Register("CursorThickness", typeof(double), typeof(ChartHost), new UIPropertyMetadata(null));
            CursorDashStyleProperty = DependencyProperty.Register("CursorDashStyle", typeof(DashStyle), typeof(ChartHost), new UIPropertyMetadata(null));

            YScaleDockProperty = DependencyProperty.Register("YScaleDock", typeof(YScaleDock), typeof(ChartHost), new UIPropertyMetadata(null));
            XScaleDockProperty = DependencyProperty.Register("XScaleDock", typeof(XScaleDock), typeof(ChartHost), new UIPropertyMetadata(null));
            CursorMovedEvent = EventManager.RegisterRoutedEvent("CursorMoved", RoutingStrategy.Bubble, typeof(CursorMovedEventHandler), typeof(ChartHost));
        }

        /// <summary>
        /// The image displayed by the button.
        /// </summary>
        /// <remarks>The image is specified in XAML as an absolute or relative path.</remarks>
        [Description("The background color"), Category("Common Properties")]
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        [Description("The cursor color"), Category("Common Properties")]
        public Brush CursorColor
        {
            get { return (Brush)GetValue(CursorColorProperty); }
            set { SetValue(CursorColorProperty, value); }
        }

        [Description("The cursor line thickness"), Category("Common Properties")]
        public double CursorThickness
        {
            get { return (double)GetValue(CursorThicknessProperty); }
            set { SetValue(CursorThicknessProperty, value); }
        }

        [Description("The cursor line style"), Category("Common Properties")]
        public DashStyle CursorDashStyle
        {
            get { return (DashStyle)GetValue(CursorDashStyleProperty); }
            set { SetValue(CursorDashStyleProperty, value); }
        }

        [Description("The border color"), Category("Common Properties")]
        public Brush BorderColor
        {
            get { return (Brush)GetValue(BorderColorProperty); }
            set { SetValue(BorderColorProperty, value); }
        }

        [Description("The border thickness"), Category("Common Properties")]
        public double BorderThickness
        {
            get { return (double)GetValue(BorderThicknessProperty); }
            set { SetValue(BorderThicknessProperty, value); }
        }

        [Description("The y axis font"), Category("Common Properties")]
        public FontFamily YAxisFont
        {
            get { return (FontFamily)GetValue(YAxisFontProperty); }
            set { SetValue(YAxisFontProperty, value); }
        }

        [Description("The y axis text color"), Category("Common Properties")]
        public Brush YAxisColor
        {
            get { return (Brush)GetValue(YAxisColorProperty); }
            set { SetValue(YAxisColorProperty, value); }
        }

        [Description("The y axis location"), Category("Common Properties")]
        public YScaleDock YScaleDock
        {
            get { return (YScaleDock)GetValue(YScaleDockProperty); }
            set { SetValue(YScaleDockProperty, value); }
        }

        [Description("The y axis location"), Category("Common Properties")]
        public XScaleDock XScaleDock
        {
            get { return (XScaleDock)GetValue(XScaleDockProperty); }
            set { SetValue(XScaleDockProperty, value); RecalcuteChartMargin(); }
        }

        private void RecalcuteChartMargin()
        {
            var margin = new Thickness();
            if (XScaleDock == XScaleDock.Bottom)
            {
                margin.Bottom += 15;
            }
            switch (YScaleDock)
            {
                case YScaleDock.Left:
                    margin.Left += 60;
                    break;
                case YScaleDock.Right:
                    margin.Right += 60;
                    break;
            }
            ChartMargin = margin;
        }
        #endregion

        #region Route Events
        public delegate void CursorMovedEventHandler (object sender, CursorMovedRoutedEventArgs e);

        public static readonly RoutedEvent CursorMovedEvent;

        // Provide CLR accessors for the event
        public event RoutedEventHandler CursorMoved
        {
            add { AddHandler(CursorMovedEvent, value); }
            remove { RemoveHandler(CursorMovedEvent, value); }
        }

        // This method raises the NewColor event
        private void RaiseCursorMovedEvent(IEnumerable<CurrentChartItem> items)
        {
            var eventArgs = new CursorMovedRoutedEventArgs(items);
            eventArgs.RoutedEvent = ChartHost.CursorMovedEvent;
            RaiseEvent(eventArgs);
        }

        #endregion

        #region Public constructors and methods

        public ChartHost()
        {
            _children = new VisualCollection(this);

            this.MouseMove += ChartHost_MouseMove;
            this.SizeChanged += ChartHost_SizeChanged;
            this.KeyDown += ChartHost_KeyDown;
            this.MouseLeftButtonDown += ChartHost_MouseLeftButtonDown;
            this.MouseLeftButtonUp += ChartHost_MouseLeftButtonUp;
            this.LostMouseCapture += ChartHost_LostMouseCapture;
            this.MouseLeave += ChartHost_LostMouseCapture;

            Focusable = true;
            FocusVisualStyle = null;
        }

        public void SetMainCollection(ChartItemCollection collection)
        {
            this._mainCollection = collection;
            DrawVisuals();
        }

        public void AddAssistCollection(ChartItemCollection collection)
        {
            collection.AssistTo(_mainCollection);
            _assistCollections.Add(collection);

            DrawVisuals();
        }
        
        private List<WeakReference<ChartHost>> connectionList = new List<WeakReference<ChartHost>>();

        public void AddConnection(ChartHost otherChart)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart == otherChart)
                        return;
                }
            }

            connectionList.Add(new WeakReference<ChartHost>(otherChart));

            otherChart.AddConnection(this);
        }

        #endregion

        #region Event Methods

        private Point? startPoint;
        private Point? midPoint;
        private void ChartHost_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!IsFocused)
                this.Focus();

            startPoint = e.GetPosition(this);

            var chartRect = GetChartRect();
            if (!IsPointInChart(startPoint.Value))
            {
                return;
            }

            if (mouseAction == MouseAction.None)
            {
                midPoint = null;
                mouseAction = startAction;
            }

            
            //var pos = e.GetPosition(this);
            //CreateCursor(pos, true);
            //CreateCursor4ConnectionList(e);
        }

        private void ChartHost_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var curPos = e.GetPosition(this);
            var chartRect = GetChartRect();
            if (!IsPointInChart(curPos))
            {
                return;
            }
            if (startPoint != null && startPoint.Value.X == curPos.X && startPoint.Value.Y == curPos.Y)
            {
                startPoint = null;
            }
            else if (mouseAction == MouseAction.Select)
            {
                if (startPoint != null)
                {
                    ShowRegion(startPoint.Value, curPos);
                    ShowRegion4ConnectionList(startPoint.Value, curPos);

                    startPoint = null;
                }

                if (midPoint != null)
                {
                    _children.Remove(_cursorVisual);
                    _cursorVisual = null;
                }
            }
            else if(mouseAction == MouseAction.Measure)
            {
                startPoint = null;
                _children.Remove(_cursorVisual);
                _cursorVisual = null;
            }

            mouseAction = MouseAction.None;
        }

        private void ChartHost_MouseMove(object sender, MouseEventArgs e)
        {
            if (CursorColor == null)
                return;

            var cursorPos = e.GetPosition(this);

            var chartRect = GetChartRect();
            if (!IsPointInChart(cursorPos))
            {
                DropMouseAction();
                return;
            }

            midPoint = cursorPos;

            if (mouseAction == MouseAction.None)
            {
                MoveCursor(cursorPos, true);

                MoveCursor4ConnectionList(e);
            }
            else if(mouseAction == MouseAction.Select)
            {
                CreateSelectionRectVisual(AdjustPoint(startPoint.Value), AdjustPoint(cursorPos));
            }
            else if (mouseAction == MouseAction.Measure)
            {
                CreateRulerVisual(AdjustPoint(startPoint.Value), AdjustPoint(cursorPos), _mainCollection.GetChange(startPoint.Value.Y, cursorPos.Y));
            }
        }

        private void ChartHost_LostMouseCapture(object sender, MouseEventArgs e)
        {
            DropMouseAction();
        }

        private void DropMouseAction()
        {
            if (mouseAction == MouseAction.Select || mouseAction == MouseAction.Measure)
            {
                mouseAction = MouseAction.None;
                startPoint = null;
                _children.Remove(_cursorVisual);
                _cursorVisual = null;
            }
        }

        void ChartHost_KeyDown(object sender, KeyEventArgs e)
        {
            int steps = 0;
            if (Keyboard.Modifiers == ModifierKeys.None)
            {
                if (e.Key == Key.Left)
                {
                    steps = -1;
                }
                else if (e.Key == Key.Right)
                {
                    steps = 1;
                }
                else if (e.Key == Key.Up)
                {
                    ZoomChart(false);
                    ZoomChart4ConnectionList(false);
                }
                else if (e.Key == Key.Down)
                {
                    ZoomChart(true);
                    ZoomChart4ConnectionList(true);
                }

                if (steps != 0)
                {
                    MoveCursor(steps, true);
                    MoveCursor4ConnectionList(steps);
                }


            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Key == Key.Left)
                {
                    steps = -20;
                }
                else if (e.Key == Key.Right)
                {
                    steps = 20;
                }
                MoveChart(steps);
                MoveChart4ConnectionList(steps);
            }
        }

        void ChartHost_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Trace.WriteLine(string.Format("{0}-{1}", e.PreviousSize.ToString(), e.NewSize.ToString()));
            
            _children.Clear();

            Size newSize = new Size(e.NewSize.Width - ChartMargin.Left - ChartMargin.Right, e.NewSize.Height - ChartMargin.Top - ChartMargin.Bottom);
            if (_mainCollection != null)
            {
                _mainCollection.Resize(newSize);
            }

            foreach (var assistColl in _assistCollections)
            {
                assistColl.Resize(newSize);
            }
            

            if(_mainCollection != null)
                DrawVisuals();

            _collectionRect = GetChartRect();
        }

        #endregion

        #region Private methods

        private Rect GetChartRect()
        {
            return new Rect(ChartMargin.Left, ChartMargin.Top, (int)(ActualWidth - ChartMargin.Left - ChartMargin.Right), 
                (int)(ActualHeight - ChartMargin.Top - ChartMargin.Bottom));
        }

        private bool IsPointInChart(Point pt)
        {
            return pt.X >= _collectionRect.Left && pt.X < _collectionRect.Right &&
                pt.Y >= _collectionRect.Top && pt.Y < _collectionRect.Bottom;
        }

        private void DrawVisuals()
        {
            if(_mainVisual != null)
            {
                _children.Remove(_mainVisual);
            }
            _mainVisual = new DrawingVisual();
            _mainVisual.CacheMode = new BitmapCache();

            var renderRect = GetChartRect();
            var whileRect = new Rect(0, 0, Math.Round(ActualWidth), Math.Round(ActualHeight));

            _mainVisual.Clip = new RectangleGeometry(whileRect);

            var dc = _mainVisual.RenderOpen();

            CreateBackgroundVisual(dc, whileRect);
            if (_mainCollection != null)
            {
                var collRect = new Rect(renderRect.Left + 1, renderRect.Top + 1, renderRect.Width - 2, renderRect.Height - 2);
                _mainCollection.CalculatePosition(collRect);

                CreateYScaleVisual(dc, renderRect, renderRect.Right, _mainCollection);
                CreateXScaleVisual(dc, renderRect, renderRect.Bottom, _mainCollection);
                _mainCollection.Draw(dc);
            }

            CreateBorderVisual(dc);
            foreach (var assistColl in _assistCollections)
            {
                CreateCollectionVisual(dc, renderRect, assistColl);
            }
            dc.Close();
            _children.Add(_mainVisual);
        }

        private void CreateBackgroundVisual(DrawingContext drawingContext, Rect drawRect)
        {
            Background.Freeze();

            drawingContext.DrawRectangle(Background, null, drawRect);
        }

        private void CreateBorderVisual(DrawingContext drawingContext)
        {
            Pen pen = new Pen(scaleColor, scaleThickness);
            pen.Freeze();

            var chartRect = GetChartRect();

            drawingContext.DrawRectangle(null, pen, new Rect(AdjustPoint(chartRect.TopLeft), AdjustPoint(chartRect.BottomRight)));
        }

        private void CreateCollectionVisual(DrawingContext drawingContext, Rect drawRect, ChartItemCollection collection)
        {
            // Create a rectangle and draw it in the DrawingContext.
            collection.CalculatePosition(drawRect);
            collection.Draw(drawingContext);
        }

        private void CreateYScaleVisual(DrawingContext drawingContext, Rect drawRect, double x, ChartItemCollection collection)
        {
            if (YScaleDock == YScaleDock.None)
                return;

            Pen scalePen = new Pen(scaleColor, scaleThickness);
            var face = new Typeface(scaleFontFamily);
            var scales = collection.GetYAxisScales();
            for (int i = 1; i < scales.Count - 1; i++)
            {
                Point pt1 = AdjustPoint(new Point(drawRect.Left, scales[i].Pos));
                Point pt2 = AdjustPoint(new Point(drawRect.Right, scales[i].Pos));
                drawingContext.DrawLine(scalePen, pt1, pt2);

                FormattedText format = new FormattedText(scales[i].Value.ToString(scaleNumberFormat), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, scaleFontSize, scaleColor);
                Point ptext = pt2;

                ptext.Y -= format.Height / 2;
                drawingContext.DrawText(format, ptext);
            }

            
            //Point pt11 = AdjustPoint(new Point(drawRect.Left, scales[scales.Count - 1].Pos));
            //Point pt21 = AdjustPoint(new Point(drawRect.Right, scales[scales.Count - 1].Pos));
            //drawingContext.DrawLine(scalePen, pt11, pt21);
            FormattedText formatTop = new FormattedText(scales[0].Value.ToString(scaleNumberFormat), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, scaleFontSize, scaleColor);
            Point ptextTop = new Point(x, drawRect.Top);
            drawingContext.DrawText(formatTop, ptextTop);

            FormattedText formatBottom = new FormattedText(scales[scales.Count - 1].Value.ToString(scaleNumberFormat), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, scaleFontSize, scaleColor);
            Point ptextBottom = new Point(x, drawRect.Bottom - scaleFontSize);
            drawingContext.DrawText(formatBottom, ptextBottom);
        }

        private void CreateXScaleVisual(DrawingContext drawingContext, Rect drawRect, double y, ChartItemCollection collection)
        {
            if (xScaleDock == XScaleDock.None)
                return;

            var face = new Typeface(scaleFontFamily);
            var scales = collection.GetXAxisScales();

            KeyValuePair<FormattedText, Point>[] dates = new KeyValuePair<FormattedText, Point>[scales.Count];

            for (int i = 0; i < scales.Count; i++)
            {
                FormattedText format = new FormattedText(scales[i].Value.ToString(timeScaleFormat), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, face, scaleFontSize, scaleColor);
                Point ptext = new Point(scales[i].Pos, y);

                if (scales[i].Pos + format.Width > drawRect.Right)
                {
                    scales[i].Pos = scales[i].Pos - (drawRect.Right - (scales[i].Pos + format.Width));
                }
                dates[i] = new KeyValuePair<FormattedText, Point>(format, ptext);
            }

            bool isOverlapped = dates.Length > 1 && (dates[0].Value.X + dates[0].Key.Width * 3 >= dates[1].Value.X);

            if (isOverlapped)
            {
                dates = new KeyValuePair<FormattedText, Point>[] { dates[0], dates[dates.Length - 1] };
            }
 
            foreach (var date in dates)
            {
                drawingContext.DrawText(date.Key, date.Value);
            }
        }

        private void CreateCursorVisual(double x, double y, double value, bool isShowhorizontalLine)
        {
            if (_cursorVisual != null)
            {
                _children.Remove(_cursorVisual);
            }

            var chartRect = _collectionRect;
            var whileRect = new Rect(0, 0, Math.Round(ActualWidth), Math.Round(ActualHeight));

            _cursorVisual = new DrawingVisual();
            _cursorVisual.CacheMode = new BitmapCache();
            //_cursorVisual.Clip = new RectangleGeometry(whileRect);
            var drawingContext = _cursorVisual.RenderOpen();

            Pen pen = new Pen(CursorColor, CursorThickness);

            FormattedText textFormat = null;
            Rect textRect = new Rect();
            
            if(isShowhorizontalLine)
            {
                drawingContext.DrawLine(pen, new Point(chartRect.Left, y), new Point(chartRect.Right, y));
                Typeface typeface = new Typeface(cursorFontFamily);
                textFormat = new FormattedText(value.ToString("F2"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, cursorFontSize, cursorTextColor);
                var ptText = AdjustPoint(new Point(chartRect.Right, y - textFormat.Height / 2));
                var szText = new Size(textFormat.Width, textFormat.Height);
                if (ptText.Y < chartRect.Top)
                    ptText.Y = chartRect.Top;
                else if (ptText.Y + szText.Height > chartRect.Bottom)
                {
                    ptText.Y = chartRect.Bottom - szText.Height;
                }
                
                textRect = new Rect(ptText, szText);
                drawingContext.DrawRectangle(CursorColor, null, textRect);
            }

            drawingContext.DrawLine(pen, new Point(x, chartRect.Top), new Point(x, chartRect.Bottom));
            if (textFormat != null)
            {
                drawingContext.DrawText(textFormat, textRect.TopLeft);
            }
            drawingContext.Close();

            _children.Add(_cursorVisual);
        }

        private void CreateSelectionRectVisual(Point ptStart, Point ptEnd)
        {
            if (_cursorVisual != null)
            {
                _children.Remove(_cursorVisual);
            }

            _cursorVisual = new DrawingVisual();
            var drawingContext = _cursorVisual.RenderOpen();
            
            Pen mouseSelectionPen = new Pen(mouseSelectRectColor, mouseSelectRectThickness);

            drawingContext.DrawRectangle(null, mouseSelectionPen, new Rect(ptStart, ptEnd));
            drawingContext.Close();

            _children.Add(_cursorVisual);
        }

        private void CreateRulerVisual(Point point1, Point point2, double change)
        {
            if (_cursorVisual != null)
            {
                _children.Remove(_cursorVisual);
            }

            _cursorVisual = new DrawingVisual();
            var drawingContext = _cursorVisual.RenderOpen();

            Pen mouseSelectionPen = new Pen(mouseRulerColor, mouseRulerThickness);

            GeometryGroup group = new GeometryGroup();

            drawingContext.DrawLine(mouseSelectionPen, point1, point2);
            FormattedText format = new FormattedText(change.ToString("P2"), CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface(rulerFontFamily), rulerFontSize, mouseRulerColor);
            Point ptext = point2;
            ptext.Y -= rulerFontSize;
            drawingContext.DrawText(format, ptext);
            drawingContext.Close();

            _children.Add(_cursorVisual);
        }

        private void MoveCursor(Point position, bool isActive)
        {
            if (CursorColor == null)
                return;

            List<CurrentChartItem> currentItems = new List<CurrentChartItem>();

            if (_mainCollection != null)
            {
                var itemWp = _mainCollection.LocateCurrentChartItem(position);
                if (itemWp != null)
                {
                    var x = ChartItemCollection.Adjust(position.X);
                    var y = ChartItemCollection.Adjust(position.Y);
                    var v = _mainCollection.GetValueFromPosition(position.Y);
                    CreateCursorVisual(x, y, v, isActive);

                    currentItems.Add(new CurrentChartItem(_mainCollection.Id, itemWp.CharItem));

                }
            }

            foreach (var assitCollection in _assistCollections)
            {
                var itemWp = assitCollection.LocateCurrentChartItem(position);
                if (itemWp != null)
                {
                    currentItems.Add(new CurrentChartItem(assitCollection.Id, itemWp.CharItem));
                }
            }

            if (currentItems.Any())
                RaiseCursorMovedEvent(currentItems);
            
        }

        private void MoveCursor(int steps, bool isActive)
        {
            if (CursorColor == null)
                return;

            List<CurrentChartItem> currentItems = new List<CurrentChartItem>();

            if (steps != 0)
            {
                if (_mainCollection != null)
                {
                    var itemWp = _mainCollection.LocateNearItem(steps);

                    if (itemWp != null)
                    {
                        CreateCursorVisual(itemWp.Point.X, itemWp.Point.Y, itemWp.CharItem.Value, isActive);

                        currentItems.Add(new CurrentChartItem(_mainCollection.Id, itemWp.CharItem));
                        
                    }
                }

                foreach (var assitCollection in _assistCollections)
                {
                    var itemWp = assitCollection.LocateNearItem(steps);
                    if (itemWp != null)
                    {
                        currentItems.Add(new CurrentChartItem(assitCollection.Id, itemWp.CharItem));
                    }
                }

                if(currentItems.Any())
                    RaiseCursorMovedEvent(currentItems);
            }
        }

        private void MoveChart(int steps)
        {
            bool isNeedRedraw = false;
            if (steps != 0)
            {
                if (_mainCollection != null)
                {
                    if (_mainCollection.Move(steps))
                    {
                        isNeedRedraw = true;
                    }
                }

                for (int i = 0; i < _assistCollections.Count; i ++)
                {
                    _assistCollections[i].Move(steps);
                }
            }

            if (isNeedRedraw)
                DrawVisuals();
        }

        private void ZoomChart(bool isZoomIn)
        {
            if (_mainCollection != null)
            {
                if (isZoomIn)
                {
                    _mainCollection.ZoomIn();
                    for (int i = 0; i < _assistCollections.Count; i++)
                    {
                        _assistCollections[i].ZoomIn();
                    }
                }
                else
                {
                    _mainCollection.ZoomOut();
                    for (int i = 0; i < _assistCollections.Count; i++)
                    {
                        _assistCollections[i].ZoomOut();
                    }
                }

                DrawVisuals();

            }
            
        }

        private void ShowRegion(Point start, Point end)
        {
            bool isNeedRedraw = false;

            if (_mainCollection != null)
            {
                if (_mainCollection.ShowRegion(start, end))
                {
                    isNeedRedraw = true;
                }
            }

            if (isNeedRedraw)
            {
                for (int i = 0; i < _assistCollections.Count; i++)
                {
                    _assistCollections[i].ShowRegion(start, end);
                }
            }
            
            if(isNeedRedraw)
                DrawVisuals();
        }

        private void MoveCursor4ConnectionList(MouseEventArgs e)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart != null)
                    {
                        var pos = e.GetPosition(chart);
                        chart.MoveCursor(pos, false);
                    }
                }
            }
        }

        private void MoveCursor4ConnectionList(int steps)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart != null)
                    {
                        chart.MoveCursor(steps, false);
                    }
                }
            }
        }

        private void MoveChart4ConnectionList(int steps)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart != null)
                    {
                        chart.MoveChart(steps);
                    }
                }
            }
        }

        private void ZoomChart4ConnectionList(bool isZoomIn)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart != null)
                    {
                        chart.ZoomChart(isZoomIn);
                    }
                }
            }
        }

        private void ShowRegion4ConnectionList(Point start, Point end)
        {
            foreach (var wChar in connectionList)
            {
                ChartHost chart;
                if (wChar.TryGetTarget(out chart))
                {
                    if (chart != null)
                    {
                        chart.ShowRegion(start, end);
                    }
                }
            }
        }

        private Point AdjustPoint(Point pt)
        {
            pt.X = ChartItemCollection.Adjust(pt.X);
            pt.Y = ChartItemCollection.Adjust(pt.Y);

            return pt;
        }
        #endregion

        #region Visual Tree override
        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount
        {
            get { return _children.Count; }
        }

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
            {
                throw new ArgumentOutOfRangeException();
            }

            return _children[index];
        }
        #endregion

    }

    public class CurrentChartItem
    {
        public CurrentChartItem(int id, ChartItem item)
        {
            Id = id;
            Item = item;
        }

        public int Id
        {
            get;
            private set;
        }

        public ChartItem Item
        {
            get;
            private set;
        }
    }

    public class CursorMovedRoutedEventArgs : RoutedEventArgs
    {
        public CursorMovedRoutedEventArgs(IEnumerable<CurrentChartItem> currentItems)
        {
            CurrentItems = currentItems;
        }

        public IEnumerable<CurrentChartItem> CurrentItems
        {
            get;
            set;
        }
    }
}
