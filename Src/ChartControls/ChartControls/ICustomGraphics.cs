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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if USINGCANVAS
using Windows.Foundation;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// 自定义图形接口。<see cref="ChartControl.StartDrawCustomGraphics(ICustomGraphics)"/>
    /// </summary>
    public interface ICustomGraphics
    {
        /// <summary>
        /// 坐标点的数量。
        /// </summary>
        int PointCount
        {
            get;
            set;
        }

        /// <summary>
        /// 坐标点数组。
        /// </summary>
        Point[] Points
        {
            get;
            set;
        }

        /// <summary>
        /// 时间和值组合数组。
        /// </summary>
        ValuePoint[] ValuePoints
        {
            get;
            set;
        }

        /// <summary>
        /// 开始绘制第一个点。
        /// </summary>
        /// <param name="pt">坐标。</param>
        void StartDraw(Point pt);

        /// <summary>
        /// 绘制下一个点。
        /// </summary>
        /// <param name="pt">坐标。</param>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        /// <param name="isFinal">是否是最后一个点。</param>
        /// <returns>是否绘制完成。</returns>
        bool NextDraw(Point pt, IDrawingContext dc, bool isFinal);

        /// <summary>
        /// 绘制整个自定义图形。
        /// </summary>
        /// <param name="dc">绘制上下文。<see cref="IDrawingContext"/></param>
        void Draw(IDrawingContext dc);
    }

    /// <summary>
    /// 值时间组合，用于坐标点的对应，实现自定义图形的持久化和恢复。
    /// </summary>
    public struct ValuePoint
    {
        /// <summary>
        /// 值。
        /// </summary>
        public double Value
        {
            get;
            set;
        }

        /// <summary>
        /// 时间。
        /// </summary>
        public DateTime Date
        {
            get;
            set;
        }

        /// <summary>
        /// 时间轴偏移百分比。
        /// </summary>
        public double Deviation
        {
            get;
            set;
        }
    }

    /// <summary>
    /// 坐标与时间值组合转化接口。
    /// </summary>
    public interface IPoint2ValuePoint
    {
        /// <summary>
        /// 坐标转值时间组合。
        /// </summary>
        ValuePoint ConvertFromPoint(Point pt);
        /// <summary>
        /// 值时间组合转坐标。
        /// </summary>
        Point ConvertFromValuePoint(ValuePoint vp);
    }
}
