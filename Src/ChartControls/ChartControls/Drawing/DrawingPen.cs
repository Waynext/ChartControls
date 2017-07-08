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
using System.Windows.Media;

namespace ChartControls.Drawing
{
    class DrawingPen : IPen
    {
        private Pen pen;

        public DrawingPen(Brush brush, double thickness)
        {
            pen = new Pen(brush, thickness);
        }

        public Brush Brush
        {
            get { return pen.Brush; }
            set { pen.Brush = value; }
        }

        public PenLineCap DashCap
        {
            get { return pen.DashCap; }
            set { pen.DashCap = value; } 
        }

        public DoubleCollection Dashes
        {
            get { return pen.DashStyle.Dashes; }
            set { pen.DashStyle = new DashStyle(value, 0); }
        }

        public double DashOffest
        {
            get { return pen.DashStyle.Offset; }
            set { pen.DashStyle.Offset = value; }
        }

        public PenLineCap EndLineCap
        {
            get { return pen.EndLineCap; }
            set { pen.EndLineCap = value; }
        }

        public PenLineJoin LineJoin
        {
            get { return pen.LineJoin; }
            set { pen.LineJoin = value; }
        }

        public double MiterLimit
        {
            get { return pen.MiterLimit; }
            set { pen.MiterLimit = value; }
        }

        public PenLineCap StartLineCap
        {
            get { return pen.StartLineCap; }
            set { pen.StartLineCap = value; }
        }

        public double Thickness
        {
            get { return pen.Thickness; }
            set { pen.Thickness = value; }
        }

        public object LowObject
        { 
            get { return pen; } 
        }

        private DrawingPen(Pen other)
        {
            this.pen = other.Clone();
        }

        public IPen Clone()
        {
            return new DrawingPen(pen);
        }

        public void Freeze()
        {
            if (!pen.IsFrozen)
            {
                pen.Freeze();
            }
        }
    }
}
