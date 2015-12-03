using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace CampusNetGuard.Code.Log
{
    class LogImpl : ILog
    {
        object locker = new object();
        public void Debug(string message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        private string errorStr;
        private string iInfoFileName;
        public string InfoFileName
        {
            get
            {
                return iInfoFileName;
            }
            set
            {
                if (Directory.Exists(Path.GetDirectoryName(value)))
                {
                    iInfoFileName = value;
                    WriteToFile(value, string.Format("{0}\t#############Info###############", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")));
                }
                else
                {
                    RunTime.Logger.Error("设置INfo目录出错将恢复默认");
                }
            }
        }


        public void Error(string message)
        {
            if (errorStr != message)
            {
                WriteToFile(RunTime.Directory.AppData + "error.log", message);
                errorStr = message;
            }
        }

        public void Fatal(string message)
        {

        }

        public event EventHandler<string> OnInfoed;
        public void Info(string message)
        {
            if (OnInfoed != null)
            {
                OnInfoed(this, message);
            }
            if (InfoFileName != null)
            {
                WriteToFile(InfoFileName, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss\t") + message);
            }
        }

        public void Warn(string message)
        {
            throw new NotImplementedException();
        }
        private void WriteToFile(string path,string contents)
        {
            try
            {
                lock (this)
                {
                    File.AppendAllText(path, contents + Environment.NewLine, RunTime.Encode);
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error(ex.ToString());
            }
        }
        
    }
}
