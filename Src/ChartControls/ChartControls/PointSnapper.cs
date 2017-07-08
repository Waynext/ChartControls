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


#if USINGCANVAS
using Windows.Foundation;
#else
using System.Windows;
#endif

namespace ChartControls
{
    /// <summary>
    /// 坐标微调器。
    /// </summary>
    public sealed class PointSnapper
    {
        /// <summary>
        /// 微调坐标值。
        /// </summary>
        public static double SnapValue(double p)
        {
            return (int)(p) + 0.5;
        }

        /// <summary>
        /// 取整坐标值。
        /// </summary>
        public static double RoundValue(double p)
        {
            return Math.Round(p);
        }

        /// <summary>
        /// 微调坐标点。
        /// </summary>
        public static Point SnapPoint(Point pt)
        {
            pt.X = SnapValue(pt.X);
            pt.Y = SnapValue(pt.Y);

            return pt;
        }

        /// <summary>
        /// 取整坐标点。
        /// </summary>
        public static Point RoundPoint(Point pt)
        {
            pt.X = RoundValue(pt.X);
            pt.Y = RoundValue(pt.Y);

            return pt;
        }
    }
}
