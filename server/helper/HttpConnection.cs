using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace HDPVRRecoder_W.helper
{
    public class HttpConnection:BaseHelper
    {
        private NetworkStream stream;
        private HttpServer parent;
        private IPEndPoint _local;
        private IPEndPoint _remote;

        public HttpConnection(HttpServer parent,TcpClient client) {
            this.stream = client.GetStream();
            this.parent = parent;
            this._local = (IPEndPoint)client.Client.LocalEndPoint;
            this._remote = (IPEndPoint)client.Client.RemoteEndPoint;
        }

        protected override void Run()
        {
            byte[] buffer = new byte[10240];
            int count = 0;
            try
            {
                count = stream.Read(buffer, 0, buffer.Length);
            }
            catch (ObjectDisposedException)
            {
                stream.Close();
                this.parent.RemoveConnection(this);
                this.parent = null;
                return;
            }

            int startpos = 0;
            int pos = 0;
            byte NL = (byte)'\n';
            byte CR = (byte)'\r';
            string url = "";
            Dictionary<string, string> headers = new Dictionary<string, string>();
            Dictionary<string, string> querys = new Dictionary<string, string>();
            byte[] body = null;

            int i = startpos;
            for (; i < count; i++)
            {
                if (buffer[i] == NL)
                {
                    string commandline = Encoding.ASCII.GetString(buffer, startpos, (i > 1 && buffer[i - 1] == CR ? i - 1 : i) - startpos);
                    Log.WriteLine("requesting " + commandline);
                    url = commandline.Substring(commandline.IndexOf(' ') + 1, commandline.LastIndexOf(' ') - commandline.IndexOf(' ') - 1);
                    Log.WriteLine("url=" + url);
                    pos = url.IndexOf('?');
                    if (pos > -1 && pos < url.Length - 1)
                    {

                        string[] querysstr = url.Substring(pos + 1).Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string q in querysstr)
                        {
                            string[] nv = q.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                            if (nv.Length == 2)
                            {
                                if (!querys.ContainsKey(nv[0]))
                                {
                                    querys.Add(nv[0], nv[1]);
                                }
                            }
                        }
                        url = url.Substring(0, pos);
                    }
                    break;

                }
            }
            ++i;
            startpos = i;
            for (; i < count; i++)
            {
                if (buffer[i] == NL)
                {
                    string line = Encoding.ASCII.GetString(buffer, startpos, (i > 1 && buffer[i - 1] == CR ? i - 1 : i) - startpos);
                    if (line == "")
                        break;
                    else
                    {
                        pos = line.IndexOf(':');
                        if (pos > 0)
                        {
                            string name = line.Substring(0, pos).Trim();
                            string value = (pos == line.Length - 1 ? "" : line.Substring(pos + 1)).Trim();
                            if (headers.ContainsKey(name))
                                headers[name] += ";" + value;
                            else
                                headers[name] = value;
                        }
                        startpos = i + 1;
                    }
                }
            }
            ++i;
            startpos = i;
            int contentlength = 0;
            if (headers.ContainsKey("Content-Length"))
            {
                if (!int.TryParse(headers["Content-Length"], out contentlength))
                    contentlength = 0;
            }
            if (contentlength > 0)
            {
                body = new byte[contentlength];
                int leftbytes = count - startpos;
                Buffer.BlockCopy(buffer, startpos, body, 0, Math.Min(leftbytes, contentlength));
                if (contentlength > leftbytes)
                {
                    count = 0;
                    int tryreadcount = 0;
                    while (count < contentlength - leftbytes && tryreadcount < 10)
                    {
                        try
                        {
                            count += stream.Read(body, leftbytes + count, body.Length - count - leftbytes);
                        }
                        catch (ObjectDisposedException)
                        {
                            stream.Close();
                            this.parent.RemoveConnection(this);
                            this.parent = null;
                            return;
                        }
                        ++tryreadcount;
                        Thread.Sleep(200);
                    }
                }
            }
            if (!string.IsNullOrEmpty(url))
            {
                HttpServer.HttpConnectionEventArgs eventargs = new HttpServer.HttpConnectionEventArgs(url, headers,querys, body,this._local,this._remote);
                this.parent.OnHttpConnection(this, eventargs);
                //回复
                StringBuilder sb = new StringBuilder();
                sb.Append("HTTP/1.1 " + (int)eventargs.responseCode + " " + eventargs.responseDesc + "\r\n");
                foreach (string name in eventargs.responseHeaders.Keys)
                {
                    sb.Append(name + ": " + eventargs.responseHeaders[name] + "\r\n");
                }
                if (eventargs.responseBody != null && eventargs.responseBody.Length > 0 && !eventargs.headers.ContainsKey("Content-Length"))
                {
                    sb.Append("Content-Length: " + eventargs.responseBody.Length + "\r\n");
                }
                sb.Append("\r\n");
                byte[] responsedata = Encoding.ASCII.GetBytes(sb.ToString());
                try
                {
                    stream.Write(responsedata, 0, responsedata.Length);
                }
                catch (ObjectDisposedException)
                {
                    stream.Close();
                    this.parent.RemoveConnection(this);
                    this.parent = null;
                    return;
                }
                if (eventargs.responseBody != null && eventargs.responseBody.Length > 0)
                {
                    try
                    {
                        stream.Write(eventargs.responseBody, 0, eventargs.responseBody.Length);
                    }
                    catch (ObjectDisposedException)
                    {
                        stream.Close();
                        this.parent.RemoveConnection(this);
                        this.parent = null;
                        return;
                    }
                }
            }
            stream.Close();
            this.parent.RemoveConnection(this);
            this.parent = null;
            
        }
        public override void Stop()
        {
            stream.Close();
            this.parent = null;
            base.Stop();
        }
    }
}
