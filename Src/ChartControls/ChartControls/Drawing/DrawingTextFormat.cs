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
using System.Globalization;

namespace ChartControls.Drawing
{
    class DrawingTextFormat : ITextFormat
    {
        private FormattedText formatedText;
        private Typeface typeface;
        public DrawingTextFormat(string textToFormat, FontFamily fontFamily, double fontSize, Brush foreground)
        {
            Init(textToFormat, FlowDirection.LeftToRight, fontFamily, FontStyles.Normal, FontWeights.Normal, FontStretches.Normal, fontSize, foreground);
        }

        public DrawingTextFormat(string textToFormat, FlowDirection flowDirection, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, double emSize, Brush foreground)
        {
            Init(textToFormat, flowDirection, fontFamily, style, weight, stretch, emSize, foreground);
        }

        private void Init(string textToFormat, FlowDirection flowDirection, FontFamily fontFamily, FontStyle style, FontWeight weight, FontStretch stretch, double emSize, Brush foreground)
        {
            typeface = new Typeface(fontFamily, style, weight, stretch);
            formatedText = new FormattedText(textToFormat, CultureInfo.CurrentUICulture, flowDirection, typeface, emSize, foreground);
            FontSize = emSize;
            Foreground = foreground;
        }

        public string Content {
            get
            {
                return formatedText.Text;
            }

        }
        public FontFamily FontFamily {
            get
            {
                return typeface.FontFamily;
            }
        }

        public FontStretch Stretch {
            get
            {
                return typeface.Stretch;
            }
        }

        public FontStyle Style {
            get
            {
                return typeface.Style;
            }
        }

        public FontWeight Weight {
            get
            {
                return typeface.Weight;
            }
        }

        public double FontSize {
            get;
            private set;
        }

        public Brush Foreground 
        {
            get;
            private set;
        }

        public TextAlignment TextAlignment {
            get
            {
                return formatedText.TextAlignment;
            }
            set
            {
                formatedText.TextAlignment = value;
            }
        }
        public TextTrimming TextTrimming
        {
            get
            {
                return formatedText.Trimming;
            }
            set
            {
                formatedText.Trimming = value;
            }
        }

        public FlowDirection FlowDirection
        {
            get
            {
                return formatedText.FlowDirection;
            }
            set
            {
                formatedText.FlowDirection = value;
            }
        }

        public double Width {
            get
            {
                return formatedText.Width;
            }
        }

        public double Height
        {
            get
            {
                return formatedText.Height;
            }
        }

        public object LowObject { 
            get {
                return formatedText;
            } 
        }
    }
}
