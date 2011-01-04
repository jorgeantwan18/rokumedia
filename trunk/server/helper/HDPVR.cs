using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using DirectShowLib;
using System.Threading;
using System.IO;
using System.Diagnostics;

namespace HDPVRRecoder_W.helper
{
    public class HDPVR:BaseHelper,ISupportAddOutputStream,IFFMpegParameters
    {
        public static readonly HDPVR Instance = new HDPVR();
        
        private const string HDPVRFilterName = "Hauppauge HD PVR Encoder";
        private static readonly Guid HDPVRAvgBitrate = new Guid("49cc4c43-ca83-4ad4-a9af-f3696af666df");
        private static readonly Guid HDPVRPeakBitrate = new Guid("703f16a9-3d48-44a1-b077-018dff915d19");
        private static readonly Guid HDPVRBitrateMode = new Guid("ee5fb25c-c713-40d1-9d58-c0d7241e250f");
        private string filterGraphFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "hdpvrcap.grf");
        private string dumpFilterName = "ZZ Dump";
        private IGraphBuilder graphBuilder;
        private IFileSinkFilter sink;
        private IBaseFilter fileDump = null;
        private IMediaControl mediaControl = null;
        private const string SAVEFILEPIPE = "\\\\.\\pipe\\hdpvr.ts";

        
        private List<Stream> receiveStreams;

        private HDPVR()
        {
            this.receiveStreams = new List<Stream>();
        }
        protected override void Run()
        {
#if !DEBUG
            WindowsNamedPipe pipe1 = new WindowsNamedPipe("hdpvr.ts", true);
            WindowsNamedPipe pipe2 = new WindowsNamedPipe("hdpvr.ts", true);
#endif

            try
            {

#if !DEBUG
                if (!File.Exists(filterGraphFile))
                    throw new System.Exception("Filter graph file doesn't exist: " + filterGraphFile);
                graphBuilder = (IGraphBuilder)new FilterGraph();
                FilterGraphTools.LoadGraphFile(graphBuilder, filterGraphFile);

                SetSaveFile(SAVEFILEPIPE);

                setFiltProp(graphBuilder, "Hauppauge HD PVR Encoder", HDPVRAvgBitrate, 0, 3000000);
                setFiltProp(graphBuilder, "Hauppauge HD PVR Encoder", HDPVRPeakBitrate, 0, 3000000);
                setFiltProp(graphBuilder, "Hauppauge HD PVR Encoder", HDPVRBitrateMode, 0, 0);

                mediaControl = (IMediaControl)this.graphBuilder;
                mediaControl.Run();
#endif
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                return;
            }

#if !DEBUG
            new Thread(new ThreadStart(delegate
            {
                byte[] tmp = new byte[188];
                pipe1._connected.WaitOne();
                while (true)
                {
                    int readcount = pipe1.Read(tmp);
                    Log.WriteLine(readcount.ToString());
                    if (readcount > 0)
                    {
                        Log.WriteLine("pipe1 read " + readcount + " bytes, this is not normal");
                    }
                    else if (readcount == -1)
                    {
                        Log.WriteLine("pipe1 disconnected, do not worry, this is normal");
                        pipe1.Close();
                        break;
                    }
                }
            })).Start();
#endif
            byte[] readbuffer = new byte[188 * 20 * 1024];
            int loopcount = 0;
            DateTime noneReceiverFrom = DateTime.Now;
            try
            {

                while (true)
                {
#if !DEBUG
                    int readcount = pipe2.Read(readbuffer);

                    if (readcount == -1)
                    {
                        if (loopcount < 10)
                        {
                            Thread.Sleep(1000);
                            loopcount++;
                            continue;
                        }
                        Log.WriteLine("pipe2 disconnected");
                        pipe2.Close();
                        return;
                    }
                    Thread.Sleep(1000);
#else
                                FileStream fs = new FileStream("c:\\0.ts", FileMode.Open, FileAccess.Read);
                                int readcount = fs.Read(readbuffer, 0, readbuffer.Length);
#endif
                    loopcount = 0;
                    lock (this.receiveStreams)
                    {
                        if (receiveStreams.Count > 0)
                            noneReceiverFrom = DateTime.Now;
                        else if ((DateTime.Now - noneReceiverFrom).TotalSeconds > 60 && this.working)
                        { 
                            //关闭
                            new Thread(new ThreadStart(delegate {
                                this.Stop();
                            })).Start();
                            return;
                        }
                        foreach (Stream s in receiveStreams)
                        {
                            s.Write(readbuffer, 0, readcount);
                            Log.WriteLine("写入" + readcount + "字节");
                        }
                    }
                }
            }
            catch (ThreadAbortException)
            {
                return;
            }
            catch (Exception e)
            {
                Log.WriteLine(e.ToString());
                return;
            }
        }
        
