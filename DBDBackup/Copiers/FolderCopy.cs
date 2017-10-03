using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DBDBackup.Copiers
{
    class FolderCopy
    {

        private int maxAllowed;
        private string source, destination;

        public FolderCopy(int maxAllowed, string source, string destination)
        {
            if (maxAllowed <= 0)
                throw new Exception("Maximum must be greater than 0");
            if (source == null)
                throw new Exception("source must not be null");
            if (destination == null)
                throw new Exception("destination must not be null");

            this.maxAllowed = maxAllowed;
            this.source = source;
            this.destination = destination;
        }
        public FolderCopy()
        {
            maxAllowed = 5;
            source = @"C:\Program Files (x86)\Steam\userdata\81409495\381210\remote\ProfileSaves";
            destination = @"%USERPROFILE%\Documents\DBDBackups\";
        }

        internal bool CopyFolderContents()
        {
            return CopyFolderContents(source, Environment.ExpandEnvironmentVariables(destination + "Save" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm_ss")));
        }

        /// <summary>
        /// Uses code from https://www.codeproject.com/Tips/278248/Recursively-Copy-folder-contents-to-another-in-Csh
        /// Credit Balaji Birajdar
        /// </summary>
        internal bool CopyFolderContents(string SourcePath, string DestinationPath)
        {
            lock (this)
            {
                SourcePath = SourcePath.EndsWith(@"\") ? SourcePath : SourcePath + @"\";
                DestinationPath = DestinationPath.EndsWith(@"\") ? DestinationPath : DestinationPath + @"\";

                try
                {
                    if (Directory.Exists(SourcePath))
                    {
                        if (Directory.Exists(DestinationPath) == false)
                        {
                            Directory.CreateDirectory(DestinationPath);
                        }

                        foreach (string files in Directory.GetFiles(SourcePath))
                        {
                            FileInfo fileInfo = new FileInfo(files);
                            fileInfo.CopyTo(string.Format(@"{0}\{1}", DestinationPath, fileInfo.Name), true);
                        }

                        foreach (string drs in Directory.GetDirectories(SourcePath))
                        {
                            DirectoryInfo directoryInfo = new DirectoryInfo(drs);
                            if (CopyFolderContents(drs, DestinationPath + directoryInfo.Name) == false)
                            {
                                return false;
                            }
                        }
                        var dirsInBackups = new List<string>(Directory.GetDirectories(Environment.ExpandEnvironmentVariables(destination)));

                        if (dirsInBackups.Count > maxAllowed)
                        {
                            dirsInBackups.Sort();

                            while (dirsInBackups.Count > maxAllowed)
                            {
                                Directory.Delete(dirsInBackups[0], true);
                                dirsInBackups.RemoveAt(0);

                            }

                        }

                    }
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }
    }
}
