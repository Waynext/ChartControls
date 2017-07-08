using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRequest.Requests.StockData
{
    class Error
    {
        public int Code
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }
}
