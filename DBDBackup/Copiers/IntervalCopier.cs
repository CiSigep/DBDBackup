using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBDBackup.Copiers
{
    class IntervalCopier : NonExitCopier
    {
        private FolderCopy fc;
        private int interval;
        private Thread t;

        public IntervalCopier(FolderCopy fc, int interval)
        {
            if (interval <= 0)
                throw new Exception("interval must not be 0");


            this.interval = interval;
            this.fc = fc;
        }

        public void start()
        {
            lock (this)
            {
                if (t == null)
                {
                    t = new Thread(intervalRun);
                    t.Start();
                }
                else
                    throw new Exception("Interval already running");
            }
        }

        public void stop()
        {
            lock (this)
            {
                if (t != null)
                {
                    t.Abort();
                    t = null;
                }
                else
                {
                    throw new Exception("Interval is not running");
                }
            }
        }

        private void intervalRun()
        {
            while (true)
            {
                lock (this)
                {
                    fc.CopyFolderContents();
                }
                Thread.Sleep(interval);
            }
        }

    }
}