        public override void Start()
        {
            base.Start();

        }
        public override void Stop()
        {
            
            if (this.receiveStreams.Count > 0)
            {
                Log.WriteLine("cannot stop since there are other receivers");
                return;
            }
            if (this.working)
            {
                Log.WriteLine("stopping........");
                try
                {
                    if (!object.Equals(graphBuilder, null))
                    {
                        int hr = (this.graphBuilder as IMediaControl).StopWhenReady();
                        hr = (this.graphBuilder as IMediaControl).Stop();
                    }
                    FilterGraphTools.RemoveAllFilters(this.graphBuilder);

                    if (!object.Equals(this.sink, null)) Marshal.ReleaseComObject(sink); this.sink = null;
                    if (!object.Equals(this.fileDump, null)) Marshal.ReleaseComObject(this.fileDump); this.fileDump = null;
                    if (!object.Equals(this.graphBuilder, null)) Marshal.ReleaseComObject(this.graphBuilder); this.graphBuilder = null;
                }
                catch { }
            }
            base.Stop();
            
        }

        #region ChangeChannel
        public void ChangeChannel(string channel)
        {
#if !DEBUG
            Process p = new Process();
            //p.StartInfo.FileName = "C:\\Recoder\\extensions\\WinTV2TME.exe";
            //p.StartInfo.CreateNoWindow = false;
            //p.StartInfo.Arguments = "-c" + channel;

            p.StartInfo.FileName = @"C:\Program Files\Timmmoore\MCE 2005 STB Controller\channel.exe";
            p.StartInfo.WorkingDirectory = @"C:\Program Files\Timmmoore\MCE 2005 STB Controller";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.Arguments = "-f -a0 2 " + channel;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.Start();
            p.WaitForExit();
#endif

        }
        #endregion

        #region hdpvr functions
        private bool setFiltProp(IGraphBuilder builder, string filtName, Guid guid, int propid, int propval)
        {
            IntPtr val = Marshal.AllocHGlobal(4);
            try
            {

                IBaseFilter ibf = FilterGraphTools.FindFilterByName(builder, filtName);
                IKsPropertySet ips = (IKsPropertySet)ibf;

                int[] pva = new int[] { propval };
                Marshal.Copy(pva, 0, val, 1);
                int hr = ips.Set(guid, propid, IntPtr.Zero, 0, val, 4);
                Marshal.ThrowExceptionForHR(hr);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                Marshal.FreeHGlobal(val);
            }
        }
        private void SetSaveFile(string filename)
        {
            fileDump = FilterGraphTools.FindFilterByName(graphBuilder, dumpFilterName);
            if (object.Equals(fileDump, null))
                throw new System.Exception("Couldn't find dump filter in filter graph: " + dumpFilterName);


            sink = fileDump as IFileSinkFilter;

            AMMediaType media = new AMMediaType();
            media.majorType = MediaType.Video;
            media.subType = MediaSubType.Mpeg2Transport;
            media.formatType = FormatType.VideoInfo;


            int hr = sink.SetFileName(filename, media);
            DsError.ThrowExceptionForHR(hr);
        }
        #endregion



        public void AddOutputStream(Stream stream)
        {
            lock (this.receiveStreams)
            {
                this.receiveStreams.Add(stream);
            }
        }

        public void RemoveOutputStream(Stream stream)
        {
            lock (this.receiveStreams)
            {
                if (this.receiveStreams.Contains(stream))
                    this.receiveStreams.Remove(stream);
            }
        }

        public string GetFFMpegParameters()
        {
            return " -vcodec copy -acodec libfaac -ac 2 -ab 192k ";
        }


        public string GetSize()
        {
            return "";
        }
    }
}
