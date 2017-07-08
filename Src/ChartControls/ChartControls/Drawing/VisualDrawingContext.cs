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
using System.Windows;
using System.Windows.Media;

namespace ChartControls.Drawing
{
    class VisualDrawingContext : IDrawingContext
    {
        private DrawingContext drawingContext;
        private bool isOwnded;

        public VisualDrawingContext(DrawingContext dc, bool isOwned = true)
        {
            drawingContext = dc;
            isOwnded = isOwned;
        }

        public void DrawEllipse(Brush brush, IPen pen, Point center, double radiusX, double radiusY)
        {
            drawingContext.DrawEllipse(brush, GetPen(pen), center, radiusX, radiusY);
        }
        public void DrawGeometry(Brush brush, IPen pen, Geometry geometry)
        {
            drawingContext.DrawGeometry(brush, GetPen(pen), geometry);
        }
        public void DrawImage(ImageSource imageSource, Rect rectangle)
        {
            drawingContext.DrawImage(imageSource, rectangle);
        }

        public void DrawLine(IPen pen, Point point0, Point point1)
        {
            drawingContext.DrawLine(GetPen(pen), point0, point1);
        }

        public void DrawRectangle(Brush brush, IPen pen, Rect rectangle)
        {
            drawingContext.DrawRectangle(brush, GetPen(pen), rectangle);
        }

        public void DrawRoundedRectangle(Brush brush, IPen pen, Rect rectangle, double radiusX, double radiusY)
        {
            drawingContext.DrawRoundedRectangle(brush, GetPen(pen), rectangle, radiusX, radiusY);
        }

        public void DrawText(ITextFormat formattedText, Point origin)
        {
            drawingContext.DrawText(formattedText.LowObject as FormattedText, origin);
        }

        public void Pop()
        {
            drawingContext.Pop();
        }

        public void PushClip(Geometry clipGeometry)
        {
            drawingContext.PushClip(clipGeometry);
        }

        public void PushOpacity(double opacity)
        {
            drawingContext.PushOpacity(opacity);
        }

        public void PushTransform(Transform transform)
        {
            drawingContext.PushTransform(transform);
        }

        public object LowContext
        {
            get
            {
                return drawingContext;
            }
        }

        public object LastDrawnObject { 
            get
            {
                return null;
            }
        }

        public void Dispose()
        {
            if (isOwnded && drawingContext != null)
            {
                drawingContext.Close();
                drawingContext = null;
            }
        }

        private Pen GetPen(IPen pen)
        {
            Pen p = null;
            if (pen != null)
            {
                p = (pen as DrawingPen).LowObject as Pen;

            }

            return p;
        }
    }
}
