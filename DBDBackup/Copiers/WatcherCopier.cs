using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBDBackup.Copiers
{
    class WatcherCopier : NonExitCopier
    {
        private FolderCopy fc;
        private FileSystemWatcher fsw;

        public WatcherCopier(FolderCopy fc, string location)
        {
            this.fc = fc;
            fsw = new FileSystemWatcher();
            fsw.Path = location;
            fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
        }


        public void start()
        {
            lock (this)
            {
                if (!fsw.EnableRaisingEvents)
                {
                    fsw.Changed += onChanges;
                    fsw.EnableRaisingEvents = true;
                }
                else
                {
                    throw new Exception("Watcher already running");
                }
            }

        }

        public void stop()
        {
            lock (this)
            {
                if (fsw.EnableRaisingEvents)
                {
                    fsw.Changed -= onChanges;
                    fsw.EnableRaisingEvents = false;
                }
                else
                    throw new Exception("Watcher is not running");
            }
        }

        private void onChanges(object o, FileSystemEventArgs fsea)
        {
            lock (this)
            {
                fc.CopyFolderContents();
            }
        }
    }
}
