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
using System.Windows.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace ChartControls
{
    public partial class ChartControl : Canvas
    {
        private Canvas _mainCanvas;
        private Canvas _customGraphicListCanvas;

        private DispatcherTimer tooltipTimer;
        private ToolTip toolTip;

        private Canvas _cursorCanvas;
        private Line _cursorYLine;
        private Line _cursorXLine;
        private Rectangle _cursorXDynamicBgRect;
        //private Rectangle _cursorXFlag;
        private TextBlock _xDynamicScaleTB;

        private Button _focusable;
        private GestureRecognizer _gestureRecognizer;

        private void InitControl()
        {
            SwitchInteractiveState(isInteractive);

            this.SizeChanged += ChartControl_SizeChanged;

            //CreateTooltipTimer();
            globalBitmapCache = new BitmapCache();

            CreateFocusable();
        }

        #region Focusable
        private void CreateFocusable()
        {
            _focusable = new Button();
            _focusable.SetValue(Canvas.LeftProperty, 0);
            _focusable.SetValue(Canvas.TopProperty, 0);

            _focusable.KeyDown += ChartControl_KeyDown;
            _focusable.KeyUp += ChartControl_KeyUp;
        }

        private void AddFocusable(bool isFocused)
        {
            Children.Add(_focusable);
            _focusable.MinHeight = _focusable.MinWidth = _focusable.Width = _focusable.Height = 0;
            if (isFocused)
                Focus();
        }

        private void Focus()
        {
            if (_focusable != null)
            {
                _focusable.Focus(FocusState.Programmatic);
            }
        }
        #endregion

        #region Event Methods
        void ChartControl_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_mainCollection == null)
                return;

            int steps = 0;

            var state = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
            if (state == CoreVirtualKeyStates.None)
            {
                if (e.Key == VirtualKey.Left)
                {
                    steps = -1;
                }
                else if (e.Key == VirtualKey.Right)
                {
                    steps = 1;
                }
                else if (e.Key == VirtualKey.Up)
                {
                    Zoom(zoomInTimes);
                    e.Handled = true;
                }
                else if (e.Key == VirtualKey.Down)
                {
                    Zoom(zoomOutTimes);
                    e.Handled = true;
                }

                if (steps != 0)
                {
                    MoveCursorPosition(steps);
                    e.Handled = true;
                }
            }
            else if (state == CoreVirtualKeyStates.Down)
            {
                if (e.Key == VirtualKey.Left)
                {
                    steps = -moveChartSteps;
                    CreateTimer();
                }
                else if (e.Key == VirtualKey.Right)
                {
                    steps = moveChartSteps;
                    CreateTimer();
                }

                if (steps != 0)
                {
                    MoveChartPosition(steps);

                    e.Handled = true;
                }
            }
        }

        private void ChartControl_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_mainCollection == null)
                return;

            if (e.Key == VirtualKey.Left || e.Key == VirtualKey.Right)
            {
                moveChartSteps = 1;

                if (dispatcherTimer != null && dispatcherTimer.IsEnabled)
                {
                    dispatcherTimer.Stop();
                }

                e.Handled = true;
            }

        }

        DispatcherTimer dispatcherTimer;
        private void CreateTimer()
        {
            if (dispatcherTimer == null)
            {
                dispatcherTimer = new DispatcherTimer();
                dispatcherTimer.Tick += new EventHandler<object>(dispatcherTimer_Tick);
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

        private void dispatcherTimer_Tick(object sender, object e)
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
        private void ChartControl_PointPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if (_mainCollection == null)
                return;

            var ppointer = e.GetCurrentPoint(this);
            
            _gestureRecognizer.ProcessDownEvent(ppointer);

            if (startPoint != null)
            {
                DropPointerAction();
                return;
            }

            startPoint = ppointer.Position;

            if (!IsPointInChart(startPoint.Value))
            {
                return;
            }

            Focus();
            if (_drawingCustomGraphics != null)
            {
                if (!_isCustomGraphicsDrawingStarted)
                {
                    _drawingCustomGraphics.StartDraw(startPoint.Value);
                    _isCustomGraphicsDrawingStarted = true;
                }
            }
            else 
            {
                pointerAction = PointerStartAction;
                midPoint = null;

                if (pointerAction == PointerAction.Measure)
                {
                    midPoint = null;

                    if (MeasureGraphics != null)
                    {
                        MeasureGraphics.StartDraw(startPoint.Value);
                    }
                }
            }
        }

        private void ChartControl_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            
            if (_mainCollection == null)
                return;

            var ppointer = e.GetCurrentPoint(this);
            var curPos = ppointer.Position;

            _gestureRecognizer.ProcessUpEvent(ppointer);

            if (!IsPointInChart(curPos))
            {
                startPoint = null;
                pointerAction = PointerAction.None;
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
                startPoint = null;

            }
            else if (IsUpdatingGraphicLocation())
            {
                EndUpdateGraphicLocation();
                startPoint = null;
            }
            else if(pointerAction == PointerAction.None)
            {
                startPoint = null;
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

        private void ChartControl_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;

            if (!IsShowCursor() || _mainCollection == null)
                return;

            var cursorPos = e.GetCurrentPoint(this).Position;
            _gestureRecognizer.ProcessMoveEvents(e.GetIntermediatePoints(this));
            //Debug.WriteLine("Move " + cursorPos.ToString());
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
                CreateSelectionRectVisual(PointSnapper.RoundPoint(startPoint.Value), PointSnapper.RoundPoint(cursorPos));
            }
            else if (pointerAction == PointerAction.Measure)
            {
                CreateRulerVisual(PointSnapper.SnapPoint(startPoint.Value), PointSnapper.SnapPoint(cursorPos), GetChange(startPoint.Value.Y, cursorPos.Y));
            }
        }

        private void ChartControl_LostPointerCapture(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("ChartControl_PointerExited");
            _gestureRecognizer.CompleteGesture();

            DropPointerAction();
            e.Handled = true;
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
            Children.Clear();

            _assistCollections.Clear();

            preChartItem = null;

            CoordinateType = defaultCoordinateType;

            _customGraphicsList.Clear();

            _extraDataGraphicsList.Clear();
			
			CoordinateType = defaultCoordinateType;
            PointerStartAction = defaultStartAction;

            _connectionList.Clear();
        }

        private Canvas CreateCanvas()
        {
            var canvas = new Canvas();
            canvas.CacheMode = globalBitmapCache;

            return canvas;
        }

        private void CreateLayers()
        {
            bool isFocused = false;
            if(_focusable != null)
            {
                isFocused = _focusable.FocusState != FocusState.Unfocused;
            }
            Children.Clear();

            AddFocusable(isFocused);

            _mainCanvas = CreateCanvas();

            var renderRect = _collectionRect;
            var borderRect = GetChartBorderRect();
            var whileRect = new Rect(0, 0, ActualWidth, ActualHeight);

            Clip = new RectangleGeometry()
            {
                Rect = whileRect

            };

            if (_customGraphicListCanvas != null)
            {
                _customGraphicListCanvas = null;
            }

            if (_customGraphicsList != null)
            {
                _customGraphicListCanvas = CreateCanvas();
                //_children.Add(_customGraphicListCanvas);
            }
        }

        private void CreateCursorLayer(bool includeX = false)
        {
            _cursorCanvas = CreateCanvas();
            Children.Add(_cursorCanvas);

            if (includeX)
            {
                CreateCursorXDynamicLayer();
            }
        }

        private void CreateCursorXLayer()
        {
        }

        private void CreateCursorXDynamicLayer()
        {
            _xDynamicScaleTB = new TextBlock();
            
            //_cursorCanvas.Children.Add(_xDynamicScaleTB);
        }

        private void RemoveCursorLayer()
        {
            RemoveXCursorLayer();
            RemoveCursorXDynamicLayer();

            if (_cursorCanvas != null)
            {
                if (_cursorYLine != null)
                {
                    _cursorCanvas.Children.Remove(_cursorYLine);
                    _cursorYLine = null;
                }

                Children.Remove(_cursorCanvas);
                _cursorCanvas = null;
            }
        }

        private void RemoveXCursorLayer()
        {
            if (_cursorXLine != null)
            {
                _cursorCanvas.Children.Remove(_cursorXLine);
                _cursorXLine = null;
            }

            if (_cursorXDynamicBgRect != null)
            {
                _cursorCanvas.Children.Remove(_cursorXDynamicBgRect);
                _cursorXDynamicBgRect = null;
            }
        }

        private void RemoveCursorXDynamicLayer(bool cleanDynSize = true)
        {
            if (_cursorCanvas != null)
            {
                if (_xDynamicScaleTB != null)
                {
                    _cursorCanvas.Children.Remove(_xDynamicScaleTB);
                    _xDynamicScaleTB = null;
                }
            }
        }

        private void PopCursorLayer()
        {
            if(_cursorCanvas != null)
                Children.Remove(_cursorCanvas);
        }

        private void PushCursorLayer()
        {
            if (_cursorCanvas != null)
                Children.Add(_cursorCanvas);
        }

        private IDrawingContext CreateBackgroundDC()
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(this);

            return cdc;
        }

        private IDrawingContext CreateMainDC(IDrawingContext bgDC)
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(_mainCanvas);
            Children.Add(_mainCanvas);
            return cdc;
        }

        private IDrawingContext CreateCursorDC()
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(_cursorCanvas);

            return cdc;
        }

        private IDrawingContext CreateCursorXDC()
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(_cursorCanvas);
            return cdc;
        }

        private IDrawingContext CreateCursorXDynamicDC()
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(_cursorCanvas);

            return cdc;
        }

        private IDrawingContext CreateCustomDC()
        {
            CanvasDrawingContext cdc = new CanvasDrawingContext(_customGraphicListCanvas);
            Children.Add(_customGraphicListCanvas);
            return cdc;
        }

        private bool IsCursorLayerCreated()
        {
            return _cursorCanvas != null && _cursorYLine != null;
        }

        private void TransformCursorLayer(double x, double y)
        {
            _cursorYLine.RenderTransform = new TranslateTransform()
            {
                X = x,
                Y = y
            };
        }

        private bool IsXCursorLayerCreated()
        {
            return _cursorCanvas != null && _cursorXLine != null;
        }

        private void TransformXCursorLayer(double x, double y)
        {
            _cursorXLine.RenderTransform = new TranslateTransform()
            {
                X = x,
                Y = y
            };

            if(_cursorXDynamicBgRect != null)
            {
                _cursorXDynamicBgRect.RenderTransform = new TranslateTransform()
                {
                    X = x,
                    Y = y
                };
            }
        }

        private void SetYCursorLine(FrameworkElement yCursorLine)
        {
            _cursorYLine = yCursorLine as Line;
        }

        private void SetXCursorLine(FrameworkElement xCursorLine, FrameworkElement xCursorDynamicValueBgRect)
        {
            _cursorXLine = xCursorLine as Line;
            _cursorXDynamicBgRect = xCursorDynamicValueBgRect as Rectangle;
        }

        private ITextFormat CreateDynamicScaleTextFormat(string text)
        {
            _xDynamicScaleTB.Text = text;
            return DrawingObjectFactory.CreateTextFormat(_xDynamicScaleTB,
                 FlowDirection, FontFamily, FontStyle, FontWeight, FontStretch, FontSize, Foreground);
        }

        void SwitchInteractiveState(bool isInteractive)
        {
            if (isInteractive)
            {
                this.PointerMoved += ChartControl_PointerMoved;
                this.PointerPressed += ChartControl_PointPressed;
                this.PointerReleased += ChartControl_PointerReleased;


                this.PointerExited += ChartControl_LostPointerCapture;
                this.PointerCaptureLost += ChartControl_LostPointerCapture;
                this.PointerCanceled += ChartControl_PointerCanceled;
                if (_focusable != null)
                {
                    _focusable.KeyDown += ChartControl_KeyDown;
                    _focusable.KeyUp += ChartControl_KeyUp;
                }

                if (_gestureRecognizer == null)
                    _gestureRecognizer = new GestureRecognizer();
                _gestureRecognizer.GestureSettings = GestureSettings.ManipulationScale;

                _gestureRecognizer.ManipulationStarted += _gestureRecognizer_ManipulationStarted;
                _gestureRecognizer.ManipulationUpdated += _gestureRecognizer_ManipulationUpdated;
                _gestureRecognizer.ManipulationCompleted += _gestureRecognizer_ManipulationCompleted;
            }
            else
            {
                this.PointerMoved -= ChartControl_PointerMoved;
                this.PointerPressed -= ChartControl_PointPressed;
                this.PointerReleased -= ChartControl_PointerReleased;


                this.PointerExited -= ChartControl_LostPointerCapture;
                this.PointerCaptureLost -= ChartControl_LostPointerCapture;
                this.PointerCanceled -= ChartControl_PointerCanceled;
                if (_focusable != null)
                {
                    _focusable.KeyDown -= ChartControl_KeyDown;
                    _focusable.KeyUp -= ChartControl_KeyUp;
                }

                _gestureRecognizer.GestureSettings = GestureSettings.None;

                _gestureRecognizer.ManipulationStarted -= _gestureRecognizer_ManipulationStarted;
                _gestureRecognizer.ManipulationUpdated -= _gestureRecognizer_ManipulationUpdated;
                _gestureRecognizer.ManipulationCompleted -= _gestureRecognizer_ManipulationCompleted;

            }
        }

        private void ChartControl_PointerCanceled(object sender, PointerRoutedEventArgs e)
        {
            Debug.WriteLine("ChartControl_PointerCanceled");
            _gestureRecognizer.CompleteGesture();
        }

        private void _gestureRecognizer_ManipulationCompleted(GestureRecognizer sender, ManipulationCompletedEventArgs args)
        {
            Debug.WriteLine("CompleteManipulation=" + args.Cumulative.Scale + " Point=" + args.Position);
        }

        private void _gestureRecognizer_ManipulationUpdated(GestureRecognizer sender, ManipulationUpdatedEventArgs args)
        {
            Debug.WriteLine("UpdateManipulation=" + args.Cumulative.Scale + "Delta=" + args.Delta.Scale);
            
            if(args.Delta.Scale > 1)
            {
                var zoomIn = zoomInChange != 1 ? zoomInChange : args.Delta.Scale;
                if (!Zoom(zoomIn, false))
                    zoomInChange *= args.Delta.Scale;
                else
                    zoomInChange = 1;
            }
            else if(args.Delta.Scale < 1)
            {
                var zoomOut = zoomOutChange != 1 ? zoomOutChange : args.Delta.Scale;
                if (!Zoom(zoomOut, false))
                    zoomOutChange *= args.Delta.Scale;
                else
                    zoomOutChange = 1;
            }

        }

        private double zoomInChange = 1;
        private double zoomOutChange = 1;
        private void _gestureRecognizer_ManipulationStarted(GestureRecognizer sender, ManipulationStartedEventArgs args)
        {
            Debug.WriteLine("StartManipulation=" + args.Cumulative.Scale + " Point=" + args.Position + 
                "Expan=" + args.Cumulative.Expansion + "Tran=" + args.Cumulative.Translation);

            zoomInChange = 1;
            zoomOutChange = 1;

            //DropPointerAction();

        }

        private Rect GetChartBorderRect()
        {
            if (ActualWidth == 0 || ActualHeight == 0)
            {
                return new Rect(0, 0, 0, 0);
            }
            else
            {
                return new Rect(chartMargin.Left, chartMargin.Top, (int)(ActualWidth - chartMargin.Left - chartMargin.Right),
                (int)(ActualHeight - chartMargin.Top - chartMargin.Bottom));
            }
        }

        private Rect GetOutsideChartBorderRect(Rect borderRect)
        {
            return new Rect(borderRect.Left, borderRect.Top, borderRect.Width, borderRect.Height);
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

        private void tooltipTimer_Tick(object sender, object e)
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
    }
}
