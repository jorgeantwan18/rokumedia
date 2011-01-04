using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using HDPVRRecoder_W.ts;
using System.Threading;
using MediaInfoLib;

namespace HDPVRRecoder_W.helper
{
    public class FileHttpLiveStreaming:HttpLiveStreaming
    {
        
       
        private class FileInput : BaseHelper, IFileStream,IFFMpegParameters {
            public string filename;
            private string _size = "";
            private string _parameter = "";
            public FileInput(string filename)
            {
                this.filename = filename;
                MediaInfo MI = new MediaInfo();
                if (MI.Open(this.GetFileName()) > 0)
                {

                    string s = MI.Option("Info_Parameters");

                    int width = int.Parse(MI.Get(StreamKind.Video, 0, "Width"));
                    int height = int.Parse(MI.Get(StreamKind.Video, 0, "Height"));
                    decimal aspect = (decimal)width / (decimal)height;
                    bool resize = false;
                    if (width > 1280)
                    {
                        width = 1280;
                        height = (int)(width / aspect);
                        resize = true;
                    }
                    if (height > 720)
                    {
                        height = 720;
                        width = (int)(height * aspect);
                        resize = true;
                    }
                    if (resize)
                    {
                        if (width % 2 > 0)
                            width -= 1;
                        if (height % 2 > 0)
                            height -= 1;
                        this._size = " -s " + width + "x" + height;
                    }
                    Log.WriteLine("resize=" + resize + ",size=" + width + "x" + height);


                    int videobitrate = int.Parse(MI.Get(StreamKind.Video, 0, "BitRate"));
                    if (videobitrate > 7 * 1024 * 1024 || resize)
                    {
                        this._parameter = " libx264 -coder 0 -flags -loop -cmp +chroma -partitions -parti8x8-parti4x4-partp8x8-partb8x8 -me_method dia -subq 0 -me_range 16 -g 250 -keyint_min 25 -sc_threshold 0 -i_qfactor 0.71 -b_strategy 0 -qcomp 0.6 -qmin 10 -qmax 51 -qdiff 4 -bf 0 -refs 1 -directpred 1 -trellis 0 -flags2 -bpyramid-mixed_refs-wpred-dct8x8+fastpskip-mbtree -wpredp 0 -b 4096k ";
                    }
                    else
                    {
                        this._parameter = " copy";
                    }
                    this._parameter = " -vcodec " + this._parameter;
                    //Log.WriteLine("media info supported :" + s);

                    MI.Close();
                }


                if (this.GetFileName().ToLower().EndsWith(".mp4") || this.GetFileName().ToLower().EndsWith(".mkv"))
                {
                    this._parameter += " -vbsf h264_mp4toannexb ";
                }
                this._parameter += " -acodec libfaac -ac 2 -ab 192k ";
            }
            public string GetFileName()
            {
                return this.filename;
            }

            protected override void Run()
            {
                
            }
            
            public string GetFFMpegParameters()
            {
                return this._parameter;
            }


            public string GetSize()
            {
                return this._size;
            }
        }
        public FileHttpLiveStreaming(int port)
            : base(port)
        {
            
            //this.ffmpeg = new FFMpeg(new FileInput());
            Log.WriteLine("HDPVRHttpLiveStreaming 初始化");
        }
        
