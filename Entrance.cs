using System;
using System.ServiceProcess;
using CampusNetGuard.Code;
using CampusNetGuard.Libraries;
using System.Security.Principal;
using System.Configuration.Install;

namespace CampusNetGuard
{
    public class Entrance
    {

        /// <summary>
        /// Application Entry Point.
        /// </summary>
        [System.STAThreadAttribute()]
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                if (GlobalStatus.IsSystemAccout)
                {
                    #region 服务
                    ServiceBase.Run(new ServiceBase[] { new CampusNetGuardSvc() });
                    #endregion
                }
                else
                {

                    #region 应用程序
                    bool isFirstInstance;
                    using (var mtx = new System.Threading.Mutex(true, RunTime.AppName + "a55saa", out isFirstInstance))
                    {
                        if (isFirstInstance)
                        {
                            App app = new App();
                            app.InitializeComponent();
                            app.Run();
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("此程序已打开!");
                        }
                    }
                    #endregion

                }
            }
            else
            {
                new Entrance().Run(args);
            }
        }
        public string ThisExe
        {
            get
            {
                return GetType().Assembly.Location;
            }
        }

        
        public void Run(string[] args)
        {
            switch (args[0].ToLower())
            {
                case "s/i"://安装服务
                    if (CommandLine.IsAdministrator)
                    {
                        new ServiceUtils().InstallService(ThisExe);
                    }
                    else
                    {
                        CommandLine.RunAsAdministrator(ThisExe, args);
                    }
                    break;
                case "s/u"://卸载服务
                    if (CommandLine.IsAdministrator)
                    {
                        new ServiceUtils().UnInstallService(ThisExe);
                    }
                    else
                    {
                        CommandLine.RunAsAdministrator(ThisExe, args);
                    }
                    break;
                default:
                    Console.WriteLine("Unknow command");
                    break;

            }
        }
    }
}
