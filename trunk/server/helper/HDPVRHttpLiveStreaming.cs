using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using HDPVRRecoder_W.ts;

namespace HDPVRRecoder_W.helper
{
    public class HDPVRHttpLiveStreaming:HttpLiveStreaming
    {
        
        public HDPVRHttpLiveStreaming(int port):base(port)
        {
            this._transcodingInstance.Add(new FFMpeg(HDPVR.Instance,""));
            Log.WriteLine("HDPVRHttpLiveStreaming 初始化");
        }
        protected override void Run()
        {
            base.Run();
            FFMpeg ffmpeg = this._transcodingInstance[0];
            try
            {
                
                while (true)
                {
                    Thread.Sleep(300);

                    if ((DateTime.Now - ffmpeg._lastActiveTime).TotalSeconds > 30)
                    {
                        ffmpeg.Stop();
                        return;
                    }
                       
                }
            }
            catch (ThreadAbortException) {
                ffmpeg.Stop();
            }
            
        }
        protected override void server_HttpConnectionEvent(object sender, HttpServer.HttpConnectionEventArgs e)
        {
            FFMpeg ffmpeg = this._transcodingInstance[0];
            ffmpeg.Start();
            ffmpeg._lastActiveTime = DateTime.Now;
            if (e.url.EndsWith(".m3u8"))//请求url
            {
                string filewithchannel = e.url.Substring(e.url.LastIndexOf('/') + 1);

                int channel = 0;
                Log.WriteLine("Channel is:" + filewithchannel);
                if (int.TryParse(filewithchannel.Substring(0, filewithchannel.Length - ".m3u8".Length), out channel) && channel != 0 && channel != this.currentChannel)
                {
                    this.currentChannel = channel;
                    Log.WriteLine("频道更换为：" + channel);
                    //换频道
                    HDPVR.Instance.ChangeChannel(channel.ToString());

                }
                string body = @"#EXTM3U
#EXT-X-TARGETDURATION:" + tsfiletimelength + @"
#EXT-X-MEDIA-SEQUENCE:" + ffmpeg.fileSequence + "\r\n";


                while (ffmpeg.cachedVideo.Count < 2)
                {
                    Thread.Sleep(1000);
                }
                lock (ffmpeg.cachedVideo)
                {

                    for (int i = 0; i < ffmpeg.cachedVideo.Count - 1; i++)
                    {
                        body += "#EXTINF:" + tsfiletimelength + ", no desc\r\n" + (ffmpeg.fileSequence + i).ToString() + ".ts\r\n";
                    }

                }
                if (body.Length > 0)
                    body = body.Substring(0, body.Length - 2);
                e.responseCode = System.Net.HttpStatusCode.OK;
                e.responseDesc = "OK";
                e.responseHeaders = new Dictionary<string, string>();
                e.responseHeaders.Add("Server", "Cute Server");
                e.responseHeaders.Add("Content-Type", "application/vnd.apple.mpegurl");
                e.responseHeaders.Add("Connection", "Close");
                e.responseBody = Encoding.UTF8.GetBytes(body);
                return;

            }
            else if (e.url.EndsWith(".ts"))
            {
                int sequence = 0;
                string filename = e.url.Substring(0, e.url.Length - ".ts".Length).Trim('/');

                e.responseCode = System.Net.HttpStatusCode.OK;
                e.responseDesc = "OK";
                e.responseHeaders = new Dictionary<string, string>();
                e.responseHeaders.Add("Server", "Cute Server");
                e.responseHeaders.Add("Content-Type", "mpeg/ts");
                e.responseHeaders.Add("Connection", "Close");

                if (int.TryParse(filename, out sequence))
                {
                    sequence = sequence - ffmpeg.fileSequence;
                    if (sequence >= 0 && sequence < ffmpeg.cachedVideo.Count - 1)
                    {
                        lock (ffmpeg.cachedVideo)
                        {
                            e.responseBody = ffmpeg.cachedVideo[sequence].ToArray();
                        }
                        return;
                    }
                    else
                        goto onerror;
                }
                else goto onerror;
            }
            else
                goto onerror;
        onerror:
            e.responseCode = System.Net.HttpStatusCode.NotFound;
            e.responseHeaders = new Dictionary<string, string>();
            e.responseHeaders.Add("Server", "Cute Server");
            e.responseHeaders.Add("Connection", "Close");
            e.responseDesc = "Not Exist Any More";
            e.responseBody = new byte[] { };
        }
    }
}
