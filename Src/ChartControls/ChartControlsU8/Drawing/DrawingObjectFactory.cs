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
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ChartControls.Drawing
{
    /// <summary>
    /// 创建画图对象， 比如画笔、文本。在创建ChartItemCollection时需要用到。
    /// </summary>
    public sealed class DrawingObjectFactory
    {
        /// <summary>
        /// 创建画笔。
        /// </summary>
        /// <param name="brush"></param>
        /// <param name="thickness"></param>
        /// <returns></returns>
        public static IPen CreatePen(Brush brush, double thickness)
        {
            var pen = new CanvasPen(brush, thickness);
            //pen.Freeze();
            return pen;
        }

        /// <summary>
        /// 创建文本。
        /// </summary>
        /// <param name="textToFormat"></param>
        /// <param name="flowDirection"></param>
        /// <param name="fontFamily"></param>
        /// <param name="style"></param>
        /// <param name="weight"></param>
        /// <param name="stretch"></param>
        /// <param name="emSize"></param>
        /// <param name="foreground"></param>
        /// <returns></returns>
        public static ITextFormat CreateTextFormat(string textToFormat, FlowDirection flowDirection, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, double emSize, Brush foreground)
        {
            return new CanvasTextFormat(textToFormat)
            {
                FlowDirection = flowDirection,
                FontFamily = fontFamily,
                Style = style,
                Weight = weight,
                Stretch = stretch,
                FontSize = emSize,
                Foreground = foreground
            };
        }

        /// <summary>
        /// 创建文本。
        /// </summary>
        /// <param name="textBlock"></param>
        /// <param name="flowDirection"></param>
        /// <param name="fontFamily"></param>
        /// <param name="style"></param>
        /// <param name="weight"></param>
        /// <param name="stretch"></param>
        /// <param name="emSize"></param>
        /// <param name="foreground"></param>
        /// <returns></returns>
        public static ITextFormat CreateTextFormat(TextBlock textBlock, FlowDirection flowDirection, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, double emSize, Brush foreground)
        {
            return new CanvasTextFormat(textBlock)
            {
                FlowDirection = flowDirection,
                FontFamily = fontFamily,
                Style = style,
                Weight = weight,
                Stretch = stretch,
                FontSize = emSize,
                Foreground = foreground
            };
        }
    }
}
