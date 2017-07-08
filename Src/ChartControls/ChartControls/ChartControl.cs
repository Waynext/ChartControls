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
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ChartControls.Drawing;
#if USINGCANVAS
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
#endif

namespace ChartControls
{
    /// <summary>
    /// 股票图表控件。
    /// 显示股票K线图，分时图，成交量图及技术指标图。
    /// </summary>
    /// <remarks>
    /// 
    /// </remarks>
    /// <example>
    /// 1.创建K线图。
    /// Xaml
    /// &lt;local:ChartControl x:Name="price"/&gt;
    /// <code>
    /// Stock stock = ...;
    /// var closeList = new List&lt;ChartItem&gt;();
    /// CandleData cdPre = stock.Items.First();
    /// foreach (var cd in stock.Items)
    /// {
    ///     closeList.Add(new StockItem()
    ///     {
    ///         Close = cd.close,
    ///         Date = cd.DateTime,
    ///         High = cd.high,
    ///         Low = cd.low,
    ///         Open = cd.open,
    ///         CloseChange = (cd.close - cdPre.close) / cdPre.close
    ///     });
    ///     cdPre = cd;
    /// }
    ///
    /// CollectionId id = new CollectionId("000001", "SH");
    /// StockItemCollection coll = new StockItemCollection(id, closeList, raisePen, fallPen, null, false);
    /// price.SetMainCollection(coll);
    /// price.ForceDraw();
    /// </code>
    /// 
    /// 2.创建K线图及成交量图。
    /// Xaml
    /// &lt;local:ChartControl x:Name="price"/&gt;
    /// &lt;local:ChartControl x:Name="volumn" /&gt;
    /// <code>
    /// Stock stock = ...;
    ///
    ///var closeList = new List&lt;ChartItem&gt;();
    ///var volList = new List&lt;ChartItem&gt;();
    ///CandleData cdPre = stock.Items.First();
    ///foreach (var cd in stock.Items)
    ///{
    ///    closeList.Add(new StockItem()
    ///    {
    ///        Close = cd.close,
    ///        Date = cd.DateTime,
    ///        High = cd.high,
    ///        Low = cd.low,
    ///        Open = cd.open,
    ///        CloseChange = (cd.close - cdPre.close) / cdPre.close
    ///    });
    ///    cdPre = cd;
    ///
    ///    volList.Add(new VolumnItem()
    ///    {
    ///        Date = cd.DateTime,
    ///        IsRaise = cd.open &lt;= cd.close,
    ///        Volumn = cd.amount,
    ///        Turnover = cd.money
    ///    });
    ///}
    ///
    ///string stockId = "000001";
    ///string marketId = "SH";
    ///CollectionId id = new CollectionId(stockId, marketId);
    ///StockItemCollection coll = new StockItemCollection(id, closeList, raisePen, fallPen, null, false);
    ///price.SetMainCollection(coll);
    ///
    ///id = new CollectionId(stockId, marketId);
    ///VolumnItemCollection volColl = new VolumnItemCollection(id, volList, raisePen, fallPen, false);
    ///volumn.SetMainCollection(volColl);
    ///price.AddConnection(volumn);
    ///price.ForceDraw();
    /// </code>
    /// </example>
    /// 
#if USINGCANVAS
    public partial class ChartControl : Canvas
#else
    public partial class ChartControl : FrameworkElement
#endif
    {
        #region Private members
        private ChartItemCollection _mainCollection;

        private List<ChartItemCollection> _assistCollections = new List<ChartItemCollection>();

        private List<IExtraDataGraphics> _extraDataGraphicsList = new List<IExtraDataGraphics>();

        private List<ICustomGraphics> _customGraphicsList = new List<ICustomGraphics>();

        private List<IInteractive> _interactiveList = new List<IInteractive>();

        private Rect _collectionRect;
        
        private PointerAction pointerAction = PointerAction.None;

        private Thickness chartMargin;
        private string scalePercentageFormat = "P2";

        private ICustomGraphics _drawingCustomGraphics;
        private bool _isCustomGraphicsDrawingStarted;
        private bool _isDeletingCustomGraphics;

        private BitmapCache globalBitmapCache;

        private double _cursorStart;
        private double _xCursorStart;
        private Size? _dynamicScaleSize;
        private double _xDynamicScaleStart;
        private double _mainMaxValue;

        private int moveChartSteps = 1;
        private const double zoomInTimes = 1.2;
        private const double zoomOutTimes = 0.8;

        private bool isInteractive = true;
        #endregion

        #region Public constructors and methods
        /// <summary>
        /// 构造函数，初始化控件。
        /// </summary>
        public ChartControl()
        {
            InitControl();

            this.SizeChanged += ChartControl_SizeChanged;

            RecalcuteChartMargin();
            ResizeLayout();
        }

        /// <summary>
        /// 设置主要数据集合，比如k线图数据集合。
        /// </summary>
        /// <param name="collection">图表数据集合。</param>
        /// <param name="needDraw">设置主数据后是否绘制。缺省值是false，表示不绘制，需要自行调用ForceDraw。</param>
        public void SetMainCollection(ChartItemCollection collection, bool needDraw = false)
        {
            ResetChartControl();

            _mainCollection = collection;
            SetIsScalesOptimized(IsScalesOptimized);
            SetYColumnCount(YColumnCount);
            SetXColumnCount(XColumnCount);

            if(needDraw)
                DrawVisuals();
        }

        /// <summary>
        /// 设置辅助数据集合，比如K线图的均线数据集合，或者其他指标数据集合。
        /// </summary>
        /// <param name="collection">图表数据集合。</param>
        /// <param name="isIndependent">辅助数据集合显示位置是否是独立计算的。缺省值是false，表示辅助数据集合显示位置不是独立计算，而是通过根据主要数据集合计算。</param>
        /// <param name="needDraw"></param>
        public void AddAssistCollection(ChartItemCollection collection, bool isIndependent = false, bool needDraw = false)
        {
            if (_mainCollection == null)
            {
                return;
            }

            collection.AssistTo(_mainCollection, isIndependent);
            _assistCollections.Add(collection);

            if (needDraw)
                DrawVisuals();
        }

        /// <summary>
        /// 添加额外数据绘制接口， 比如除权数据、分红数据、消息。
        /// </summary>
        /// <param name="graphic"><see cref="IExtraDataGraphics"/></param>
        public void AddExtraDataGraphic(IExtraDataGraphics graphic)
        {
            _extraDataGraphicsList.Add(graphic);
            var ia = graphic as IInteractive;
            if (ia != null)
            {
                _interactiveList.Add(ia);
            }
        }

        /// <summary>
        /// 移除额外数据绘制接口。
        /// </summary>
        /// <param name="graphic"><see cref="IExtraDataGraphics"/></param>
        public void RemoveExtraDataGraphic(IExtraDataGraphics graphic)
        {
            _extraDataGraphicsList.Remove(graphic);

            var ia = graphic as IInteractive;
            if (ia != null)
            {
                _interactiveList.Remove(ia);
            }
        }

        /// <summary>
        /// 强制重画控件。
        /// </summary>
        /// <param name="needResizeLayout">是否需要重新计算大小，缺省值是false，不需要重新计算。</param>
        /// <param name="needRemoveCursor">是否需要删除光标线。缺省值是true， 表示需要删除光标线。</param>
        public void ForceDraw(bool needResizeLayout = false, bool needRemoveCursor = true)
        {
            if (needResizeLayout)
            {
                RecalcuteChartMargin();
                ResizeLayout();
            }

            DrawVisuals(needRemoveCursor);

            foreach (var wChar in _connectionList)
            {
                if (wChar.IsAlive)
                {
                    ChartControl chart = wChar.Target as ChartControl;

                    if (needResizeLayout)
                    {
                        chart.RecalcuteChartMargin();
                        chart.ResizeLayout();
                    }

                    chart.DrawVisuals(needRemoveCursor);
                }
                
            }
        }

        /// <summary>
        /// 开始绘制自定义图形，比如直线，平行线等等。用户可以通过鼠标选择图形位置。
        /// </summary>
        /// <param name="g"><see cref="ICustomGraphics"/></param>
        public void StartDrawCustomGraphics(ICustomGraphics g)
        {
            _drawingCustomGraphics = g;
            _isCustomGraphicsDrawingStarted = false;
        }

        /// <summary>
        /// 结束绘制自定义图形。
        /// </summary>
        public void StopDrawCustomGraphics()
        {
            StartDrawCustomGraphics(null);
        }

        /// <summary>
        /// 开始删除自定义图形。用户可以通过鼠标选择删除图形。
        /// </summary>
        public void StartRemoveCustomGraphics()
        {
            _isDeletingCustomGraphics = true;
        }

        /// <summary>
        /// 结束删除自定义图形。
        /// </summary>
        public void StopRemoveCustomGraphics()
        {
            _isDeletingCustomGraphics = false;
        }

        private List<WeakReference> _connectionList = new List<WeakReference>();

        /// <summary>
        /// 关联另一个图表控件。关联后的几个控件鼠标移动，放大缩小会联动起来。
        /// </summary>
        /// <param name="otherChart"></param>
        public void AddConnection(ChartControl otherChart)
        {
            foreach (var wChar in _connectionList)
            {
                if (wChar.IsAlive)
                {
                    ChartControl chart = wChar.Target as ChartControl;

                    if (chart == otherChart)
                        return;
                }
                
            }

            _connectionList.Add(new WeakReference(otherChart));

            otherChart.AddConnection(this);
        }

        /// <summary>
        /// 主要数据集合
        /// </summary>
        public ChartItemCollection MainCollection
        {
            get
            {
                return _mainCollection;
            }
        }

        /// <summary>
        /// 辅助数据集合列表
        /// </summary>
        public IEnumerable<ChartItemCollection> AssistCollections
        {
            get
            {
                return _assistCollections;
            }
        }

        /// <summary>
        /// 设置光标位置
        /// </summary>
        /// <param name="position"></param>
        public void SetCursorPosition(Point position)
        {
            MoveCursor(position, true);
            MoveCursor4ConnectionList(position);
        }

        /// <summary>
        /// 移动光标
        /// </summary>
        /// <param name="steps">正值表示向右移动的单位，负值向左移动的单位。</param>
        public void MoveCursorPosition(int steps)
        {
            MoveCursor(steps, true);
            MoveCursor4ConnectionList(steps);
        }

        /// <summary>
        /// 移动图表
        /// </summary>
        /// <param name="steps">正值表示向右移动的单位，负值向左移动的单位。</param>
        public void MoveChartPosition(int steps)
        {
            var hasMoved = MoveChart(steps);
            if (hasMoved)
                MoveChart4ConnectionList(steps);

        }

        /// <summary>
        /// 放大或者缩小图表。
        /// </summary>
        /// <param name="times">当前图表的倍数，大于1表示放大。大于0小于1表示缩小。</param>
        /// <param name="autoAdjust">如果倍数过小，放大缩小失败，是否自动调整。缺省值是True，自动调整倍数。</param>
        /// <returns>是否成功</returns>
        public bool Zoom(double times, bool autoAdjust = true)
        {
            if(ZoomChart(times, autoAdjust))
            {
                ZoomChart4ConnectionList(times, autoAdjust);
                return true;
            }
            else
            {
                return false;
            }
            
        }

        /// <summary>
        /// 显示两个点之间的数据集合，等于放大显示。
        /// </summary>
        /// <param name="start">起始点</param>
        /// <param name="end">结束点</param>
        public void DisplayRegion(Point start, Point end)
        {
            ShowRegion(start, end);
            ShowRegion4ConnectionList(start, end);
        }

        /// <summary>
        /// 查找数据集合。
        /// </summary>
        public ChartItemCollection FindCollection(CollectionId id)
        {
            if(MainCollection != null && MainCollection.Id.Equals(id))
            {
                return MainCollection;
            }
            else if(_assistCollections != null)
            {
                return _assistCollections.FirstOrDefault(c => c.Id.Equals(id));
            }
            else
            {
                return null;
            }
        }
        
        /// <summary>
        /// 是否相应交互事件，比如鼠标，触摸及键盘输入。
        /// </summary>
        public bool IsIteractive
        {
            get
            {
                return isInteractive;
            }
            set
            {
                if(isInteractive != value)
                {
                    isInteractive = value;
                    SwitchInteractiveState(isInteractive);
                }
                
            }
        }
        #endregion

        #region Event handlers
        void ChartControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeLayout();
            DrawVisuals();
        }

