using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;
using System.Windows;
using System.IO;
using CampusNetGuard;

using CampusNetGuard.Code;

namespace CampusNetGuard
{
    partial class App: Application
    {
        private System.Windows.Forms.NotifyIcon systemTray;
        public App()
        {
            //异常捕捉
            DispatcherUnhandledException += (o, e) => {
                RunTime.Logger.Error("###########WPF#############");
                RunTime.Logger.Error(e.Exception.ToString());
                e.Handled = true;
            };

            systemTray = new System.Windows.Forms.NotifyIcon();
            systemTray.Icon = Res.Icon;
            systemTray.ContextMenu = new System.Windows.Forms.ContextMenu(new System.Windows.Forms.MenuItem[]{
                new System.Windows.Forms.MenuItem("显示", AppShow),
                new System.Windows.Forms.MenuItem("帮助", AppHelp),
                new System.Windows.Forms.MenuItem("关于", AppAbout),
                new System.Windows.Forms.MenuItem("退出", AppExit)}
            );
            systemTray.Click += AppShow;
            systemTray.Visible = true;
        }
        private void AppShow(object sender, EventArgs e)
        {
            if (MainWindow == null) return;
            if (MainWindow.IsVisible)
            {
                if (MainWindow.WindowState == WindowState.Minimized)
                {
                    MainWindow.WindowState = WindowState.Normal;
                }
                else
                {
                    MainWindow.Topmost = !(MainWindow.Topmost = true);
                }
            }
            else 
            {
                MainWindow.BeginStoryboard(MainWindow.FindResource("StoryboardWindowShow") as Storyboard);
            }
        }
        private void AppHelp(object sender, EventArgs e)
        {
            iMessageBox.Show("帮助", Res.帮助文档,400,500);
        }

        private void AppAbout(object sender, EventArgs e)
        {
            iMessageBox.Show(string.Format("关于{0}" , RunTime.AppName),string.Format("{0}({1})\n说明:自动化端口认证,和网页认证。\rQQ:21954595\rCopyright © Mokeyish 2015", RunTime.AppName, RunTime.Version,330,280));
        }
        
        private void AppExit(object sender, EventArgs e)
        {
            try
            {
                //如果2秒没有退出程序即强制关闭
                RunTime.DelayRunMethod(2000, (o) => { Environment.Exit(0); });
                CmdApi.Instance.Close();
                if (MainWindow != null)
                {
                    if (MainWindow.IsVisible)
                    {
                        Storyboard sb = MainWindow.FindResource("StoryboardWindowClose") as Storyboard;
                        sb.Completed += (xo, xe) => {
                            MainWindow.Close();
                        };
                        MainWindow.BeginStoryboard(sb);
                    }
                    else
                    {
                        MainWindow.Close();
                    }
                }
            }
            catch (Exception)
            {
                Environment.Exit(0);
            }
        }
    }
}
