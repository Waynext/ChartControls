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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ChartControls
{
    public partial class ChartControl : FrameworkElement
    {
        private VisualCollection _children;

        private DrawingVisual _backgroundVisual;
        //private DrawingVisual _mainVisual;
        private DrawingVisual _customGraphicListVisual;

        private DispatcherTimer tooltipTimer;
        private ToolTip toolTip;

        private DrawingVisual _cursorVisual;
        private DrawingVisual _xCursorVisual;
        private DrawingVisual _xDynamicScalVisual;


        private void InitControl()
        {
            _children = new VisualCollection(this);

            SwitchInteractiveState(isInteractive);

            Focusable = true;
            FocusVisualStyle = null;

            CreateTooltipTimer();

            globalBitmapCache = new BitmapCache();
            globalBitmapCache.SnapsToDevicePixels = true;

            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);
        }

        #region Event Methods
        void ChartControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (_mainCollection == null)
                return;

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
                    Zoom(zoomInTimes);
                }
                else if (e.Key == Key.Down)
                {
                    Zoom(zoomOutTimes);
                }

                if (steps != 0)
                {
                    MoveCursorPosition(steps);
                }
            }
            else if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                if (e.Key == Key.Left)
                {
                    steps = -moveChartSteps;
                    CreateTimer();
                }
                else if (e.Key == Key.Right)
                {
                    steps = moveChartSteps;
                    CreateTimer();
                }

                MoveChartPosition(steps);
            }

            e.Handled = true;
        }

        private void ChartControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (_mainCollection == null)
                return;

            if (e.Key == Key.Left || e.Key == Key.Right)
            {
                moveChartSteps = 1;

                if (dispatcherTimer != null && dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Stop();

                }
            }

        }

        DispatcherTimer dispatcherTimer;
        private void CreateTimer()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                dispatcherTimer.Start();
            }
            else
            {
                if (!dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Start();
                }
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Updating the Label which displays the current second
            moveChartSteps += 1;

            if (moveChartSteps == 30)
            {
                if (dispatcherTimer != null && dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Stop();
                }
            }
        }

        private Point? startPoint;
        private Point? midPoint;
        private void ChartControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_mainCollection == null)
                return;

            if (!IsFocused)
                this.Focus();

            startPoint = e.GetPosition(this);

            if (!IsPointInChart(startPoint.Value))
            {
                return;
            }

            if (_drawingCustomGraphics != null)
            {
                if (!_isCustomGraphicsDrawingStarted)
                {
                    _drawingCustomGraphics.StartDraw(startPoint.Value);
                    _isCustomGraphicsDrawingStarted = true;
                }
            }
            else if (pointerAction == PointerAction.None)
            {
                SelectGraphic(startPoint.Value);
                if (!IsInteractiveSelectedChanged && !IsUpdatingGraphicLocation())
                {
                    midPoint = null;
                    pointerAction = PointerStartAction;

                    if (pointerAction == PointerAction.Measure)
                    {
                        if (MeasureGraphics != null)
                        {
                            MeasureGraphics.StartDraw(startPoint.Value);
                        }
                    }
                }


            }
        }

        private void ChartControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (_mainCollection == null)
                return;

            var curPos = e.GetPosition(this);

            if (!IsPointInChart(curPos))
            {
                return;
            }

            if (_drawingCustomGraphics != null)
            {
                if (CreateCurrentCustomGraphicVisual(curPos, true))
                {
                    _customGraphicsList.Add(_drawingCustomGraphics);
                    ConvertCustomGraphicsCoordiate(_drawingCustomGraphics);
                    var iactive = _drawingCustomGraphics as IInteractive;
                    if (iactive != null)
                    {
                        _interactiveList.Add(iactive);
                    }

                    _drawingCustomGraphics = null;
                    _isCustomGraphicsDrawingStarted = false;


                    RemoveCursorLayer();

                    DrawCustomGraphicList();
                }

            }
            else if (IsUpdatingGraphicLocation())
            {
                EndUpdateGraphicLocation();
            }
            else if (startPoint != null && startPoint.Value.X == curPos.X && startPoint.Value.Y == curPos.Y)
            {
                startPoint = null;
                RemoveCursorLayer();
            }
            else if (pointerAction == PointerAction.ZoomIn)
            {
                if (startPoint != null)
                {
                    DisplayRegion(startPoint.Value, curPos);

                    startPoint = null;
                }

                if (midPoint != null)
                {
                    RemoveCursorLayer();
                }
            }
            else if (pointerAction == PointerAction.Measure)
            {
                startPoint = null;
                RemoveCursorLayer();
            }
            else if (pointerAction == PointerAction.Select)
            {
                var items = GetChartItems(startPoint.Value, curPos);
                startPoint = null;
                if (midPoint != null)
                {
                    RemoveCursorLayer();
                }

                if (items != null && items.Any())
                {
                    RaiseSelectItemsEvent(items);
                }
            }

            pointerAction = PointerAction.None;
        }

        private void ChartControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            pointerAction = PointerAction.Select;
            ChartControl_MouseLeftButtonDown(sender, e);
        }

        private void ChartControl_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChartControl_MouseLeftButtonUp(sender, e);
        }

        private void ChartControl_MouseMove(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            if (!IsShowCursor() || _mainCollection == null)
                return;

            var cursorPos = e.GetPosition(this);
            //Trace.WriteLine("Move " + cursorPos.ToString());
            if (!IsPointInChart(cursorPos))
            {
                DropPointerAction();
                return;
            }

            midPoint = cursorPos;

            if (_drawingCustomGraphics != null)
            {
                if (_isCustomGraphicsDrawingStarted)
                    CreateCurrentCustomGraphicVisual(cursorPos, false);
            }
            else if (IsUpdatingGraphicLocation())
            {
                UpdateGraphicLocation(cursorPos);
            }
            else if (pointerAction == PointerAction.None)
            {
                SetCursorPosition(cursorPos);

                RestartTooltipTimer();
            }
            else if (pointerAction == PointerAction.ZoomIn || pointerAction == PointerAction.Select)
            {
                CreateSelectionRectVisual(PointSnapper.SnapPoint(startPoint.Value), PointSnapper.SnapPoint(cursorPos));
            }
            else if (pointerAction == PointerAction.Measure)
            {
                CreateRulerVisual(PointSnapper.SnapPoint(startPoint.Value), PointSnapper.SnapPoint(cursorPos), GetChange(startPoint.Value.Y, cursorPos.Y));
            }
        }

        private void ChartControl_LostMouseCapture(object sender, MouseEventArgs e)
        {
            DropPointerAction();
        }

        private void DropPointerAction()
        {
            if (pointerAction == PointerAction.ZoomIn || pointerAction == PointerAction.Measure || pointerAction == PointerAction.Select)
            {
                pointerAction = PointerAction.None;
                startPoint = null;
                RemoveCursorLayer();
            }

            StopHideToolTip();

            if (_drawingCustomGraphics != null)
            {
                _isCustomGraphicsDrawingStarted = false;
                RemoveCursorLayer();
            }
            else if (IsUpdatingGraphicLocation())
                EndUpdateGraphicLocation();
        }
        #endregion

        #region Private methods
        private void ResetChartControl()
        {
            _children.Clear();

            _assistCollections.Clear();

            preChartItem = null;

            _customGraphicsList.Clear();

            _extraDataGraphicsList.Clear();

            _interactiveList.Clear();

            _connectionList.Clear();
        }

        private DrawingVisual CreateVisual()
        {
            var visual = new DrawingVisual();
            visual.CacheMode = globalBitmapCache;

            RenderOptions.SetEdgeMode(visual, EdgeMode.Aliased);

            return visual;
        }

        private void CreateLayers()
        {
            if (_backgroundVisual != null)
            {
                _children.Remove(_backgroundVisual);
                _backgroundVisual = null;
            }

            _backgroundVisual = CreateVisual();
            _children.Add(_backgroundVisual);

            /*if (_mainVisual != null)
            {
                _children.Remove(_mainVisual);
                _mainVisual = null;
            }

            _mainVisual = CreateVisual();
            _children.Add(_mainVisual);*/
            var renderRect = _collectionRect;
            var borderRect = GetChartBorderRect();
            var whileRect = new Rect(0, 0, ActualWidth, ActualHeight);

            _backgroundVisual.Clip = new RectangleGeometry(whileRect);
            //_mainVisual.Clip = new RectangleGeometry(borderRect);

            if (_customGraphicListVisual != null)
            {
                _children.Remove(_customGraphicListVisual);
                _customGraphicListVisual = null;
            }

            if (_customGraphicsList != null)
            {
                _customGraphicListVisual = CreateVisual();
                _children.Add(_customGraphicListVisual);
            }
        }

        private void CreateCursorLayer(bool includeX = false)
        {
            _cursorVisual = CreateVisual();
            _children.Add(_cursorVisual);

            if (includeX)
            {
                CreateCursorXLayer();
                CreateCursorXDynamicLayer();
            }
        }

        private void CreateCursorXLayer()
        {
            _xCursorVisual = CreateVisual();
            _children.Add(_xCursorVisual);
        }

        private void CreateCursorXDynamicLayer()
        {
            _xDynamicScalVisual = CreateVisual();
            _children.Add(_xDynamicScalVisual);
        }

        private void RemoveCursorLayer()
        {
            if (_cursorVisual != null)
            {
                _children.Remove(_cursorVisual);
                _cursorVisual = null;
            }

            RemoveXCursorLayer();
            RemoveCursorXDynamicLayer();
        }

        private void RemoveXCursorLayer()
        {
            if (_xCursorVisual != null)
            {
                _children.Remove(_xCursorVisual);
                _xCursorVisual = null;
            }
        }

        private void RemoveCursorXDynamicLayer(bool cleanDynSize = true)
        {
            if (_xDynamicScalVisual != null)
            {
                _children.Remove(_xDynamicScalVisual);
                _xDynamicScalVisual = null;
            }

            if(cleanDynSize)
                _dynamicScaleSize = null;
        }

        private void PopCursorLayer()
        {
            if (_cursorVisual != null)
            {
                _children.Remove(_cursorVisual);
            }
            if(_xCursorVisual != null)
                _children.Remove(_xCursorVisual);

            if(_xDynamicScalVisual != null)
                _children.Remove(_xDynamicScalVisual);
        }

        private void PushCursorLayer()
        {
            if (_cursorVisual != null)
                _children.Add(_cursorVisual);
            if (_xCursorVisual != null)
                _children.Add(_xCursorVisual);
            if (_xDynamicScalVisual != null)
                _children.Add(_xDynamicScalVisual);
        }

        private IDrawingContext CreateBackgroundDC()
        {
            var dc = _backgroundVisual.RenderOpen();
            VisualDrawingContext vdc = new VisualDrawingContext(dc);
            return vdc;
        }

        private IDrawingContext CreateMainDC(IDrawingContext bgDc)
        {
            VisualDrawingContext vdc = new VisualDrawingContext(bgDc.LowContext as DrawingContext, false);
            return vdc;
        }

        private IDrawingContext CreateCursorDC()
        {
            var dc = _cursorVisual.RenderOpen();
            VisualDrawingContext vdc = new VisualDrawingContext(dc);
            return vdc;
        }

        private IDrawingContext CreateCursorXDC()
        {
            var dc = _xCursorVisual.RenderOpen();
            VisualDrawingContext vdc = new VisualDrawingContext(dc);
            return vdc;
        }

        private IDrawingContext CreateCursorXDynamicDC()
        {
            var dc = _xDynamicScalVisual.RenderOpen();
            VisualDrawingContext vdc = new VisualDrawingContext(dc);
            return vdc;
        }

        private IDrawingContext CreateCustomDC()
        {
            var dc = _customGraphicListVisual.RenderOpen();
            VisualDrawingContext vdc = new VisualDrawingContext(dc);
            return vdc;
        }

        private bool IsCursorLayerCreated()
        {
            return _cursorVisual != null;
        }

        private void TransformCursorLayer(double x, double y)
        {
            _cursorVisual.Transform = new TranslateTransform(x, y);
        }

        private bool IsXCursorLayerCreated()
        {
            return _xCursorVisual != null;
        }

        private void TransformXCursorLayer(double x, double y)
        {
            _xCursorVisual.Transform = new TranslateTransform(x, y);
        }

        private void SetYCursorLine(FrameworkElement yCursorLine)
        {
        }

        private void SetXCursorLine(FrameworkElement xCursorLine, FrameworkElement xCursorDynamicValueBgRect)
        {
        }

        private ITextFormat CreateDynamicScaleTextFormat(string text)
        {
            return DrawingObjectFactory.CreateTextFormat(text,
                 FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
        }

        void SwitchInteractiveState(bool isInteractive)
        {
            if (isInteractive)
            {
                this.KeyDown += ChartControl_KeyDown;
                this.KeyUp += ChartControl_KeyUp;

                this.MouseMove += ChartControl_MouseMove;
                this.MouseLeftButtonDown += ChartControl_MouseLeftButtonDown;
                this.MouseLeftButtonUp += ChartControl_MouseLeftButtonUp;
                this.MouseRightButtonDown += ChartControl_MouseRightButtonDown;
                this.MouseRightButtonUp += ChartControl_MouseRightButtonUp;

                this.LostMouseCapture += ChartControl_LostMouseCapture;
                this.MouseLeave += ChartControl_LostMouseCapture;

            }
            else
            {
                this.KeyDown -= ChartControl_KeyDown;
                this.KeyUp -= ChartControl_KeyUp;

                this.MouseMove -= ChartControl_MouseMove;
                this.MouseLeftButtonDown -= ChartControl_MouseLeftButtonDown;
                this.MouseLeftButtonUp -= ChartControl_MouseLeftButtonUp;
                this.MouseRightButtonDown -= ChartControl_MouseRightButtonDown;
                this.MouseRightButtonUp -= ChartControl_MouseRightButtonUp;

                this.LostMouseCapture -= ChartControl_LostMouseCapture;
                this.MouseLeave -= ChartControl_LostMouseCapture;

            }

        }

        private Rect GetChartBorderRect()
        {
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                return new Rect(0, 0, 0, 0);
            }
            else
            {
                double offset = BorderThickness / 2;

                return new Rect(chartMargin.Left + offset, chartMargin.Top + offset, (int)(ActualWidth - chartMargin.Left - chartMargin.Right - BorderThickness),
                (int)(ActualHeight - chartMargin.Top - chartMargin.Bottom - BorderThickness));

                
            }
        }

        private Rect GetOutsideChartBorderRect(Rect borderRect)
        {
            double offset = BorderThickness / 2;
            return new Rect(borderRect.Left - offset, borderRect.Top - offset, borderRect.Width + BorderThickness, borderRect.Height + BorderThickness);
        }
        #endregion

        #region ToolTip
        private const int tooltipDelay = 1;
        private Point? tooltipPoint;
        private IInteractive tooltipActiveItem;
        private int tooltipActiveItemIndex = -1;

        private void CreateTooltipTimer()
        {
            tooltipTimer = new DispatcherTimer();
            tooltipTimer.Interval = TimeSpan.FromSeconds(tooltipDelay);
            tooltipTimer.Tick += tooltipTimer_Tick;
        }

        private void RestartTooltipTimer()
        {
            if (tooltipTimer != null)
            {
                tooltipTimer.Stop();
                tooltipTimer.Start();
            }
        }

        private void tooltipTimer_Tick(object sender, EventArgs e)
        {
            if (midPoint == null || tooltipPoint == midPoint)
            {
                return;
            }

            foreach (var iactive in _interactiveList)
            {
                if (iactive.HasTooltip)
                {
                    int newIndex = iactive.GetNodeIndex(midPoint.Value);
                    if (newIndex == -1)
                        continue;

                    if (tooltipActiveItem != iactive || tooltipActiveItemIndex != newIndex)
                    {
                        HideToolTip();

                        tooltipPoint = midPoint;
                        if (iactive.ToolTip != null)
                        {
                            toolTip = iactive.ToolTip as ToolTip;
                            if (toolTip == null)
                            {
                                toolTip = new ToolTip();
                                toolTip.Content = iactive.ToolTip;
                            }

                            toolTip.IsOpen = true;
                        }

                        tooltipActiveItem = iactive;
                        tooltipActiveItemIndex = newIndex;
                    }

                    return;
                }
            }

            HideToolTip();
            tooltipActiveItem = null;
            tooltipActiveItemIndex = -1;
        }

        private void HideToolTip()
        {
            if (toolTip != null && toolTip.IsOpen)
            {
                toolTip.IsOpen = false;
            }

            tooltipPoint = null;
        }

        private void StopHideToolTip()
        {
            if (tooltipTimer != null)
                tooltipTimer.Stop();

            HideToolTip();

            tooltipActiveItem = null;
            tooltipActiveItemIndex = -1;

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
}
