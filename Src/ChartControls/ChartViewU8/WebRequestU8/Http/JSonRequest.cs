using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Net;
using System.Diagnostics;
using ULog;
using WebRequest.Common;

namespace WebRequest.Http
{
    class JSonRequest : HttpRequest
    {
        public JSonRequest(string uri, JObject jObject, string method = HttpConst.HttpMethod_Post)
            : base(uri, method)
        {
#if DEBUG
            Object = jObject;
#endif
            DiagnoseHelper.CheckArgument(jObject, "jObject can not be null");
            Serialize2Stream(jObject);
        }

#if DEBUG
        public JObject Object { get; private set; }
#endif
        private void Serialize2Stream(JObject jObject)
        {
            string jsonText = jObject.ToString();
            MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(jsonText));
            stream.Seek(0, SeekOrigin.Begin);

            SetRequestStream(stream, HttpLayer.JSonContentType);
        }
    }

    class ContentTypeEntity
    {
        public ContentTypeEntity(string rawContentType)
        {
            var cts = rawContentType.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            ContentType = cts[0];
            Encoding = Encoding.UTF8;
        }
        public Encoding Encoding { get; set; }
        public string ContentType { get; set; }
    }

    class JSonResponse : HttpResponse
    {
        public JSonResponse(HttpWebResponse response, int requestId)
            : base(response, requestId)
        {
            //DeseralizeFromStream();
        }

        public JObject Object
        {
            get;
            private set;
        }

        internal override void DeseralizeFromStream()
        {
            DiagnoseHelper.CheckReference(ResponseStream, "Can not get response stream");

            ResponseStream.Seek(0, SeekOrigin.Begin);

            ContentTypeEntity ct = new ContentTypeEntity(OriginalResponse.ContentType);

            int contentLen = (int)OriginalResponse.ContentLength;
            //DiagnoseHelper.CheckStringIgnoreCase(ct.ContentType, HttpLayer.JSonContentType, "Login response content type is not correct");
            byte[] buffer = null;
            if (contentLen != -1)
            {
                buffer = new byte[contentLen];

                int leftLen = contentLen;
                
                while (leftLen != 0)
                {
                    int readLen = ResponseStream.Read(buffer, 0, leftLen);
                    leftLen -= readLen;

                    if (leftLen != 0)
                    {
                        LogHelper.OnlineLogger.Debug("ToDo:: can not read all data in once, need to sleep!" + HttpLayer.LogRequetId(ResponseId));
                    }
                }
            }
            else
            {
                List<byte> buffList = new List<byte>();
                int tempBufLen = 10 * 1024;
                byte[] bufTemp = new byte[tempBufLen];

                int readLen = 0;
                while ((readLen = ResponseStream.Read(bufTemp, 0, tempBufLen)) != 0)
                {
                    if (readLen == tempBufLen)
                        buffList.AddRange(bufTemp);
                    else
                        buffList.AddRange(bufTemp.Take(readLen));
                }

                buffer = buffList.ToArray();

                contentLen = buffList.Count;
            }

            string sResult = ct.Encoding.GetString(buffer, 0, contentLen);
            DiagnoseHelper.CheckString(sResult, "Login response content is empty");

            this.Object = JsonConvert.DeserializeObject<JObject>(sResult);
            
        }
    }
}
