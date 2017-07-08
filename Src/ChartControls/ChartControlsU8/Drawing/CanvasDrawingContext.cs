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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChartControls.Drawing;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml;
using System.Diagnostics;

namespace ChartControls.Drawing
{
    class CanvasDrawingContext : IDrawingContext
    {
        private Canvas context;

        public CanvasDrawingContext(Canvas canvas)
        {
            context = canvas;
        }

        public void DrawEllipse(Brush brush, IPen pen, Point center, double radiusX, double radiusY)
        {
            Ellipse e = new Ellipse()
            {
                Width = radiusX * 2,
                Height = radiusY * 2
            };

            AssginPenFillProperties(e, pen, brush);
            
            SetPosition(e, center);
            ApplyOperations(e);
        }

        public void DrawGeometry(Brush brush, IPen pen, Geometry geometry)
        {
            Path path = new Path()
            {
                Data = geometry
            };

            AssginPenFillProperties(path, pen, brush);

            context.Children.Add(path);
            ApplyOperations(path);
        }

        public void DrawImage(ImageSource imageSource, Rect rectangle)
        {
            Image img = new Image()
            {
                Source = imageSource,
                Width = rectangle.Width,
                Height = rectangle.Height
            };

            SetPosition(img, rectangle.Left, rectangle.Top);
            ApplyOperations(img);
        }

        public void DrawLine(IPen pen, Point point0, Point point1)
        {
            Line line = new Line()
            {
                X1 = point0.X,
                Y1 = point0.Y,
                X2 = point1.X,
                Y2 = point1.Y
            };

            AssginPenFillProperties(line, pen, null);
            context.Children.Add(line);
            ApplyOperations(line);
        }

        public void DrawRectangle(Brush brush, IPen pen, Rect rectangle)
        {
            Rectangle rect = new Rectangle()
            {
                Width = rectangle.Width,
                Height = rectangle.Height
            };

            AssginPenFillProperties(rect, pen, brush);
            SetPosition(rect, rectangle.Left, rectangle.Top);
            ApplyOperations(rect);
        }

        public void DrawRoundedRectangle(Brush brush, IPen pen, Rect rectangle, double radiusX, double radiusY)
        {
            Rectangle rect = new Rectangle()
            {
                Width = rectangle.Width,
                Height = rectangle.Height,
                RadiusX = radiusX,
                RadiusY = radiusY
            };

            AssginPenFillProperties(rect, pen, brush);
            SetPosition(rect, rectangle.Left, rectangle.Top);

            ApplyOperations(rect);
        }

        public void DrawText(ITextFormat formattedText, Point origin)
        {
            var fe = formattedText.LowObject as FrameworkElement;
            if (fe == null)
            {
                fe = new TextBlock()
                {
                    Text = formattedText.Content,
                    FontFamily = formattedText.FontFamily,
                    FontStretch = formattedText.Stretch,
                    FontStyle = formattedText.Style,
                    FontWeight = formattedText.Weight,
                    FontSize = formattedText.FontSize,
                    Foreground = formattedText.Foreground,
                    TextAlignment = formattedText.TextAlignment,
                    TextTrimming = formattedText.TextTrimming
                };
                
            }

            SetPosition(fe, origin);

            ApplyOperations(fe);
        }

        enum OperationType
        { 
            Unknown,
            Clip,
            Opacity,
            Tramsform
        }

        class Operation
        {
            public Operation(OperationType type, Object param)
            {
                OType = type;
                Parameter = param; 
            }
            public OperationType OType
            {
                get;
                set;
            }
            public Object Parameter
            {
                get;
                set;
            }
        }

        private Stack<Operation> operationStack = new Stack<Operation>();

        public void Pop()
        {
            operationStack.Pop();
        }

        public void PushClip(Geometry clipGeometry)
        {

            Debug.Assert(clipGeometry is RectangleGeometry, "Only rectangle is accepted");
            
            operationStack.Push(new Operation(OperationType.Clip, clipGeometry));
        }

        public void PushOpacity(double opacity)
        {
            operationStack.Push(new Operation(OperationType.Opacity, opacity));
        }

        public void PushTransform(Transform transform)
        {
            operationStack.Push(new Operation(OperationType.Tramsform, transform));
        }

        public object LowContext {
            get 
            { 
                return context;
            }
        }

        public object LastDrawnObject { 
            get {
                return context.Children.Last();
            } 
        }

        public void Dispose()
        {
        }

        private void SetPosition(FrameworkElement e, Point pt)
        {
            e.SetValue(Canvas.LeftProperty, pt.X);
            e.SetValue(Canvas.TopProperty, pt.Y);

            context.Children.Add(e);
        }

        private void SetPosition(FrameworkElement e, double x, double y)
        {
            e.SetValue(Canvas.LeftProperty, x);
            e.SetValue(Canvas.TopProperty, y);

            context.Children.Add(e);
        }

        private void AssginPenFillProperties(Shape share, IPen pen, Brush fill)
        {
            share.Fill = fill;
            if (pen == null)
                return;

            share.Stroke = pen.Brush;
            if(share.StrokeDashArray == null)
                share.StrokeDashArray = pen.Dashes;
            else
            {
                share.StrokeDashArray.Clear();
                if (pen.Dashes != null)
                {
                    foreach (var d in pen.Dashes)
                        share.StrokeDashArray.Add(d);
                }
                
                
            }
            share.StrokeDashCap = pen.DashCap;
            share.StrokeDashOffset = pen.DashOffest;
            share.StrokeEndLineCap = pen.EndLineCap;
            share.StrokeLineJoin = pen.LineJoin;
            share.StrokeMiterLimit = pen.MiterLimit;
            share.StrokeStartLineCap = pen.StartLineCap;
            share.StrokeThickness = pen.Thickness;
        }

        private void ApplyOperations(FrameworkElement fe)
        {
            foreach (var oper in operationStack)
            {
                switch(oper.OType)
                {
                    case OperationType.Clip:
                        context.Clip = oper.Parameter as RectangleGeometry;
                        break;
                    case OperationType.Opacity:
                        fe.Opacity = (double)oper.Parameter;
                        break;
                    case OperationType.Tramsform:
                        fe.RenderTransform = oper.Parameter as Transform;
                        break;
                }
            }
        }
    }
}
