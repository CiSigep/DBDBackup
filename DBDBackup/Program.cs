using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using DBDBackup.Copiers;

namespace DBDBackup
{ 
    class Program
    {
        static void Main(string[] args)
        {
            string destination = @"%USERPROFILE%\Documents\DBDBackups\";
            string exeLoc = @"C:\Program Files (x86)\Steam\steamapps\common\Dead By Daylight\";
            int interval = 600000;
            int maxBackups = 5;
            BackupMode backupMode = BackupMode.ON_EXIT;
            FolderCopy fc;

            try
            {
                XmlDocument config = new XmlDocument(); // Allow for user configuration
                config.Load("BackupConfig.xml");

                XmlNode configRoot = config.SelectSingleNode("config");

                if (configRoot["destination"] != null)
                    destination = configRoot["destination"].InnerText;
                if (configRoot["exeLoc"] != null)
                    exeLoc = configRoot["exeLoc"].InnerText;
                if (configRoot["maxBackups"] != null)
                    maxBackups = Convert.ToInt32(configRoot["maxBackups"].InnerText);
                if (configRoot["backupMode"] != null)
                    backupMode = (BackupMode)(Enum.Parse(typeof(BackupMode), configRoot["backupMode"].InnerText));
                if (configRoot["interval"] != null)
                    interval = Convert.ToInt32(configRoot["interval"].InnerText) * 60000;
            }
            catch (Exception e)
            { }

            fc = new FolderCopy(maxBackups, @"C:\Program Files (x86)\Steam\userdata\81409495\381210\remote\ProfileSaves", destination);
            Directory.SetCurrentDirectory(exeLoc);

            Process DBD = new Process();
            DBD.StartInfo.FileName = @"DeadByDayLight.exe";
            DBD.Start();
            DBD.WaitForExit();

            Process[] ps = Process.GetProcessesByName("DeadByDaylight-Win64-Shipping");

            NonExitCopier nec = null;
            if (backupMode == BackupMode.INTERVAL)
                nec = new IntervalCopier(fc, interval);
            else if (backupMode == BackupMode.ON_CHANGE)
                nec = new WatcherCopier(fc, @"C:\Program Files (x86)\Steam\userdata\81409495\381210\remote\ProfileSaves\");

            if (nec != null)
                nec.start();

            ps[0].WaitForExit();

            if (nec != null)
                nec.stop();

            fc.CopyFolderContents();
        }
    }

    public enum BackupMode
    {
        ON_EXIT, INTERVAL, ON_CHANGE
    }

}
