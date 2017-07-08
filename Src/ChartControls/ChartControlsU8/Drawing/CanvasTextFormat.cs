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
using Windows.Foundation;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace ChartControls.Drawing
{
    class CanvasTextFormat : ITextFormat
    {
        private TextBlock textBlock;
        private bool isMeasured = false;
        private static Size textSize = new Size(double.PositiveInfinity, double.PositiveInfinity);
        public CanvasTextFormat(string content)
        {
            textBlock = new TextBlock();
            Content = content;
        }

        public CanvasTextFormat(TextBlock content)
        {
            textBlock = content;
        }

        public string Content 
        {
            get
            {
                return textBlock.Text;
            }
            set 
            {
                textBlock.Text = value;
                isMeasured = false;
            }
        }

        public FontFamily FontFamily
        {
            get
            {
                return textBlock.FontFamily;
            }
            set
            {
                textBlock.FontFamily = value;
                isMeasured = false;
            }
        }

        public FontStretch Stretch
        {
            get
            {
                return textBlock.FontStretch;
            }
            set
            {
                textBlock.FontStretch = value;
                isMeasured = false;
            }
        }

        public FontStyle Style
        {
            get
            {
                return textBlock.FontStyle;
            }
            set
            {
                textBlock.FontStyle = value;
                isMeasured = false;
            }
        }

        public FontWeight Weight
        {
            get
            {
                return textBlock.FontWeight;
            }
            set
            {
                textBlock.FontWeight = value;
                isMeasured = false;
            }
        }

        public double FontSize
        {
            get
            {
                return textBlock.FontSize;
            }
            set
            {
                textBlock.FontSize = value;
                isMeasured = false;
            }
        }

        public Brush Foreground
        {
            get
            {
                return textBlock.Foreground;
            }
            set
            {
                textBlock.Foreground = value;
                isMeasured = false;
            }
        }

        public TextAlignment TextAlignment
        {
            get
            {
                return textBlock.TextAlignment;
            }
            set
            {
                textBlock.TextAlignment = value;
                isMeasured = false;
            }
        }

        public TextTrimming TextTrimming
        {
            get
            {
                return textBlock.TextTrimming;
            }
            set
            {
                textBlock.TextTrimming = value;
                isMeasured = false;
            }
        }

        public double Width {
            get
            {
                if (!isMeasured)
                {
                    textBlock.Measure(textSize);
                    isMeasured = true;
                }

                return textBlock.ActualWidth;

            }
        }
        public double Height {
            get
            {
                if (!isMeasured)
                {
                    textBlock.Measure(textSize);
                    isMeasured = true;
                }

                return textBlock.ActualHeight;

            }
        }

        public FlowDirection FlowDirection
        {
            get { return textBlock.FlowDirection; }
            set {
                textBlock.FlowDirection = value;
                isMeasured = false;
            }
        }
        public object LowObject
        {
            get
            {
                return textBlock;
            }
        }
    }
}
