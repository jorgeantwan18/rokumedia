using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO;

namespace HDPVRRecoder_W.helper
{
    public abstract class BaseHelper
    {
        protected abstract void Run();
        
        public string SyncLock = "";
        protected bool working = false;
        protected Thread _runningThread;
        public virtual void Start()
        {
            lock (SyncLock)
            {
                if (this.working)
                    return;
                this.working = true;
            }

           // this.Stop();
            this._runningThread = new Thread(new ThreadStart(this.Run));
            this._runningThread.Start();
        }
        public virtual void Stop()
        {
            if (this._runningThread != null)
            {
                try
                {
                    this._runningThread.Abort();
                }
                catch { }
            }
            this.working = false;
        }
    }
}
