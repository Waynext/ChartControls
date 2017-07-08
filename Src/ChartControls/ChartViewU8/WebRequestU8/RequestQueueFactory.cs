using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebRequest
{
    class RequestQueueFactory
    {
        private static RequestQueue RequestQueue = new RequestQueue();
        public static RequestQueue Instance
        {
            get
            {
                return RequestQueue;
            }
        }
    }
}
