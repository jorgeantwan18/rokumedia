using System;
using System.Collections.Generic;

using System.Text;
using System.IO;

namespace HDPVRRecoder_W
{
    public class Log
    {
        public class LogEventArgs:EventArgs {
            public string log;
            public LogEventArgs(string log)
            {
                this.log = log;
            }
        }
        public event EventHandler<LogEventArgs> OnLog;

        public static readonly Log Instance = new Log();
        private Log() { }

        public static void WriteLine(string msg)
        {
            try
            {
                lock (typeof(Log))
                {
                    StreamWriter sw = new StreamWriter(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "my.log"),true,Encoding.UTF8);
                    sw.WriteLine("{0}:{1}", DateTime.Now.ToString("HH:mm:ss.fff"), msg);
                    sw.Close();
                }
                Console.WriteLine(msg);

                if (Log.Instance.OnLog != null)
                    Log.Instance.OnLog(null, new LogEventArgs(msg));
            }
            catch { }
        }
    }
}
