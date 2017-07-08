using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace WebRequest.Common
{
    class StreamCollection : IDisposable
    {
        public StreamCollection(Stream stream)
        {
            Streams = new List<Stream>(1);
            Streams.Add(stream);
        }

        public StreamCollection(IEnumerable<Stream> streams)
        {
            Streams = streams.ToList();
        }

        public List<Stream> Streams
        {
            get;
            private set;
        }

        public bool CanSeek { 
            get 
            {
                return Streams.All(s => s.CanSeek);
            } 
        }

        public long Length
        {
            get
            {
                return Streams.Sum(s => s.Length);
            }
        }

        private int iCurrentStream = 0;

        public long Seek(long offset)
        {
            if (Streams.Count == 1)
            {
                return Streams[0].Seek(offset, SeekOrigin.Begin);
            }
            else
            {
                long length = 0;
                for (int i = 0; i < Streams.Count; i++)
                {
                    var tempLength = length + Streams[i].Length;
                    if (offset < tempLength)
                    {
                        iCurrentStream = i;
                        Streams[i].Seek(offset - length, SeekOrigin.Begin);
                        return offset;
                    }

                    length = tempLength;
                }

                throw new IOException("Cannot seek");

            }
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            if (Streams.Count == 1)
            {
                return Streams[0].Read(buffer, offset, count);
            }
            else
            {
                int readCount = 0;
                while (true)
                {
                    var iRead = Streams[iCurrentStream].Read(buffer, offset, count);
                    readCount += iRead;

                    if (iRead < count)
                    {
                        if (iCurrentStream < Streams.Count - 1)
                        {
                            iCurrentStream++;
                            Streams[iCurrentStream].Seek(0, SeekOrigin.Begin);
                            offset += iRead;
                            count -= iRead;

                            continue;
                        }
                        
                    }

                    break;
                }

                return readCount;
            }
        }

        public void Close()
        {
            foreach (var s in Streams)
            {
                s.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var s in Streams)
            {
                s.Dispose();
            }
        }
    }
}