        protected override void Run()
        {
            base.Run();

            try
            {
                while (true)
                {
                    Thread.Sleep(300);
                    lock (this._transcodingInstance)
                    {
                        for (int i = this._transcodingInstance.Count - 1; i >= 0; i--)
                        {
                            if ((DateTime.Now - this._transcodingInstance[i]._lastActiveTime).TotalSeconds > 30)
                            {
                                this._transcodingInstance[i].Stop();
                                this._transcodingInstance.RemoveAt(i);
                            }
                        }
                    }
                }
            }
            catch (ThreadAbortException) { }
        }
        protected override void server_HttpConnectionEvent(object sender, HttpServer.HttpConnectionEventArgs e)
        {
            if (e.querys.ContainsKey("sid"))
            {
                FFMpeg ffmpeg = this._transcodingInstance.Find(delegate(FFMpeg f)
                {
                    return f.Sid == e.querys["sid"];
                });
                if (e.url.EndsWith(".m3u8") && ffmpeg == null)
                {
                    string path = e.url.Replace('/', '\\').Trim('\\');
                    if (path.EndsWith(".m3u8"))
                    {
                        path = path.Substring(0, path.Length - ".m3u8".Length);
                        if (File.Exists(path))
                        {
                            ffmpeg = new FFMpeg(new FileInput(path), e.querys["sid"]);
                            ffmpeg.Start();
                            lock (this._transcodingInstance)
                            {
                                this._transcodingInstance.Add(ffmpeg);
                            }
                        }
                        else
                            goto onerror;
                    }
                    else
                        goto onerror;
                }
                if (ffmpeg == null)
                    goto onerror;
                else
                    ffmpeg._lastActiveTime = DateTime.Now;
                if (e.url.Trim('/') == "action" && e.querys.ContainsKey("action"))
                {
                    if (e.querys["action"] == "pause")
                    {
                        ffmpeg.Pause();
                    }
                    else if (e.querys["action"] == "stop")
                    {
                        ffmpeg.Stop();
                        lock (this._transcodingInstance)
                        {
                            this._transcodingInstance.Remove(ffmpeg);
                        }
                    }
                    else if (e.querys["action"] == "resume")
                    {
                        ffmpeg.Restart();
                    }
                    e.responseCode = System.Net.HttpStatusCode.OK;
                    e.responseDesc = "OK";
                    e.responseHeaders = new Dictionary<string, string>();
                    e.responseHeaders.Add("Server", "Cute Server");
                    e.responseHeaders.Add("Content-Type", "application/vnd.apple.mpegurl");
                    e.responseHeaders.Add("Connection", "Close");
                    e.responseBody = new byte[] { };
                    return;
                }
                if (e.url.EndsWith(".m3u8"))
                {
                    string body = @"#EXTM3U
#EXT-X-TARGETDURATION:" + tsfiletimelength + @"
#EXT-X-MEDIA-SEQUENCE:" + ffmpeg.fileSequence + "\r\n";
                    ffmpeg.Start();

                    while (ffmpeg.cachedVideo.Count <= 1)
                    {
                        Thread.Sleep(1000);
                    }
                    lock (ffmpeg.cachedVideo)
                    {

                        for (int i = 0; i < ffmpeg.cachedVideo.Count - 1; i++)
                        {
                            body += "#EXTINF:" + tsfiletimelength + ", no desc\r\nhttp://" + e.headers["Host"] + "/" + (ffmpeg.fileSequence + i).ToString() + ".ts?sid=" + e.querys["sid"] + "\r\n";
                        }

                    }
                    if (ffmpeg.transcodefinished)
                        body += "#EXT-X-ENDLIST\r\n";
                    if (body.Length > 0)
                        body = body.Substring(0, body.Length - 2);
                    e.responseCode = System.Net.HttpStatusCode.OK;
                    e.responseDesc = "OK";
                    e.responseHeaders = new Dictionary<string, string>();
                    e.responseHeaders.Add("Server", "Cute Server");
                    e.responseHeaders.Add("Content-Type", "application/vnd.apple.mpegurl");
                    e.responseHeaders.Add("Connection", "Close");
                    e.responseBody = Encoding.UTF8.GetBytes(body);
                    Log.WriteLine("请求当前频道数据文件:" + body);
                    return;
                }
                else if (e.url.EndsWith(".ts"))
                {
                    int sequence = 0;
                    if (int.TryParse(e.url.Substring(0, e.url.Length - ".ts".Length).Trim('/'), out sequence))
                    {
                        sequence = sequence - ffmpeg.fileSequence;
                        if (sequence >= 0 && sequence < ffmpeg.cachedVideo.Count - 1)
                        {
                            e.responseCode = System.Net.HttpStatusCode.OK;
                            e.responseDesc = "OK";
                            e.responseHeaders = new Dictionary<string, string>();
                            e.responseHeaders.Add("Server", "Cute Server");
                            e.responseHeaders.Add("Content-Type", "mpeg/ts");
                            e.responseHeaders.Add("Connection", "Close");
                            lock (ffmpeg.cachedVideo)
                            {
                                e.responseBody = ffmpeg.cachedVideo[sequence].ToArray();
                            }
                            return;
                        }
                        else goto onerror;
                    }
                    else
                        goto onerror;
                }
                else
                    goto onerror;
            }
        onerror:
            e.responseCode = System.Net.HttpStatusCode.NotFound;
            e.responseDesc = "Not Exist Any More";
            e.responseHeaders = new Dictionary<string, string>();
            e.responseHeaders.Add("Server", "Cute Server");
            e.responseHeaders.Add("Connection", "Close");
            e.responseBody = new byte[] { };
            base.server_HttpConnectionEvent(sender, e);
        }
    }
}
