using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using System.Threading;
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using HDPVRRecoder_W.helper;

namespace HDPVRRecoder_W
{
    

    public class RecordTimer
    {
        public class RecodeStopEventArgs:EventArgs
        {
            public RecordTimer timer;
            public RecodeStopEventArgs(RecordTimer e)
            {
                this.timer = e;
            }
        }
        public event EventHandler<RecodeStopEventArgs> Stoped;
        public event EventHandler<RecodeStopEventArgs> Started;
        [DllImport("PowrProf", SetLastError = true, ExactSpelling = true)]
        public static extern Int32 SetSuspendState(bool fSuspend, bool forceCritical, bool disableWakeEvent);

        public delegate void VoidHandler();
        [DllImport("kernel32.dll")]
        public static extern SafeWaitHandle CreateWaitableTimer(IntPtr lpTimerAttributes, bool bManualReset, string lpTimerName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWaitableTimer(SafeWaitHandle hTimer, [In] ref long pDueTime, int lPeriod, IntPtr pfnCompletionRoutine, IntPtr lpArgToCompletionRoutine, bool fResume);

        public DateTime from;
        public DateTime to;

        public bool recording = false;

        private Thread thread;
        public XmlNode _node;
        public static readonly string rootPath = @"Z:\";
        public string station = "";
        public string program_title = "";
        public string episode_title = "";
        public string program_description;
        public string start_date = "";
        public string start_time = "";
        public string end_date = "";
        public string end_time = "";
        public string rf_channel = "";
        private string filename = "";
        public int duration;
        public string Key
        {
            get
            {
                return guid;
            }
        }
        private string guid;
        public override string ToString()
        {
            return this.station + " " + this.from.ToString("yyyyMMddHHmmss");
        }
        FileStream fs;
                                
        public RecordTimer(XmlNode node)
        {
            this.guid = Guid.NewGuid().ToString("N");
            this._node = node;
            this.DealXmlDocument(node);
            
            this.thread = new Thread(new ThreadStart(delegate(){
                
        }));
        }
        public void Start()
        {
            this.thread = new Thread(new ThreadStart(Run));
            this.thread.Start();
        }
        public void Stop()
        {
            HDPVR.Instance.RemoveOutputStream(fs);
            HDPVR.Instance.Stop();
            this.thread.Abort();
        }
        public void Run()
        {
            try
            {
                using (SafeWaitHandle handle = CreateWaitableTimer(IntPtr.Zero, true, Thread.CurrentThread.Name))
                {
                    long duetime = this.from.ToFileTime();
                    Log.WriteLine("start timer,due at" + this.from.ToString());
                    if (SetWaitableTimer(handle, ref duetime, 0, IntPtr.Zero, IntPtr.Zero, true))
                    {
                        Log.WriteLine("timer started");
                        using (EventWaitHandle wh = new EventWaitHandle(false, EventResetMode.AutoReset))
                        {
                            wh.SafeWaitHandle = handle;
                            wh.WaitOne();
                            Log.WriteLine("timer due");
                            if (this.to < DateTime.Now)
                            {
                                Log.WriteLine("to:" + this.to.ToString());
                                if (this.Stoped != null)
                                    this.Stoped(this, new RecodeStopEventArgs(this));
                                return;
                            }
                            Log.WriteLine("begin recording");
                            this.fs = new FileStream(this.filename, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                            TimeSpan onesecond = new TimeSpan(0, 0, 1);
                            TimeSpan zerospan = new TimeSpan(1);
                            Log.WriteLine("from:" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + ",to:" + to.ToString("yyyy-MM-dd HH:mm:ss"));
                            this.recording = true;
                            if (this.Started != null)
                                this.Started(this, new RecodeStopEventArgs(this));
                            HDPVR.Instance.AddOutputStream(fs);
                            HDPVR.Instance.ChangeChannel(rf_channel);
                            HDPVR.Instance.Start();

                            while (to > DateTime.Now)
                                Thread.Sleep(to - DateTime.Now > onesecond ? onesecond : to - DateTime.Now);
                            HDPVR.Instance.RemoveOutputStream(fs);
                            fs.Close();
                            if (this.Stoped != null)
                                this.Stoped(this, new RecodeStopEventArgs(this));
                            this.recording = false;
                        }
                    }
                    else
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error());
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    HDPVR.Instance.RemoveOutputStream(fs);
                    fs.Close();
                }
                catch { }
                Log.WriteLine(ex.ToString());
            }
        }
        public void DealXmlDocument(XmlNode node)
        {
            foreach (XmlNode attributenode in node.ChildNodes)
            {
                if (attributenode.Name == "station")
                {
                    this.station = attributenode.InnerText;
                }
                else if (attributenode.Name == "program-title")
                {
                    this.program_title = attributenode.InnerText;
                }
                else if (attributenode.Name == "episode-title")
                {
                    this.episode_title = attributenode.InnerText;
                }
                else if (attributenode.Name == "start-date")
                {
                    this.start_date = attributenode.InnerText;
                }
                else if (attributenode.Name == "start-time")
                {
                    this.start_time = attributenode.InnerText;
                }
                else if (attributenode.Name == "end-date")
                {
                    this.end_date = attributenode.InnerText;
                }
                else if (attributenode.Name == "end-time")
                {
                    this.end_time = attributenode.InnerText;
                }
                else if (attributenode.Name == "rf-channel")
                {
                    this.rf_channel = attributenode.InnerText;
                }
                else if (attributenode.Name == "program-description")
                {
                    this.program_description = attributenode.InnerText;
                }
            }
            this.from = DateTime.ParseExact(start_date + start_time, "yyyyMMddHH:mm", null);
            this.from = DateTime.SpecifyKind(from, DateTimeKind.Utc).ToLocalTime();
            this.to = DateTime.ParseExact(end_date + end_time, "yyyyMMddHH:mm", null);
            this.to = DateTime.SpecifyKind(to, DateTimeKind.Utc).ToLocalTime();
            
            this.filename = Regex.Replace(Regex.Replace(station + "_" + from.ToString("yyyyMMddHHmmss") + "_" + to.ToString("yyyyMMddHHmmss") + "_" + program_title + "-" + episode_title + ".ts", "[\\/\\:\\*\\?\"\\<\\>\\|]", ""), "[/\\\\\\|\\*\\?:\"\\<\\>]", new MatchEvaluator(delegate(Match m) { return ""; }));
            this.filename = Path.Combine(rootPath, this.filename);
            Log.WriteLine("now to:" + to.ToString("yyyy-MM-dd HH:mm:ss"));
        }

    }
}
