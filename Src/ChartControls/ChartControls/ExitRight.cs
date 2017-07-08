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

namespace ChartControls
{
    public class ExitRight
    {
        public ExitRight() 
        {
            Dividen = BonusRate = RationedRate = RationedPrice = ChartItemCollection.valueNA;
        }
        public double Dividen { get; set; }
        public double BonusRate { get; set; }
        public double RationedRate { get; set; }
        public double RationedPrice { get; set; }

        public override string ToString()
        {
            StringBuilder text = new StringBuilder();

            if (Dividen != double.NaN)
            {
                text.AppendFormat("D:{0:F2}", Dividen);
            }

            double rate = ChartItemCollection.valueNA;
            if (BonusRate != ChartItemCollection.valueNA)
            {
                rate = BonusRate;
            }
            if (RationedRate != ChartItemCollection.valueNA)
            {
                if (rate != ChartItemCollection.valueNA)
                {
                    rate += RationedRate;
                }
                else
                {
                    rate = RationedRate;
                }
            }

            if (rate != ChartItemCollection.valueNA)
            {
                if (text.Length != 0)
                {
                    text.AppendLine();
                }
                text.AppendFormat("S:{0:F2}", rate);
            }

            return text.ToString();
        }
    }
}
