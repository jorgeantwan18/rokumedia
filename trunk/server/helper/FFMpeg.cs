using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Text.RegularExpressions;
using HDPVRRecoder_W.ts;

namespace HDPVRRecoder_W.helper
{
    public class FFMpeg:BaseHelper
    {
        public const int CACHELENGTH = 9;
        public const int TOTALCACHEFILES = 10;
        public bool transcodefinished = false;
        private bool paused = false;
        private BaseHelper _input;
        private Process ffmpeg;
        
        public AutoResetEvent IntervalEclipse;
        public decimal currentTime;

        public string Sid;
        public List<MemoryStream> cachedVideo = new List<MemoryStream>();
        public int fileSequence = 0;
        public DateTime _lastActiveTime = DateTime.Now;


        public FFMpeg(BaseHelper input,string sid)
        {
            this._input = input;
            this.Sid = sid;
            
            this.IntervalEclipse = new AutoResetEvent(false);
        }
        public override void Start()
        {
            base.Start();
        }
        public override void Stop()
        {
            if (this._input is ISupportAddOutputStream)
            {
                try
                {
                    ((ISupportAddOutputStream)this._input).RemoveOutputStream(ffmpeg.StandardInput.BaseStream);
                }
                catch { }
            }
            this._input.Stop();

            try
            {
                Log.WriteLine("stopping ffmpeg!!!!!!!!!!!!");
                ffmpeg.Kill();
            }
            catch { }           

            lock (cachedVideo)
            { 
                cachedVideo.ForEach(delegate(MemoryStream ms){
                    ms.Close();
                });
                cachedVideo.Clear();
            }
            base.Stop();
        }
        public void Pause()
        {
            this.paused = true;
        }
        public void Restart()
        {
            this.paused = false;
        }
        protected override void Run()
        {
            Log.WriteLine("ffmpeg 正在启动");
            ffmpeg = new Process();
            ffmpeg.StartInfo.FileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
            ffmpeg.StartInfo.Arguments = " -threads 6 " + (this._input is IFFMpegParameters ? ((IFFMpegParameters)this._input).GetSize() : "") + " -i " + (this._input is IFileStream ? "\"" + ((IFileStream)this._input).GetFileName() + "\"" : "-") + (this._input is IFFMpegParameters ? ((IFFMpegParameters)this._input).GetFFMpegParameters():"") + " -f mpegts -";
            Log.WriteLine("ffmpeg 参数=" + ffmpeg.StartInfo.Arguments);
            ffmpeg.StartInfo.RedirectStandardError = true;
            ffmpeg.StartInfo.RedirectStandardOutput = true;
            ffmpeg.StartInfo.RedirectStandardInput = true;
            ffmpeg.StartInfo.UseShellExecute = false;
            ffmpeg.StartInfo.CreateNoWindow = true;
            ffmpeg.Start();
            new Thread(new ThreadStart(delegate
            {
                Regex regex = new Regex("time=(?<time>[0-9\\.]*)");
                string error = "";
                while ((error = ffmpeg.StandardError.ReadLine()) != null)
                {
                    Match m = regex.Match(error);
                    if (m.Success)
                    {
                        this.currentTime = decimal.Parse(m.Groups["time"].Value.Trim());
                        this.IntervalEclipse.Set();
                        Log.WriteLine("time eclipse,currentTime=" + this.currentTime);
                    }
                    if (error.StartsWith("video:"))
                    {
                        Log.WriteLine("transcode finished.");
                        this.transcodefinished = true;
                    }
                    Log.WriteLine(error);
                }
                return;
            })).Start();
            //读取并缓存线程
            new Thread(new ThreadStart(delegate
            {
                byte[] buffer = new byte[188];
                int count = 0;
                int readcount = 0;
                this.cachedVideo.Add(new MemoryStream());
                decimal seconds = 0;
                DateTime startTime = DateTime.Now;
                Stream outputstream = ffmpeg.StandardOutput.BaseStream;
                while (true)
                {
                    try
                    {
                        while (this.paused)
                        {
                            Thread.Sleep(200);
                        }
                        while (count < buffer.Length && readcount < 10)
                        {
                            count += outputstream.Read(buffer, count, buffer.Length - count);
                            if (count == 0)
                                Thread.Sleep(100);
                            ++readcount;
                        }
                        if (count == buffer.Length)
                        {
                            TSPackage package = new UnknownTSPackage();
                            int pos = 0;
                            if (buffer[pos] != TSPackage.SYNCBYTE)
                            {
                                readcount = 0;
                                count = 0;
                                continue;
                            }
                            pos += 1;
                            int payload_unit_start_indicator = (buffer[pos] >> 6) & 0x1;
                            int transport_priority = (buffer[pos] >> 5) & 0x1;
                            int _pid = ((buffer[pos] & 0x1F) << 8) | buffer[pos + 1];
                            pos += 2;
                            int transport_scrambling_control = (buffer[pos] >> 6) & 0x03;
                            int adaption_field_control = (buffer[pos] >> 4) & 0x03;
                            int continuity_counter = buffer[pos] & 0xF;
                            pos += 1;
                            if ((adaption_field_control & 0x1) == 0x1 && _pid == 0)
                            {
                                package = PSI.Parse(buffer, pos);
                            }
                            if ((this.currentTime - seconds > CACHELENGTH && package is PAT))
                            {
                                lock (this.cachedVideo)
                                {
                                    this.cachedVideo[this.cachedVideo.Count - 1].Flush();
                                    if (this.cachedVideo.Count > TOTALCACHEFILES)
                                    {
                                        this.fileSequence++;
                                        //   this.startfileSequence++;
                                        this.cachedVideo[0].Close();
                                        this.cachedVideo.RemoveAt(0);
                                    }
                                    Log.WriteLine("缓存内有" + this.cachedVideo.Count + "个文件");
                                    this.cachedVideo.Add(new MemoryStream());
                                }
                                double ffmpegcurrenttime = (double)this.currentTime;
                                seconds = this.currentTime;
                                while (((DateTime.Now - startTime).TotalSeconds + (CACHELENGTH - 2) * TOTALCACHEFILES < ffmpegcurrenttime))
                                    Thread.Sleep(200);

                            }
                            this.cachedVideo[this.cachedVideo.Count - 1].Write(buffer, 0, buffer.Length);
                        }
                        else if (this.transcodefinished)
                        {
                            lock (this.cachedVideo)
                            {
                                this.cachedVideo[this.cachedVideo.Count - 1].Flush();
                                if (this.cachedVideo.Count > 10)
                                {
                                    this.fileSequence++;
                                    this.cachedVideo[0].Close();
                                    this.cachedVideo.RemoveAt(0);
                                }
                                Log.WriteLine("转码结束");
                                this.cachedVideo.Add(new MemoryStream());
                            }
                            return;
                        }
                        else
                        {
                            Log.WriteLine("读取失败，在尝试的次数内未读满188字节"); ;
                            return;
                        }
                        readcount = 0;
                        count = 0;

                    }
                    catch (ThreadAbortException)
                    {
                        return;
                    }
                }
            })).Start();

            if (this._input is ISupportAddOutputStream)
                ((ISupportAddOutputStream)this._input).AddOutputStream(ffmpeg.StandardInput.BaseStream);
            else if (this._input is IFileStream)
            { }
            else
                throw new ArgumentException("输入端不支持输出");
            this._input.Start();
        }
    }
}
