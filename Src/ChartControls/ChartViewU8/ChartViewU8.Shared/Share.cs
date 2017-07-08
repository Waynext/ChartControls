using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
    public class SimpleShare
    {
        public string StockId
        {
            get;
            set;
        }

        public IList<DateTime> Dates
        {
            get;
            set;
        }

        public IList<double> Closes
        {
            get;
            set;
        }

        public string Conext
        {
            get;
            set;
        }
    }

    public class SimpleShareVolumn : SimpleShare
    {
        public IList<double> Turnovers
        {
            get;
            set;
        }

        public IList<double> Volumns
        {
            get;
            set;
        }

        public bool IsRaise
        {
            get;
            set;
        }
    }

    public class CompleteShare : SimpleShareVolumn
    {
        public IList<double> Highs
        {
            get;
            set;
        }

        public IList<double> Lows
        {
            get;
            set;
        }

        public IList<double> Opens
        {
            get;
            set;
        }
    }
}