        #endregion

        #region Private methods

        private void ResizeLayout()
        {
            _collectionRect = GetChartRect();
            Debug.WriteLine(string.Format("({0},{1})|({2},{3})", _collectionRect.Left, _collectionRect.Top, _collectionRect.Right, _collectionRect.Bottom));

            Size newSize = new Size(_collectionRect.Width, _collectionRect.Height);

            if (newSize.Height == 0 || newSize.Width == 0)
                return;

            if (_mainCollection != null)
            {
                _mainCollection.Resize(newSize);
            }

            foreach (var assistColl in _assistCollections)
            {
                assistColl.Resize(newSize);
            }
        }

        private Rect GetChartRect()
        {
            int width = (int)(ActualWidth - chartMargin.Left - chartMargin.Right - BorderThickness * 2);
            int height = (int)(ActualHeight - chartMargin.Top - chartMargin.Bottom - BorderThickness * 2);
            if (width <= 0 || height <= 0)
            {
                return new Rect(0, 0, 0, 0);
            }

            return new Rect(chartMargin.Left + BorderThickness, chartMargin.Top + BorderThickness, width, height);
        }

        private bool IsPointInChart(Point pt)
        {
            return pt.X >= _collectionRect.Left && pt.X < _collectionRect.Right &&
                pt.Y >= _collectionRect.Top && pt.Y < _collectionRect.Bottom;
        }

