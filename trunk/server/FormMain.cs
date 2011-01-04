using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Runtime.InteropServices;
using HDPVRRecoder_W.helper;

namespace HDPVRRecoder_W
{
    public partial class FormMain : Form
    {
        public List<RecordTimer> timers = new List<RecordTimer>();
        private Thread namedPipeThread;
        private Thread sleepThread;
        public FormMain()
        {
            InitializeComponent();
            namedPipeThread = new Thread(new ThreadStart(delegate()
            {
                while (true)
                {
                    WindowsNamedPipe pipe = new WindowsNamedPipe("hdpvrqueue", true);
                    try
                    {                        
                        pipe._connected.WaitOne();
                        byte[] buffer = new byte[10240];
                        int count = pipe.Read(buffer);
                        pipe.Close();
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(Encoding.UTF8.GetString(buffer, 0, count));
                        this.AddNew(doc,false);
                        this.Invoke((MethodInvoker)delegate {
                            this.Show();
                        });
                        
                    }
                    catch (ThreadAbortException)
                    {
                        pipe.Close();
                        return;
                    }
                    catch (Exception ex)
                    {
                        Log.WriteLine(ex.ToString());
                        return;
                    }
                }
            }));
            namedPipeThread.Start();
            sleepThread = new Thread(new ThreadStart(delegate {
                while (true)
                {
                    try
                    {
                        checkPoint = DateTime.Now;
                        while ((DateTime.Now - checkPoint).TotalMinutes < 10)
                            Thread.Sleep(1000);
                        Log.WriteLine("sleep thread detected a loop!");
                        DateTime min = DateTime.MaxValue;
                        lock (this.timers)
                        {
                            bool recording = false;
                            foreach (RecordTimer timer in this.timers)
                            {
                                if (timer.recording)
                                {
                                    recording = true;
                                    break;
                                }
                                if (timer.from < min)
                                {
                                    min = timer.from;
                                }
                            }
                            if (recording)
                                continue;
                        }
                        if (checkBoxAutoSleep.Checked && (min - DateTime.Now).TotalMinutes > 10)
                        {
                            //休眠
                            int suspendstatus = RecordTimer.SetSuspendState(false, true, false);
                            if (suspendstatus == 0)
                                throw new Win32Exception(Marshal.GetLastWin32Error());
                            Console.WriteLine(suspendstatus);
                        }
                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                }
            }));
            sleepThread.Start();

        }
        DateTime checkPoint = DateTime.Now;
        public const int WM_POWERBROADCAST = 0x218;
        public const int PBT_APMRESUMEAUTOMATIC = 0x12;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_POWERBROADCAST && m.WParam.ToInt32() == PBT_APMRESUMEAUTOMATIC)
            {
                checkPoint = DateTime.Now;
                Log.WriteLine("wake up detected!!!!!!");
            }
            base.WndProc(ref m);
        }
        public void AddNew(XmlDocument document,bool loadfromhistory)
        {
            if (document.DocumentElement.Name == "tv-program-info" && document.DocumentElement.Attributes["version"] != null && document.DocumentElement.Attributes["version"].Value == "1.0")
            {
                foreach (XmlNode programNode in document.DocumentElement.ChildNodes)
                {
                    if (programNode.Name == "program")
                    {
                        RecordTimer timer = new RecordTimer(programNode);
                        timer.Stoped += new EventHandler<RecordTimer.RecodeStopEventArgs>(timer_Stoped);
                        timer.Started += new EventHandler<RecordTimer.RecodeStopEventArgs>(timer_Started);
                        lock (this.timers)
                        {
                            this.timers.Add(timer);
                        }
                        timer.Start();
                        treeViewFiles.Invoke((MethodInvoker)delegate {
                            treeViewFiles.Nodes.Add(timer.Key, timer.ToString());
                        });
                        if (!loadfromhistory)
                        {
                            XmlDocument old = new XmlDocument();
                            old.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"old.xml"));
                            XmlElement element = old.CreateElement("program");
                            element.InnerXml = programNode.InnerXml;
                            old.DocumentElement.AppendChild(element);
                            old.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"old.xml"));
                        }
                        
                    }
                }
            }
        }
        void timer_Started(object sender, RecordTimer.RecodeStopEventArgs e)
        {
            buttonStop.Enabled = true;
           
        }

        void timer_Stoped(object sender, RecordTimer.RecodeStopEventArgs e)
        {
            buttonStop.Enabled = false;

            lock (this.timers)
            {
                this.timers.Remove(e.timer);
            }
            this.Invoke(new MethodInvoker(
            delegate
            {
                this.treeViewFiles.Nodes.RemoveByKey(e.timer.Key);
            }));
            XmlDocument old = new XmlDocument();
            old.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"old.xml"));
            foreach (XmlNode node in old.DocumentElement.ChildNodes)
            {
                if (node.InnerXml == e.timer._node.InnerXml)
                {
                    old.DocumentElement.RemoveChild(node);
                    old.Save(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"old.xml"));
                    break;
                }
            }            
            checkPoint = DateTime.Now;
        }
        HDPVRHttpLiveStreaming hls;
        private void FormMain_Load(object sender, EventArgs e)
        {
           // HLSServer.Instance.Start();
            

            XmlDocument document = new XmlDocument();
            try
            {
                document.Load(Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"old.xml"));
                this.AddNew(document, true);
            }
            catch { }
            
        }

        private void treeViewFiles_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            lock (this.timers)
            {
                foreach (RecordTimer timer in this.timers)
                {
                    if (e.Node.Name == timer.Key)
                    {
                        textBoxStation.Text = timer.station;
                        textBoxChannel.Text = timer.rf_channel;
                        textBoxTitle.Text = timer.program_title;
                        textBoxStartTime.Text = timer.from.ToString("yyyy-MM-dd HH:mm:ss");
                        textBoxEndTime.Text = timer.to.ToString("yyyy-MM-dd HH:mm:ss");
                        textBoxDuration.Text = timer.duration.ToString();
                        textBoxDescription.Text = timer.program_description;
                        break;
                    }
                }
            }
        }

        private void FormMain_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                namedPipeThread.Abort();
                lock (this.timers)
                {
                    foreach (RecordTimer timer in this.timers)
                    {
                        if (timer.recording)
                            timer.Stop();
                    }
                    this.timers.Clear();
                }
                sleepThread.Abort();
            }
            catch { }
            Application.Exit();
        }

        private void buttonSleep_Click(object sender, EventArgs e)
        {
            //休眠
            int suspendstatus = RecordTimer.SetSuspendState(false, true, false);
            if (suspendstatus == 0)
                throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            lock (this.timers)
            {
                foreach (RecordTimer timer in this.timers)
                {
                    if (timer.recording)
                        timer.Stop();
                }
            }
        }

        private void buttonStartRoku_Click(object sender, EventArgs e)
        {
            FileHttpLiveStreaming fls = new FileHttpLiveStreaming(90, textBoxrokuRoot.Text);
            fls.Start();

            RokuFileServer roku = new RokuFileServer(89, textBoxrokuRoot.Text);
            roku.Start();
        }

        private void buttonRokuTVStreaming_Click(object sender, EventArgs e)
        {
            hls = new HDPVRHttpLiveStreaming(91);
            hls.Start();
        }

        private void buttonStopRokuTVStreaming_Click(object sender, EventArgs e)
        {
            hls.Stop();
        }

        
    }
}
