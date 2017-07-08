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

#if USINGCANVAS
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.Foundation;
#else
using System.Windows.Media;
#endif

namespace ChartControls.Drawing
{
    /// <summary>
    /// 画笔接口，属性定义与WPF的Pen类似
    /// </summary>
    public interface IPen
    {
        Brush Brush { get; set; }
        PenLineCap DashCap { get; set; }
        DoubleCollection Dashes { get; set; }
        double DashOffest { get; set; }
        PenLineCap EndLineCap { get; set; }
        PenLineJoin LineJoin { get; set; }
        double MiterLimit { get; set; }
        PenLineCap StartLineCap { get; set; }
        double Thickness { get; set; }

        /// <summary>
        /// 取得底层对象，如果依赖WPF，就返回Pen对象
        /// </summary>
        object LowObject { get; }

        /// <summary>
        /// 克隆对象
        /// </summary>
        /// <returns>画笔</returns>
        IPen Clone();
    }

    /// <summary>
    /// 文本接口, 属性定义与WPF的FormattedText类似
    /// </summary>
    public interface ITextFormat
    {
        string Content { get; }
        FontFamily FontFamily { get; }
        FontStretch Stretch { get; }
        FontStyle Style { get; }
        FontWeight Weight { get; }
        double FontSize { get; }
        Brush Foreground { get; }
        TextAlignment TextAlignment { get; set; }
        TextTrimming TextTrimming { get; set; }
        FlowDirection FlowDirection { get; set; }
        double Width { get; }
        double Height { get; }

        /// <summary>
        /// 取得底层对象，如果依赖WPF，就返回FormattedText对象
        /// </summary>
        object LowObject { get; }
    }

    /// <summary>
    /// 画图上下文接口，提供画图函数。
    /// </summary>
    public interface IDrawingContext : IDisposable
    {
        /// <summary>
        /// 画椭圆
        /// </summary>
        void DrawEllipse(Brush brush, IPen pen, Point center, double radiusX, double radiusY);
        /// <summary>
        /// 画自定义几何图形
        /// </summary>
        void DrawGeometry(Brush brush, IPen pen, Geometry geometry);
        /// <summary>
        /// 画图像
        /// </summary>
        void DrawImage(ImageSource imageSource, Rect rectangle);
        /// <summary>
        /// 画直线
        /// </summary>
        void DrawLine(IPen pen, Point point0, Point point1);
        /// <summary>
        /// 画矩形
        /// </summary>
        void DrawRectangle(Brush brush, IPen pen, Rect rectangle);
        /// <summary>
        /// 画圆角矩形
        /// </summary>
        void DrawRoundedRectangle(Brush brush, IPen pen, Rect rectangle, double radiusX, double radiusY);
        /// <summary>
        /// 画文本
        /// </summary>
        void DrawText(ITextFormat formattedText, Point origin);
        /// <summary>
        /// 弹出最后压入的效果
        /// </summary>
        void Pop();
        /// <summary>
        /// 压入剪切区域
        /// </summary>
        void PushClip(Geometry clipGeometry);
        /// <summary>
        /// 压入透明值效果
        /// </summary>
        void PushOpacity(double opacity);
        /// <summary>
        /// 压入转换效果
        /// </summary>
        void PushTransform(Transform transform);

        /// <summary>
        /// 取得底层上下文， 如果依赖WPF， 返回DrawingContext对象。否则返回Canvas对象
        /// </summary>
        object LowContext { get; }
        /// <summary>
        /// 返回最后画的对象
        /// </summary>
        object LastDrawnObject { get; }
    }
}
