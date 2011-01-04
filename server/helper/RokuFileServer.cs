using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Text.RegularExpressions;

namespace HDPVRRecoder_W.helper
{
    public class RokuFileServer:BaseHelper
    {
        private HttpServer server;
        private string _rootpath = "c:\\";
        public RokuFileServer(int port,string rootpath)
        {
            this.server = new HttpServer(port);
            this._rootpath = rootpath;
        }
        public override void Start()
        {
            this.server.HttpConnectionEvent += new EventHandler<HttpServer.HttpConnectionEventArgs>(server_HttpConnectionEvent);
            this.server.Start();
            base.Start();
        }
        public override void Stop()
        {
            this.server.Stop();
            base.Stop();
        }

        void server_HttpConnectionEvent(object sender, HttpServer.HttpConnectionEventArgs e)
        {
            string path = e.url.Replace('/', '\\').Trim('\\');
            string requestpath = Path.Combine(_rootpath,path);//TODO 注意输入太多的..可能导致
            if (requestpath.EndsWith(".m3u8"))
            {
                string file = requestpath.Substring(0, requestpath.Length - ".m3u8".Length);
                if (File.Exists(file))
                {
                    e.responseCode = System.Net.HttpStatusCode.OK;
                    e.responseDesc = "OK";
                    e.responseHeaders = new Dictionary<string, string>();
                    e.responseHeaders.Add("Server", "Cute Server");
                    e.responseHeaders.Add("Content-Type", "application/vnd.apple.mpegurl");
                    e.responseHeaders.Add("Connection", "Close");
                    string respbody = @"#EXTM3U 
#EXT-X-STREAM-INF:PROGRAM-ID=1,BANDWIDTH=800000
http://" + Regex.Replace(e.headers["Host"], ":.*", "") + ":90/" + file.Substring(_rootpath.Length).Replace('\\', '/').Trim('/') + ".m3u8?sid=" + (e.querys.ContainsKey("sid") ? e.querys["sid"]:Guid.NewGuid().ToString("N"));
                    e.responseBody = Encoding.UTF8.GetBytes(respbody);
                    return;
                }
            }
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?><root></root>");
            if (Directory.Exists(requestpath))
            {
                DirectoryInfo dir = new DirectoryInfo(requestpath);
                DirectoryInfo[] subdirs = dir.GetDirectories();
                {
                    XmlElement element = doc.CreateElement("tv");
                    XmlAttribute attribute;
                    attribute = doc.CreateAttribute("name");
                    attribute.Value = "HD-PVR";
                    element.Attributes.Append(attribute);
                    attribute = doc.CreateAttribute("bitrate");
                    attribute.Value = "2048";
                    element.Attributes.Append(attribute);
                    attribute = doc.CreateAttribute("format");
                    attribute.Value = "hls";
                    element.Attributes.Append(attribute);
                    string u = "http://" + Regex.Replace(e.headers["Host"], ":.*", "") + ":91";
                    element.InnerXml = RokuEncode(u);
                    doc.DocumentElement.AppendChild(element);
                }
                foreach (DirectoryInfo subdir in subdirs)
                {
                    if (subdir.Name[0] == '$')
                        continue;
                    XmlElement element = doc.CreateElement("folder");
                    XmlAttribute attribute;
                    attribute = doc.CreateAttribute("name");
                    attribute.Value = subdir.Name;
                    element.Attributes.Append(attribute);
                    string u = "http://" + e.headers["Host"] + "/" + subdir.FullName.Substring(_rootpath.Length).Replace('\\', '/').Trim('/');
                    element.InnerXml = RokuEncode(u);
                    doc.DocumentElement.AppendChild(element);

                }
                FileInfo[] subfiles = dir.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (FileInfo subfile in subfiles)
                {
                    if (!(subfile.Name.EndsWith(".ts") || subfile.Name.EndsWith(".mp4") || subfile.Name.EndsWith(".mkv")))
                        continue;
                    XmlElement element = doc.CreateElement("file");
                    XmlAttribute attribute;
                    attribute = doc.CreateAttribute("name");
                    attribute.Value = subfile.Name;
                    element.Attributes.Append(attribute);

                    attribute = doc.CreateAttribute("bitrate");
                    attribute.Value = "2048";
                    element.Attributes.Append(attribute);

                    attribute = doc.CreateAttribute("format");
                    attribute.Value = "hls";
                    element.Attributes.Append(attribute);
                    string sid = Guid.NewGuid().ToString("N");
                    //string u = "http://" + Regex.Replace(e.headers["Host"],":.*","") + ":90/" + subfile.FullName.Substring(_rootpath.Length).Replace('\\', '/').Trim('/') + ".m3u8";
                    string u = "http://" + e.headers["Host"] + "/" + subfile.FullName.Substring(_rootpath.Length).Replace('\\', '/').Trim('/') + ".m3u8?sid=" + sid;
                    element.InnerXml = RokuEncode(u);

                    string stopurl = "http://" + Regex.Replace(e.headers["Host"], ":.*", "") + ":90/action?sid=" + sid + "&action=stop";
                    attribute = doc.CreateAttribute("stopurl");
                    attribute.Value = stopurl;
                    element.Attributes.Append(attribute);

                    string pauseurl = "http://" + Regex.Replace(e.headers["Host"], ":.*", "") + ":90/action?sid=" + sid + "&action=pause";
                    attribute = doc.CreateAttribute("pauseurl");
                    attribute.Value = pauseurl;
                    element.Attributes.Append(attribute);

                    string resumeurl = "http://" + Regex.Replace(e.headers["Host"], ":.*", "") + ":90/action?sid=" + sid + "&action=resume";
                    attribute = doc.CreateAttribute("resumeurl");
                    attribute.Value = resumeurl;
                    element.Attributes.Append(attribute);

                    doc.DocumentElement.AppendChild(element);

                }
                e.responseCode = System.Net.HttpStatusCode.OK;
                e.responseDesc = "OK";
                e.responseHeaders = new Dictionary<string, string>();
                e.responseHeaders.Add("Server", "Cute Server");
                e.responseHeaders.Add("Content-Type", "text/xml");
                e.responseHeaders.Add("Connection", "Close");
                MemoryStream ms = new MemoryStream();
                doc.Save(ms);
                e.responseBody = ms.ToArray();
                ms.Close();
                return;
            }
            e.responseCode = System.Net.HttpStatusCode.NotFound;
            e.responseDesc = "Not Found";
            e.responseHeaders = new Dictionary<string, string>();
            e.responseHeaders.Add("Server", "Cute Server");
            e.responseHeaders.Add("Connection", "Close");
            e.responseBody = new byte[] { };
        }
        protected override void Run()
        {
            
        }
        public string RokuEncode(string original)
        {
            byte[] data = Encoding.UTF8.GetBytes(original);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in data)
                sb.Append("%" + b.ToString("X2"));
            return sb.ToString();
        }
        public string RokuDecode(string value)
        {
            StringBuilder original = new StringBuilder();
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '%')
                {
                    original.Append((char)byte.Parse(value.Substring(i+1,2),System.Globalization.NumberStyles.AllowHexSpecifier));
                    i += 2;
                }
                else
                    original.Append(value[i]);
            }
            return original.ToString();
        }
    }
}
