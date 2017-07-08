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

#if USINGCANVAS
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
#else
using System.Windows.Media;
#endif

namespace ChartControls
{
    /// <summary>
    /// y轴刻度位置。
    /// </summary>
    public enum YScaleDock {
        /// <summary>
        /// 图表左侧。
        /// </summary>
        Left,
        /// <summary>
        /// 图表右侧。
        /// </summary>
        Right,
        /// <summary>
        /// 图表左内侧。
        /// </summary>
        InnerLeft,
        /// <summary>
        /// 图表右内侧。
        /// </summary>
        InnerRight,
        /// <summary>
        /// 不显示。
        /// </summary>
        None
    };

    /// <summary>
    /// x轴刻度位置。
    /// </summary>
    public enum XScaleDock {
        /// <summary>
        /// 不显示。
        /// </summary>
        None,
        /// <summary>
        /// 底部。
        /// </summary>
        Bottom
    }

    /// <summary>
    /// 坐标类型。
    /// </summary>
    public enum CoordinateType
    {
        /// <summary>
        /// 线性坐标。
        /// </summary>
        Linear,
        /// <summary>
        /// 对数坐标。
        /// </summary>
        Log10,
        /// <summary>
        /// 百分比坐标。
        /// </summary>
        Percentage
    };

    /// <summary>
    /// 接触操作类型。
    /// </summary>
    public enum PointerAction
    {
        /// <summary>
        /// 无。
        /// </summary>
        None,
        /// <summary>
        /// 放大。
        /// </summary>
        ZoomIn,
        /// <summary>
        /// 测量。
        /// </summary>
        Measure,
        /// <summary>
        /// 选择。
        /// </summary>
        Select
    };

    /// <summary>
    /// 额外数据索引名字。
    /// </summary>
    public sealed class ExtraDataNames
    {
        /// <summary>
        /// 除权及分红数据名字。
        /// </summary>
        public const string XRName = "ExitRight";
        /// <summary>
        /// 消息名字。
        /// </summary>
        public const string MessageName = "News";
    }

    sealed class ConstStrings
    {
        public const string DateTimeFormat = "yyyyMMdd";
    }

    class FontConst
    {
        public static readonly FontFamily DefaultFontFamily = new FontFamily("Segoe UI");
        public const double DefaultFontSize = 10;
    }

#if USINGCANVAS
    class Brushes
    {
        public static readonly SolidColorBrush Black = new SolidColorBrush(Colors.Black);
        public static readonly SolidColorBrush White = new SolidColorBrush(Colors.White);
        public static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush Gray = new SolidColorBrush(Colors.Gray);
        public static readonly SolidColorBrush Goldenrod = new SolidColorBrush(Colors.Goldenrod);
        public static readonly SolidColorBrush Silver = new SolidColorBrush(Colors.Silver);
    }

    class FontStretches
    {
        public const FontStretch Normal = FontStretch.Normal;
    }

    class FontStyles
    {
        public const FontStyle Normal = FontStyle.Normal;
        public const FontStyle Oblique = FontStyle.Oblique;
        public const FontStyle Italic = FontStyle.Italic;
    }
#endif
}