        private bool ReserveCursorLayer(bool needRemoveCursor)
        {
            if (needRemoveCursor)
                RemoveCursorLayer();
            else
            {
                var max = _mainCollection.GetMaxValue();
                var diff = max - _mainMaxValue;
                diff = Math.Pow(diff, 10);
                if (diff >= 1)
                {
                    _mainMaxValue = max;
                    needRemoveCursor = true;
                    RemoveCursorLayer();
                }
                else
                {
                    PopCursorLayer();
                }
            }

            return needRemoveCursor;
        }

        private void RestoreCursorLayer(bool needRemoveCursor)
        {
            if (needRemoveCursor)
                _dynamicScaleSize = null;
            else
                PushCursorLayer();
        }

        private void DrawVisuals(bool needRemoveCursor = true)
        {
            if (Visibility != Visibility.Visible)
                return;

            var renderRect = _collectionRect;
            if (renderRect.Width == 0 || renderRect.Height == 0)
            {
                return;
            }

            CreateLayers();

            var borderRect = GetChartBorderRect();
            var whileRect = new Rect(0, 0, ActualWidth, ActualHeight);

            var dcBg = CreateBackgroundDC();

            FillBackgroundVisual(dcBg, whileRect);
            CreateBorderVisual(dcBg, borderRect);

            if (_mainCollection != null)
            {
                _mainCollection.CalculatePosition(renderRect);

                CreateYScaleVisual(dcBg, borderRect, _mainCollection);
                CreateXScaleVisual(dcBg, borderRect, _mainCollection);

                var dcMain = CreateMainDC(dcBg);

                foreach (var extratGraphic in _extraDataGraphicsList)
                {
                    extratGraphic.DrawExtraData(this, dcBg);
                }

                dcMain.PushClip(new RectangleGeometry() { Rect = renderRect });
                _mainCollection.Draw(dcMain);

                foreach (var assistColl in _assistCollections)
                {
                    CreateCollectionVisual(dcMain, renderRect, assistColl);
                }

                dcMain.Pop();
                dcMain.Dispose();

                needRemoveCursor = ReserveCursorLayer(needRemoveCursor);
            }
            else
            {
                needRemoveCursor = ReserveCursorLayer(needRemoveCursor);
            }

            dcBg.Dispose();
            
            DrawCustomGraphicList(true);

            RestoreCursorLayer(needRemoveCursor);
        }

        private void FillBackgroundVisual(IDrawingContext drawingContext, Rect drawRect)
        {
#if USINGCANVAS
#else
            if(!Background.IsFrozen)
                Background.Freeze();

            Rect rect = new Rect(drawRect.Left + 0.5, drawRect.Top + 0.5, drawRect.Width, drawRect.Height);
            drawingContext.DrawRectangle(Background, null, drawRect);

#endif
        }

        private void CreateBorderVisual(IDrawingContext drawingContext, Rect borderRect)
        {
            IPen pen = DrawingObjectFactory.CreatePen(Border, BorderThickness);
#if USINGCANVAS
#else
            var dPen = pen as DrawingPen;
            dPen.Freeze();
#endif

            drawingContext.DrawRectangle(null, pen, borderRect);
        }

        private void CreateCollectionVisual(IDrawingContext drawingContext, Rect drawRect, ChartItemCollection collection)
        {
            // Create a rectangle and draw it in the DrawingContext.
            collection.CalculatePosition(drawRect);
            collection.Draw(drawingContext);
        }

        private string GetYScaleNumberFormat()
        {
            return CoordinateType != CoordinateType.Percentage ? YScaleFormat : scalePercentageFormat;
        }

