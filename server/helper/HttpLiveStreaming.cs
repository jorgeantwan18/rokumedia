using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HDPVRRecoder_W.helper
{
    public abstract class HttpLiveStreaming:BaseHelper
    {
        private HttpServer server;
        protected List<FFMpeg> _transcodingInstance;
        // private int startfileSequence = 0;
        protected int currentChannel = 0;
        /// <summary>
        /// ts文件长度，10秒
        /// </summary>
        protected int tsfiletimelength = 10;

        public HttpLiveStreaming(int port)
        {
            this.server = new HttpServer(port);
            this.server.HttpConnectionEvent += new EventHandler<HttpServer.HttpConnectionEventArgs>(server_HttpConnectionEvent);
            this._transcodingInstance = new List<FFMpeg>();
        }

        protected virtual void server_HttpConnectionEvent(object sender, HttpServer.HttpConnectionEventArgs e)
        { 
            
        }
        public override void Start()
        {
            this.server.Start();
            base.Start();
        }

        protected override void Run()
        {
            
        }
    }
}
