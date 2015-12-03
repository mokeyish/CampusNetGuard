using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using CampusNetGuard.Code.Log;
using CampusNetGuard.Code.Libraries;

namespace CampusNetGuard.Code
{
    /// <summary>
    /// 运行时所必需的信息
    /// 以及向磁盘输出，运行信息，以及错误日志
    /// </summary>
    public static class RunTime
    {


        public const string AppName = "CampusNetGuard";
        public const string Version = "1.0.2";

        public static ILog Logger = LogManager.GetLogger("");


        private static AppDirectoryInfo _Directory;
        public static AppDirectoryInfo Directory { get { return _Directory == null ? _Directory = new AppDirectoryInfo() : _Directory; } }
        public static readonly Encoding Encode;
        static RunTime()
        {
            Encode= Encoding.UTF8;
            ConfigManager = new ConfigManager(string.Format("{0}{1}.{2}", Directory.AppData, "Config", "ini"));
            ConfigManager.Read();
        }
        public static ConfigManager ConfigManager { get; private set; }

        public static void ClearMemory()
        {
            if (ConfigManager.Config.IsCompactMemory)
            {
                CampusNetGuard.Libraries.NativeMethods.ClearMemory();
            }
        }

        /// <summary>
        /// 延迟执行方法
        /// </summary>
        /// <param name="millisecondsTimeout">延迟时间</param>
        /// <param name="callback">执行方法</param>
        public static void DelayRunMethod(int millisecondsTimeout, TimerCallback callback)
        {
            new Timer(callback, null, millisecondsTimeout, 0);
        }
    }


    public class AppDirectoryInfo
    {

        private string _AppData;
        public string AppData { get { return _AppData == null ? GetPath(_AppData = Current + "AppData" + System.IO.Path.DirectorySeparatorChar) : _AppData; } }


        public string Current { get { return AppDomain.CurrentDomain.BaseDirectory; } }



        private string _Temp;
        public string Temp { get { return _Temp == null ? GetPath(_Temp = System.IO.Path.GetTempPath() + RunTime.AppName + System.IO.Path.DirectorySeparatorChar) : _Temp; } }


        private string GetPath(string p)
        {
            if (!System.IO.Directory.Exists(p)) System.IO.Directory.CreateDirectory(p);
            return p;
        }
    }
}