        private void CreateYScaleVisual(IDrawingContext drawingContext, Rect borderRect, ChartItemCollection collection)
        {
            if (YScaleDock == YScaleDock.None)
                return;

            Rect drawRect = (YScaleDock == YScaleDock.InnerLeft || YScaleDock == YScaleDock.InnerRight) ? _collectionRect : GetOutsideChartBorderRect(borderRect);
            IPen scalePen = null;
            if (YScaleLineThickness != 0)
            { 
                scalePen = DrawingObjectFactory.CreatePen(YScaleLineColor, YScaleLineThickness);
                if (YScaleLineDashes != null)
                {
                    scalePen.Dashes = YScaleLineDashes;
                }
                scalePen.StartLineCap = scalePen.EndLineCap = PenLineCap.Square;

#if USINGCANVAS
#else
                var dPen = scalePen as DrawingPen;
                dPen.Freeze();
#endif
            }
            var scales = collection.GetYAxisScales(CoordinateType);

            if (scales == null || !scales.Any())
                return;

            double x = 0f;

            ITextFormat formatTop = DrawingObjectFactory.CreateTextFormat(scales[0].Value.ToString(GetYScaleNumberFormat()), 
                FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
            x = GetTextStartPoint(drawRect, formatTop);
            Point ptextTop = new Point(x, drawRect.Top);
            double heightTextTop = formatTop.Height + ptextTop.Y;

            drawingContext.DrawText(formatTop, ptextTop);

            if (scales.Count >= 2)
            {
                ITextFormat formatBottom = DrawingObjectFactory.CreateTextFormat(scales[scales.Count - 1].Value.ToString(GetYScaleNumberFormat()),
                    FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
                x = GetTextStartPoint(drawRect, formatBottom);
                Point ptextBottom = new Point(x, drawRect.Bottom - formatBottom.Height);
                double heightTextBottom = formatBottom.Height + ptextBottom.Y;
                drawingContext.DrawText(formatBottom, ptextBottom);

                for (int i = 1; i < scales.Count - 1; i++)
                {
                    ITextFormat format = DrawingObjectFactory.CreateTextFormat(scales[i].Value.ToString(GetYScaleNumberFormat()),
                        FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
                    Point ptext = new Point(x, scales[i].Pos);
                    ptext.X = PointSnapper.SnapValue(GetTextStartPoint(drawRect, format));

                    ptext.Y -= format.Height / 2;

                    if (ptext.Y > ptextTop.Y - format.Height && ptext.Y < heightTextTop)
                    {
                        continue;
                    }
                    else if (ptext.Y >= ptextBottom.Y - format.Height && ptext.Y < heightTextBottom)
                    {
                        continue;
                    }

                    if (scalePen != null)
                    {
                        Point pt1, pt2;
                        if (YScaleDock == YScaleDock.InnerLeft)
                        {
                            pt1 = PointSnapper.SnapPoint(new Point(_collectionRect.Left + format.Width, scales[i].Pos));
                            pt2 = PointSnapper.SnapPoint(new Point(_collectionRect.Right - 1, scales[i].Pos));
                        }
                        else if(YScaleDock == YScaleDock.InnerRight)
                        {
                            pt1 = PointSnapper.SnapPoint(new Point(_collectionRect.Left, scales[i].Pos));
                            pt2 = PointSnapper.SnapPoint(new Point(_collectionRect.Right - format.Width - 1, scales[i].Pos));
                        }
                        else
                        {
                            pt1 = PointSnapper.SnapPoint(new Point(_collectionRect.Left, scales[i].Pos));
                            pt2 = PointSnapper.SnapPoint(new Point(_collectionRect.Right - 1, scales[i].Pos));
                        }
                        drawingContext.DrawLine(scalePen, pt1, pt2);
                    }

                    drawingContext.DrawText(format, ptext);
                }
            }
        }

        private double GetTextStartPoint(Rect drawRect, ITextFormat format)
        {
            double x = 0f;
            if (YScaleDock == YScaleDock.Right)
            {
                x = drawRect.Right + 1;
            }
            else if (YScaleDock == YScaleDock.InnerRight)
            {
                x = drawRect.Right - format.Width;
            }
            else if (YScaleDock == YScaleDock.Left)
            {
                x = drawRect.Left - format.Width - 1;

            }
            else if (YScaleDock == YScaleDock.InnerLeft)
            {
                x = drawRect.Left + 1;
            }

            return x;
        }
        private void CreateXScaleVisual(IDrawingContext drawingContext, Rect drawRect, ChartItemCollection collection)
        {
            if (XScaleDock == XScaleDock.None)
                return;

            IPen scalePen = null;
            if (XScaleLineThickness != 0)
            {
                scalePen = DrawingObjectFactory.CreatePen(XScaleLineColor, XScaleLineThickness);
                if (XScaleLineDashes != null)
                {
                    scalePen.Dashes = XScaleLineDashes;
                }
                scalePen.StartLineCap = scalePen.EndLineCap = PenLineCap.Square;

#if USINGCANVAS
#else
                var dPen = scalePen as DrawingPen;
                dPen.Freeze();
#endif
            }

            var scales = collection.GetXAxisScales();

            if (scales == null || !scales.Any())
                return;

            double minDis = GetScalesMinSpan<DateTime>(scales);

            KeyValuePair<ITextFormat, Point>[] dates = new KeyValuePair<ITextFormat, Point>[scales.Count];

            for (int i = 0; i < scales.Count; i++)
            {
                if (scales[i].Value != DateTime.MinValue)
                {
                    ITextFormat format = DrawingObjectFactory.CreateTextFormat(scales[i].Value.ToString(XScaleFormat),
                        FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);

                    var xPos = scales[i].Pos;
                    var dd = drawRect.Right - (xPos + format.Width);
                    if (dd < 0)
                    {
                        xPos = xPos + dd;
                    }
                    Point ptext = new Point(xPos, drawRect.Bottom);

                    dates[i] = new KeyValuePair<ITextFormat, Point>(format, ptext);
                }
                if(scalePen != null && scales[i].Pos >= drawRect.Left && scales[i].Pos < drawRect.Right)
                {
                    if (( i != 0 && i != scales.Count - 1 ) ||
                        (i == 0 && scales[i].Pos - drawRect.Left >= minDis) || 
                        ( i == scales.Count - 1 && drawRect.Right - scales[i].Pos >= minDis))
                    {
                        Point pt1 = PointSnapper.SnapPoint(new Point(scales[i].Pos, _collectionRect.Top));
                        Point pt2 = PointSnapper.SnapPoint(new Point(scales[i].Pos, _collectionRect.Bottom - 1));
                        drawingContext.DrawLine(scalePen, pt1, pt2);
                    }
                }
            }
 
            foreach (var date in dates)
            {
                if (date.Key != null)
                {
                    drawingContext.DrawText(date.Key, date.Value);
                }
            }
        }

        private double GetScalesMinSpan<T>(IList<Scale<T>> scales)
        {
            double minSpan = 0;
            if(scales.Count > 1)
            {
                minSpan = (scales[1].Pos - scales[0].Pos) / 10;
            }

            return minSpan;
        }

        private void CreateSelectionRectVisual(Point ptStart, Point ptEnd)
        {
            RemoveCursorLayer();
            CreateCursorLayer();

            using (var dc = CreateCursorDC())
            {
                IPen mouseSelectionPen = DrawingObjectFactory.CreatePen(SelectionBorderColor, SelectionBorderThickness);
                if (SelectionBorderDashes != null)
                {
                    mouseSelectionPen.Dashes = SelectionBorderDashes;
                }
                mouseSelectionPen.StartLineCap = mouseSelectionPen.EndLineCap = PenLineCap.Square;

                dc.DrawRectangle(null, mouseSelectionPen, new Rect(ptStart, ptEnd));
            }
        }

        private void CreateRulerVisual(Point point1, Point point2, double change)
        {
            RemoveCursorLayer();
            CreateCursorLayer();

            using (var dc = CreateCursorDC())
            {
                if (MeasureGraphics == null)
                {
                    IPen mouseRulerPen = DrawingObjectFactory.CreatePen(SelectionBorderColor, SelectionBorderThickness);
                    mouseRulerPen.StartLineCap = mouseRulerPen.EndLineCap = PenLineCap.Square;

                    GeometryGroup group = new GeometryGroup();

                    dc.DrawLine(mouseRulerPen, point1, point2);

                    ITextFormat format = DrawingObjectFactory.CreateTextFormat(change.ToString("P2"),
                        FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
                    Point ptext = point2;
                    ptext.Y -= format.Height;
                    dc.DrawText(format, ptext);

                }
                else
                {
                    MeasureGraphics.UpdateValue(change);
                    MeasureGraphics.NextDraw(point2, dc, false);
                }
            }
        }

        private bool CreateCurrentCustomGraphicVisual(Point pt, bool isFinal)
        {
            RemoveCursorLayer();
            CreateCursorLayer();
            using (var dc = CreateCursorDC())
            {
                bool isFinished = _drawingCustomGraphics.NextDraw(pt, dc, isFinal);
                return isFinished;
            }
        }

        private CurrentChartItem CreateCurrentItem(ChartItemCollection collection, ChartItemWrap itemWp)
        {
            return new CurrentChartItem(collection.Id, collection.CopyAndAdjustItemValue(itemWp.ChartItem, CoordinateType));
        }

        private ChartItem preChartItem;
        private void MoveCursor(Point position, bool isActive)
        {
            if (!IsShowCursor())
                return;

            List<CurrentChartItem> currentItems = new List<CurrentChartItem>();

            bool isChanged = false;
            if (_mainCollection != null)
            {
                var x = PointSnapper.SnapValue(position.X);
                var y = PointSnapper.SnapValue(position.Y);
                var v = GetValue(_mainCollection.GetValueFromPosition(position.Y));
                CreateCursorVisuals(x, y, v, isActive);

                var itemWp = _mainCollection.LocateCurrentChartItem(position);
                if (itemWp != null && preChartItem != itemWp.ChartItem)
                {
                    currentItems.Add(CreateCurrentItem(_mainCollection, itemWp));
                    preChartItem = itemWp.ChartItem;
                    isChanged = true;
                    
                }
            }

            if (isChanged)
            {
                foreach (var assitCollection in _assistCollections)
                {
                    var itemWp = assitCollection.LocateCurrentChartItem(position);
                    if (itemWp != null)
                    {
                        currentItems.Add(CreateCurrentItem(assitCollection, itemWp));
                    }
                }

                if (currentItems.Any())
                    RaiseCursorMovedEvent(currentItems);
            }
            
        }

        private void MoveCursor(int steps, bool isActive)
        {
            if (!IsShowCursor())
                return;

            List<CurrentChartItem> currentItems = new List<CurrentChartItem>();

            if (steps != 0)
            {
                if (_mainCollection != null)
                {
                    var itemWp = _mainCollection.LocateNearItem(steps);

                    if (itemWp != null)
                    {
                        CreateCursorVisuals(itemWp.Point.X, itemWp.Point.Y, itemWp.ChartItem.Value, isActive);

                        currentItems.Add(CreateCurrentItem(_mainCollection, itemWp));
                        
                    }
                }

                foreach (var assitCollection in _assistCollections)
                {
                    var itemWp = assitCollection.LocateNearItem(steps);
                    if (itemWp != null)
                    {
                        currentItems.Add(CreateCurrentItem(assitCollection, itemWp));
                    }
                }

                if(currentItems.Any())
                    RaiseCursorMovedEvent(currentItems);
            }
        }

        enum ActionType { Move, ZoomOut };
        class ActionQuery
        {
            public QueryData Data
            {
                get;
                set;
            }

            public ActionType Action
            {
                get;
                set;
            }

            public int Steps
            {
                get;
                set;
            }
        }

        private ActionQuery actionQuery;

        /// <summary>
        /// 响应请求结果。
        /// </summary>
        /// <param name="result">请求结果</param>
        public void QueryFinished(QueryDataResult<ChartItem> result)
        {
            if (actionQuery == null || actionQuery.Data.QueryId != result.QueryId)
                return;

            var actionQueryTemp = actionQuery;
            actionQuery = null;

            if (result.IsSucceeded)
            {
                if (_mainCollection != null)
                {
                    AddQueryDataEntry(result, _mainCollection);

                    foreach (var assistCol in _assistCollections)
                    {
                        AddQueryDataEntry(result, assistCol);
                    }
                }

                if (actionQueryTemp.Action == ActionType.Move)
                {
                    MoveChart(actionQueryTemp.Steps);
                }
                else
                {
                    ZoomChart(zoomOutTimes, true);
                }

            }
        }

        private void AddQueryDataEntry(QueryDataResult<ChartItem> result, ChartItemCollection coll)
        {
            if (coll == null)
                return;

            if (result.IsSucceeded && result.IsSucceeded)
            {
                if(result.IsHeadIncluded)
                    coll.AddChartItems(result.HeadItems, AddLocation.Head, result.IsHeadEnd != null ? result.IsHeadEnd.Value : false);
                if (result.IsTailIncluded)
                {
                    coll.AddChartItems(result.TailItems, AddLocation.Tail, result.IsTailEnd != null ? result.IsTailEnd.Value : false);
                }

            }
        }

        private bool MoveChart(int steps)
        {
            bool isNeedRedraw = false;
            if (steps != 0)
            {
                if (actionQuery == null)
                {
                    QueryData qData = null;
                    if (_mainCollection != null)
                    {
                        if (_mainCollection.Move(steps, out qData) == ActionResult.Succeeded)
                        {
                            isNeedRedraw = true;
                        }
                        else if (qData != null)
                        {
                            isNeedRedraw = true;

                            actionQuery = new ActionQuery();
                            actionQuery.Data = qData;
                            actionQuery.Action = ActionType.Move;
                            actionQuery.Steps = steps;
                            RaiseDataQueriedEvent(qData);

                            return isNeedRedraw;
                        }
                    }

                    for (int i = 0; i < _assistCollections.Count; i++)
                    {
                        _assistCollections[i].Move(steps, out qData);
                    }
                }
            }

            if (isNeedRedraw)
            {
                DrawVisuals();
            }

            return isNeedRedraw;
        }

        private bool ZoomChart(double times, bool autoAdjust)
        {
            bool isZoomOut = times < 1;
            bool isNeedRedraw = false;

            if (times == 1)
                return isNeedRedraw;
            
            if (_mainCollection != null && actionQuery == null)
            {
                QueryData qData = null;

                if (isZoomOut)
                {
                    var aResult = _mainCollection.Zoom(times, out qData, autoAdjust);
                    if (ActionResult.Succeeded == aResult)
                    {
                        for (int i = 0; i < _assistCollections.Count; i++)
                        {
                            _assistCollections[i].Zoom(times, out qData, autoAdjust);
                        }
                        isNeedRedraw = true;
                    }
                    else
                    {
                        if (qData != null)
                        {
                            isNeedRedraw = true;

                            actionQuery = new ActionQuery();
                            actionQuery.Data = qData;
                            actionQuery.Action = ActionType.ZoomOut;
                            RaiseDataQueriedEvent(qData);
                            return isNeedRedraw;
                        }
                    }
                }
                else
                {
                    QueryData qd = null;
                    var aResult = _mainCollection.Zoom(times, out qd, autoAdjust);

                    if (ActionResult.Succeeded == aResult)
                    {
                        for (int i = 0; i < _assistCollections.Count; i++)
                        {
                            _assistCollections[i].Zoom(times, out qd, autoAdjust);
                        }

                        isNeedRedraw = true;
                    }
                    else
                    {
                        isNeedRedraw = false;
                    }
                }

                if (isNeedRedraw)
                {
                    DrawVisuals();
                }

            }

            return isNeedRedraw;
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

            if (isNeedRedraw)
            {
                DrawVisuals();
            }
        }

        private IList<ChartItem> GetChartItems(Point start, Point end)
        {
            return _mainCollection.GetChartItems(start, end);
        }

        private void MoveCursor4ConnectionList(Point pos)
        {
            foreach (var wChar in _connectionList)
            {
                if (!wChar.IsAlive)
                {
                    continue;
                }

                ChartControl chart = wChar.Target as ChartControl;
                if (chart != null)
                {
                    chart.MoveCursor(pos, false);
                }
                
            }
        }

        private void MoveCursor4ConnectionList(int steps)
        {
            foreach (var wChar in _connectionList)
            {
                if (!wChar.IsAlive)
                {
                    continue;
                }

                ChartControl chart = wChar.Target as ChartControl;
                
                if (chart != null)
                {
                    chart.MoveCursor(steps, false);
                }
                
            }
        }

        private void MoveChart4ConnectionList(int steps)
        {
            foreach (var wChar in _connectionList)
            {
                if (!wChar.IsAlive)
                {
                    continue;
                }

                ChartControl chart = wChar.Target as ChartControl;
                
                if (chart != null)
                {
                    chart.MoveChart(steps);
                }
                
            }
        }

        private void ZoomChart4ConnectionList(double times, bool autoAdjust)
        {
            foreach (var wChar in _connectionList)
            {
                if (!wChar.IsAlive)
                {
                    continue;
                }

                ChartControl chart = wChar.Target as ChartControl;
                
                if (chart != null)
                {
                    chart.ZoomChart(times, autoAdjust);
                }
                
            }
        }

        private void ShowRegion4ConnectionList(Point start, Point end)
        {
            foreach (var wChar in _connectionList)
            {
                if (!wChar.IsAlive)
                {
                    continue;
                }

                ChartControl chart = wChar.Target as ChartControl;
                
                if (chart != null)
                {
                    chart.ShowRegion(start, end);
                }
                
            }
        }

        private void CreateCursorVisuals(double x, double y, double value, bool isShowhorizontalLine)
        {
            CreateYCursorVisual(x);

            if (isShowhorizontalLine)
            {
                CreateXCursorVisual(x, y, value);
            }
            else
            {
                RemoveXCursorLayer();
                RemoveCursorXDynamicLayer();
            }
        }

        private void CreateYCursorVisual(double x)
        {
            if (!IsCursorLayerCreated())
            {
                var chartRect = _collectionRect;
                CreateCursorLayer();

                using (var drawingContext = CreateCursorDC())
                {
                    IPen pen = DrawingObjectFactory.CreatePen(CursorLines, CursorLinesThickness);
                    if(CursorLinesDashes != null)
                        pen.Dashes = CursorLinesDashes;
                    pen.StartLineCap = pen.EndLineCap = PenLineCap.Square;

#if USINGCANVAS
#else
                    DrawingPen dPen = pen as DrawingPen;
                    dPen.Freeze();
#endif

                    drawingContext.DrawLine(pen, PointSnapper.SnapPoint(new Point(x, chartRect.Top)), PointSnapper.SnapPoint(new Point(x, chartRect.Bottom - 1)));
                    _cursorStart = x;

                    var yCursorLine = drawingContext.LastDrawnObject as FrameworkElement;
                    if (yCursorLine != null)
                    {
                        SetYCursorLine(yCursorLine);
                    }
                }
            }
            else
            {
                var mov = (int)(x - _cursorStart);
                //Trace.WriteLine("TramsferX " + mov);
                TransformCursorLayer(mov, 0);
            }
        }

        private string GetYCursorNumberFormat()
        {
            return YCursorFormat;
        }

        private void CreateXCursorVisual(double x, double y, double value)
        {
            var borderRect = GetChartBorderRect();
            var outsidebBorderRect = GetOutsideChartBorderRect(borderRect);

            if (!IsXCursorLayerCreated())
            {
                CreateCursorXLayer();

                using (var drawingContext = CreateCursorXDC())
                {
                    IPen pen = DrawingObjectFactory.CreatePen(CursorLines, CursorLinesThickness);
                    if (CursorLinesDashes != null)
                        pen.Dashes = CursorLinesDashes;
                    pen.StartLineCap = pen.EndLineCap = PenLineCap.Square;
#if USINGCANVAS
#else
                    (pen as DrawingPen).Freeze();
#endif
                    FrameworkElement xCursorDynamicBg = null;

                    //if (!IsYScalesInnerChart() && YScaleDock != YScaleDock.None)
                    
                    CalculateDynamicScaleSize(drawingContext);

                    var textRect = new Rect(0, (int)(y - _dynamicScaleSize.Value.Height / 2), _dynamicScaleSize.Value.Width, PointSnapper.RoundValue(_dynamicScaleSize.Value.Height));
                    if (YScaleDock == YScaleDock.Left)
                    {
                        _xDynamicScaleStart = textRect.X = (int)(outsidebBorderRect.Left - _dynamicScaleSize.Value.Width);
                    }
                    else if (YScaleDock == YScaleDock.Right)
                    {
                        _xDynamicScaleStart = textRect.X = outsidebBorderRect.Right;
                    }
                    else if (YScaleDock == YScaleDock.InnerLeft)
                    {
                        _xDynamicScaleStart = textRect.X = _collectionRect.Left;
                    }
                    else if (YScaleDock == YScaleDock.InnerRight)
                    {
                        _xDynamicScaleStart = textRect.X = (int)(_collectionRect.Right - _dynamicScaleSize.Value.Width);
                        
                    }
                    else
                    {
                        _xDynamicScaleStart = textRect.X = outsidebBorderRect.Right;
                    }

                    drawingContext.DrawRectangle(pen.Brush, null, textRect);
                    //Debug.WriteLine("TextRect " + textRect);
                    xCursorDynamicBg = drawingContext.LastDrawnObject as FrameworkElement;
                       

                    var x1 = PointSnapper.SnapValue(_collectionRect.Left);
                    var x2 = PointSnapper.SnapValue(_collectionRect.Right - 1);
                    drawingContext.DrawLine(pen, new Point(x1, y), new Point(x2, y));
                    _xCursorStart = y;

                    var xCursorLine = drawingContext.LastDrawnObject as FrameworkElement;
                    if (xCursorLine != null)
                    {
                        SetXCursorLine(xCursorLine, xCursorDynamicBg);
                    }
                    
                }
            }
            else
            {
                var mov = (int)(y - _xCursorStart);
                //Trace.WriteLine("TramsferY " + mov);
                TransformXCursorLayer(0, mov);
            }

            CreateDynamicScaleVisual(y, value);
            
        }

        private double GetMaxValue()
        {
            double maxValue = _mainCollection.GetMaxValue();
            return GetValue(maxValue);
        }

        private double GetValue(double value)
        {
            if (CoordinateType == CoordinateType.Log10)
            {
                return Math.Pow(10, value);
            }
            else
            {
                return value;
            }
        }

        private void CalculateDynamicScaleSize(IDrawingContext dc)
        {
            if (_dynamicScaleSize == null && _mainCollection != null)
            {
                _mainMaxValue = GetMaxValue();

                var textFormat = DrawingObjectFactory.CreateTextFormat(_mainMaxValue.ToString(GetYCursorNumberFormat()),
                    FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
                _dynamicScaleSize = new Size(textFormat.Width, textFormat.Height);
            }
        }

        private void CreateDynamicScaleVisual(double y, double value)
        {
            RemoveCursorXDynamicLayer(false);
            CreateCursorXDynamicLayer();

            using (var drawingContext = CreateCursorXDynamicDC())
            {
                var chartRect = _collectionRect;

                
                var textFormat = CreateDynamicScaleTextFormat(value.ToString(GetYCursorNumberFormat()));
                var ptText = new Point(_xDynamicScaleStart, y - _dynamicScaleSize.Value.Height / 2);

                //Debug.WriteLine("Text " + ptText + " height " + textFormat.Height);
                drawingContext.DrawText(textFormat, ptText);
            }
        }

        private double GetChange(double y1, double y2)
        {
            var v1 = GetValue(_mainCollection.GetValueFromPosition(y1));
            var v2 = GetValue(_mainCollection.GetValueFromPosition(y2));

            return (v2 - v1) / v1;
        }

        private bool IsYScalesInnerChart()
        {
            return YScaleDock == YScaleDock.InnerLeft || YScaleDock == YScaleDock.InnerRight;
        }

        private bool IsShowCursor()
        {
            return CursorLines != null;
        }

        private void ChangeCoordinateType(CoordinateType newValue, CoordinateType oldValue)
        {
            if (newValue != oldValue)
            {
                if (_mainCollection != null)
                {
                    _mainCollection.TransferCoordinate(newValue);
                }

                foreach (var assitColl in _assistCollections)
                {
                    assitColl.TransferCoordinate(newValue);
                }
            }
        }

        private void RecalcuteChartMargin()
        {
            var margin = new Thickness(0, 0, 0, 0);
            if (XScaleDock == XScaleDock.Bottom)
            {
                margin.Bottom += XScaleHeight;
            }
            switch (YScaleDock)
            {
                case YScaleDock.Left:
                    margin.Left += YScaleWidth;
                    break;
                case YScaleDock.Right:
                    margin.Right += YScaleWidth;
                    break;
            }
            chartMargin = margin;
        }
        #endregion

        #region Select
        private IInteractive selectedInterative = null;
        private bool IsInteractiveSelectedChanged;
        private int selectedNodeIndex = InteractiveConst.nodeIndexOutofTarget;
        private Point selectNodePosition;

        private void SelectGraphic(Point pt)
        {
            bool isDeleted = false;
            IsInteractiveSelectedChanged = false;
            IInteractive oldSelection = null;
            if (selectedInterative != null)
            {
                oldSelection = selectedInterative;
                selectedInterative.IsSelected = false;
                selectedInterative = null;
                IsInteractiveSelectedChanged = true;
            }
            
            foreach (var interactive in _interactiveList)
            {
                if (interactive.CanSelect && interactive.IsPointInRegion(pt))
                {
                    if (_isDeletingCustomGraphics)
                    {
                        var cGraphic = interactive as ICustomGraphics;
                        if (cGraphic != null)
                        {
                            _interactiveList.Remove(interactive);
                            _customGraphicsList.Remove(cGraphic);
                            isDeleted = true;
                        }
                    }
                    else
                    {
                        selectedInterative = interactive;
                        selectedInterative.IsSelected = true;
                        IsInteractiveSelectedChanged = true;
                    }
                    break;
                }
            }

            if (oldSelection == selectedInterative && selectedInterative != null)
            {
                StartUpdateGraphicLocation(pt);
            }

            if ((IsInteractiveSelectedChanged && oldSelection != selectedInterative) || isDeleted)
                DrawCustomGraphicList();
        }

        private void StartUpdateGraphicLocation(Point pt)
        {
            if (selectedInterative == null)
                return;

            selectedNodeIndex = selectedInterative.GetNodeIndex(pt);
            selectNodePosition = pt;

            //Debug.WriteLine(string.Format("StartUpdateGraphicLocation {0} {1}", selectedNodeIndex, selectNodePosition));
        }

        private void UpdateGraphicLocation(Point pt)
        {
            if (selectedInterative == null)
                return;

            //Debug.WriteLine(string.Format("UpdateGraphicLocation {0}", pt));

            if (selectedNodeIndex == InteractiveConst.nodeIndexOutofTarget)
            {
                return;
            }
            else
            {
                if (selectedNodeIndex == InteractiveConst.nodeIndexInTarget)
                {
                    TranslateTransform transform = new TranslateTransform()
                    {
                        X = pt.X - selectNodePosition.X,
                        Y = pt.Y - selectNodePosition.Y
                    };

                    selectedInterative.TranformPosition(transform);

                    ICustomGraphics graphic = selectedInterative as ICustomGraphics;
                    if (graphic != null)
                        ConvertCustomGraphicsCoordiate(graphic);
                }
                else
                {
                    selectedInterative.UpdateNodePosition(selectedNodeIndex, pt);
                    ICustomGraphics graphic = selectedInterative as ICustomGraphics;
                    if (graphic != null)
                        ConvertCustomGraphicsCoordiate(graphic, selectedNodeIndex);
                }

                DrawCustomGraphicList();

                selectNodePosition = pt;
            }
        }

        private void EndUpdateGraphicLocation()
        {
            if (selectedInterative == null)
                return;

            //Debug.WriteLine("EndUpdateGraphicLocation");

            selectedNodeIndex = InteractiveConst.nodeIndexOutofTarget;
            selectNodePosition.X = selectNodePosition.Y = 0;
        }

        private bool IsUpdatingGraphicLocation()
        {
            return selectedNodeIndex != InteractiveConst.nodeIndexOutofTarget;
        }

        #endregion

        #region CustomGraphic
        private void DrawCustomGraphicList(bool convertPoints = false)
        {
            if (convertPoints)
            {
                ConvertAllCustomGraphicsCoordiateBack();
            }

            if (_customGraphicsList != null)
            {
                using (var dcCGList = CreateCustomDC())
                {
                    foreach (var cg in _customGraphicsList)
                    {
                        cg.Draw(dcCGList);
                    }
                }
            }
        }

        private void ConvertCustomGraphicsCoordiate(ICustomGraphics graphics)
        {
            if (graphics.Points == null || graphics.ValuePoints == null)
            {
                return;
            }
            if (MainCollection != null)
            {
                for (int i = 0; i < graphics.Points.Length; i++)
                {
                    graphics.ValuePoints[i] = MainCollection.ConvertFromPoint(graphics.Points[i]);
                }
            }
        }

        private void ConvertCustomGraphicsCoordiate(ICustomGraphics graphics, int iPosition)
        {
            if (graphics.Points == null || graphics.ValuePoints == null)
            {
                return;
            }
            if (MainCollection != null)
            {
                graphics.ValuePoints[iPosition] = MainCollection.ConvertFromPoint(graphics.Points[iPosition]);
            }
        }

        private void ConvertCustomGraphicsCoordiateBack(ICustomGraphics graphics)
        {
            if (graphics.Points == null || graphics.ValuePoints == null)
            {
                return;
            }
            if (MainCollection != null)
            {
                for (int i = 0; i < graphics.ValuePoints.Length; i++)
                {
                    graphics.Points[i] = MainCollection.ConvertFromValuePoint(graphics.ValuePoints[i]);
                }
            }
        }

        private void ConvertAllCustomGraphicsCoordiateBack()
        {
            foreach (var graphic in _customGraphicsList)
            {
                ConvertCustomGraphicsCoordiateBack(graphic);
            }
        }
        #endregion

    }

    /// <summary>
    /// 当前位置下的图表数据项。通常用于返回位于鼠标下的图表数据项。
    /// </summary>
    public class CurrentChartItem
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="id">图表数据集合的ID</param>
        /// <param name="item">图表数据项</param>
        public CurrentChartItem(CollectionId id, ChartItem item)
        {
            Id = id;
            Item = item;
        }

        /// <summary>
        /// 图表数据集合的ID
        /// </summary>
        public CollectionId Id
        {
            get;
            private set;
        }

        /// <summary>
        /// 图表数据项
        /// </summary>
        public ChartItem Item
        {
            get;
            private set;
        }
    }
}
