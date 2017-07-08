using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace ULog
{
    public class LogEventListener : EventListener
    {
        /// <summary>
        /// Storage file to be used to write logs
        /// </summary>
        private StorageFile m_StorageFile = null;

        /// <summary>
        /// Name of the current event listener
        /// </summary>
        private string m_Name;

        /// <summary>
        /// The format to be used by logging.
        /// </summary>
        private string m_Format = "{0:yyyy-MM-dd HH\\:mm\\:ss\\:ffff}\t{1}\t{2}";

        private SemaphoreSlim m_SemaphoreSlim = new SemaphoreSlim(1);

        public LogEventListener(string name)
        {
            this.m_Name = name;

            Debug.WriteLine("StorageFileEventListener for {0} has name {1}", GetHashCode(), name);

            AssignLocalFile();
        }

        private async void AssignLocalFile()
        {
            m_StorageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(m_Name.Replace(" ", "_") + ".log",
                                                                                      CreationCollisionOption.OpenIfExists);
        }

        private async void WriteToFile(IEnumerable<string> lines)
        {
            await m_SemaphoreSlim.WaitAsync();

            await Task.Run(async () =>
                                     {
                                         try
                                         {
                                             await FileIO.AppendLinesAsync(m_StorageFile, lines);
                                         }
                                         catch (Exception ex)
                                         {
                                             // TODO:
                                         }
                                         finally
                                         {
                                             m_SemaphoreSlim.Release();
                                         }
                                     });
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            if (m_StorageFile == null) return;

            if (eventData.EventId == 0)
                return;

            var lines = new List<string>();

            string newFormatedLine = string.Format(m_Format, DateTime.Now, eventData.Level, eventData.Payload[0]);
            
            Debug.WriteLine(newFormatedLine);

            lines.Add(newFormatedLine);

            WriteToFile(lines);
        }
        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Debug.WriteLine("OnEventSourceCreated for Listener {0} - {1} got eventSource {2}", GetHashCode(), m_Name, eventSource.Name);
        }
    }
}
