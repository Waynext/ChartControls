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
using Windows.UI.Xaml.Media;

namespace ChartControls.Drawing
{
    class CanvasPen : IPen
    {
        public CanvasPen(Brush stroke, double thickness)
        {
            Brush = stroke;
            Thickness = thickness;
        }

        public Brush Brush { get; set; }
        public PenLineCap DashCap { get; set; }
        public DoubleCollection Dashes { get; set; }
        public double DashOffest { get; set; }
        public PenLineCap EndLineCap { get; set; }
        public PenLineJoin LineJoin { get; set; }
        public double MiterLimit { get; set; }
        public PenLineCap StartLineCap { get; set; }
        public double Thickness { get; set; }

        public object LowObject { get; private set; }

        public IPen Clone()
        {
            return new CanvasPen(this.Brush, this.Thickness){
                DashCap = this.DashCap,
                Dashes = this.Dashes,
                DashOffest = this.DashOffest,
                EndLineCap = this.EndLineCap,
                LineJoin = this.LineJoin,
                MiterLimit = this.MiterLimit,
                StartLineCap = this.StartLineCap,
            };
        }
    }
}
