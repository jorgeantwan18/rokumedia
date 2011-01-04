using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Threading;

namespace HDPVRRecoder_W.helper
{
    public class HttpServer:BaseHelper
    {
        private TcpListener _listener;
        private List<HttpConnection> _requests;
        public HttpServer(int listenport)
        {
            this._listener = new TcpListener(IPAddress.Any, listenport);
            this._requests = new List<HttpConnection>();
        }
        public class HttpConnectionEventArgs : EventArgs
        {
            public string url;
            public Dictionary<string, string> headers;
            public Dictionary<string, string> querys;
            public byte[] body;
            public IPEndPoint _local;
            public IPEndPoint _remote;


            public Dictionary<string, string> responseHeaders;
            public byte[] responseBody;
            public HttpStatusCode responseCode;
            public string responseDesc;

            public HttpConnectionEventArgs(string url, Dictionary<string, string> headers, Dictionary<string, string> querys, byte[] body,IPEndPoint local,IPEndPoint remote)
            {
                this.url = url;
                this.headers = headers;
                this.querys = querys;
                this.body = body;
                this._local = local;
                this._remote = remote;
            }
        }
        public event EventHandler<HttpConnectionEventArgs> HttpConnectionEvent;
        public void OnHttpConnection(object sender, HttpConnectionEventArgs e)
        {
            if (this.HttpConnectionEvent != null)
                this.HttpConnectionEvent(sender, e);
        }

        protected override void Run()
        {
            this._listener.Start();
            while (true)
            {
                try
                {
                    TcpClient client = this._listener.AcceptTcpClient();
                    HttpConnection connection = new HttpConnection(this, client);
                    lock (this._requests)
                    {
                        this._requests.Add(connection);
                    }
                    connection.Start();

                }
                catch {
                
                }
            }
            
        }
        public void RemoveConnection(HttpConnection connection)
        {
            lock (this._requests)
            {
                this._requests.Remove(connection);
            }
        }
        public override void Stop()
        {
            this._listener.Stop();
            base.Stop();
            lock (this._requests)
            {
                this._requests.ForEach(new Action<HttpConnection>(delegate(HttpConnection t)
                {
                    try
                    {
                        t.Stop();
                    }
                    catch { }
                }));
                this._requests.Clear();
            }
        }
    }
}
