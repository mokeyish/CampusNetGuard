using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;
using System.Diagnostics;
using CampusNetGuard;
using CampusNetGuard.Libraries;
using CampusNetGuard.Code;
using System.IO;
using System.ComponentModel;
using System.ServiceProcess;

namespace CampusNetGuard
{


    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class UIMain : Window
    {

        enum Pages : byte
        {
            Default,
            /// <summary>
            /// 登录界面
            /// </summary>
            LoginTable,
            /// <summary>
            /// 个人中心界面
            /// </summary>
            PersonalCenter,
            /// <summary>
            /// 设置界面
            /// </summary>
            Setting,
        }
        private Pages CurrentPage;

        public readonly Queue<string> iLogQueue = new Queue<string>();
        public UIMain()
        {
            InitializeComponent();
            AsyncInitialize();
            ShowPage(Pages.LoginTable);
            FindStoryboard("StoryboardWindowClose").Completed += ClearMemory;
        }
        private async void AsyncInitialize()
        {
            await Task.Run(() => {
                CmdApi.Instance.OnModeChanged += (o, e) => { Dispatcher.Invoke(() => { iTitle.Text = string.Format("校园网客户端({0})", CmdApi.Instance.Mode.ToString()[0]); }); };
                CmdApi.Instance.OnOutputed += OutputedHandler;
                CmdApi.Instance.Open();
                if (CmdApi.Instance.Mode == ApiMode.Client)
                {
                    ShowPage(Pages.PersonalCenter);
                }
            });
        }
        private bool isFirstUserControlSetting = true;
        private UserControlSetting iUserControlSetting;
        private UserControlLoginTable iUserControlLoginTable;
        private UserControlPersonalCenter iUserControlPersonalCenter;
        private void ShowPage(Pages page)
        {
            switch (page)
            {
                case Pages.LoginTable:
                    CurrentPage = Pages.LoginTable;
                    Dispatcher.Invoke(() => {
                        if (iUserControlPersonalCenter != null)
                        {
                            _Area_Show.Children.Remove(iUserControlPersonalCenter);
                        }
                        if (iUserControlLoginTable == null)
                        {
                            iUserControlLoginTable = new UserControlLoginTable() { IsFirstRun = CurrentPage == Pages.Default };
                        }
                        if (_Area_Show.Children.Contains(iUserControlLoginTable))
                        {
                            WPFBaseUtils.ShowUIElement(Dispatcher, iUserControlLoginTable);
                        }
                        else
                        {
                            _Area_Show.Children.Add(iUserControlLoginTable);
                        }
                    });
                    break;
                case Pages.PersonalCenter:
                    if (CurrentPage == Pages.PersonalCenter) break;
                    Dispatcher.Invoke(() => {
                        if (CurrentPage == Pages.LoginTable && iUserControlLoginTable != null)
                        {
                            if (_Area_Show.Children.Contains(iUserControlLoginTable)) _Area_Show.Children.Remove(iUserControlLoginTable);
                            iUserControlLoginTable.Dispose();
                            iUserControlLoginTable = null;
                        }
                        //
                        if (iUserControlPersonalCenter == null) iUserControlPersonalCenter = new UserControlPersonalCenter();
                        if (_Area_Show.Children.Contains(iUserControlPersonalCenter))
                        {
                            WPFBaseUtils.ShowUIElement(Dispatcher, iUserControlPersonalCenter);
                        }
                        else
                        {
                            _Area_Show.Children.Add(iUserControlPersonalCenter);
                        }
                    });
                    iUserControlPersonalCenter.AppendLog(iLogQueue, string.Empty);
                    CurrentPage = Pages.PersonalCenter;
                    break;

                case Pages.Setting:
                    //初始化界面数据
                    iUserControlSetting = new UserControlSetting();
                    iUserControlSetting.OnDisposed += DisposeUserControl;
                    _Page_Setting.Children.Add(iUserControlSetting);
                    if (isFirstUserControlSetting)
                    {
                        FindStoryboard("StoryboardSettingTurnToDefault").Completed += (x, y) => {
                            _Page_Setting.Children.Remove(iUserControlSetting);
                            iUserControlSetting = null;
                            RunTime.ClearMemory();
                        };
                        isFirstUserControlSetting = true;
                    }
                    BeginStoryboard(FindStoryboard("StoryboardTurnDefaultToSetting"));
                    break;
            }
        }


        private void DisposeUserControl(object o, EventArgs e)
        {
            if (iUserControlSetting == o)
            {
                if (iUserControlSetting != null)
                {
                    WPFBaseUtils.BeginStoryboard(FindStoryboard("StoryboardSettingTurnToDefault"));
                }
                return;
            }
        }
        
        
        private Storyboard FindStoryboard(string name)
        {
            return FindResource(name) as Storyboard;
        }
        private void ClearMemory(object o,EventArgs e)
        {
            RunTime.ClearMemory();
        }
        private void WindowMinimied(object sender, RoutedEventArgs e)
        {
            // 在此处添加事件处理程序实现。
            this.WindowState = WindowState.Minimized;
        }
        private void WindowDrag(object sender, MouseButtonEventArgs e)
        {
            // 在此处添加事件处理程序实现。
            this.DragMove();
        }
        private void UpdateSatus(string username,string ipAddress,bool isEapOn,bool isWebOn,long timeTicks)
        {
            //当前页不为个人中心,需要切换界面
            if (CurrentPage != Pages.PersonalCenter)
            {
                if (isEapOn)
                {
                    ShowPage(Pages.PersonalCenter);
                }
                else
                {
                    ShowPage(Pages.LoginTable);
                }
            }
            if (iUserControlPersonalCenter != null)
            {
                iUserControlPersonalCenter.UpdateSatus(username, ipAddress, isEapOn, isWebOn, timeTicks);
            }

        }
        

        private void OutputedHandler(object sender, string x)
        {
            RunTime.Logger.Debug(string.Format("{0}{1}","桌面端####",x));
            if (string.IsNullOrEmpty(x)) return;
            if (CmdHelper.IsCmd(x))
            {
                CmdType c;
                string p;
                CmdHelper.Parse(x, out c, out p);

                string[] t;

                //命令结果
                switch (c)
                {
                    case CmdType.Status:
                        //命令结果
                        t = p.Split('\t');
                        if (t.Length == 5)
                        {
                            UpdateSatus(t[0], t[1], t[2] == "1", t[3] == "1", Convert.ToInt64(t[4]));
                        }
                        break;
                    case CmdType.EapStart:
                        switch (p.Substring(0, 3))
                        {
                            case "100"://成功
                                ShowPage(Pages.PersonalCenter);
                                break;
                            case "101"://用户名不存在
                                if (iUserControlLoginTable != null) iUserControlLoginTable.ShowNote("用户名不存在!");
                                break;
                            case "102"://密码错误
                                if (iUserControlLoginTable != null) iUserControlLoginTable.ShowNote("密码错误!");
                                break;
                            case "200":

                                break;
                            case "888":
                                if (iUserControlLoginTable != null) iUserControlLoginTable.UpdateRateOfProgress();
                                break;
                            case "207"://
                                if (iUserControlLoginTable != null) iUserControlLoginTable.ShowNote("此时段没有接入的权限!");
                                if (iUserControlPersonalCenter != null)
                                {
                                    iUserControlPersonalCenter.CurrentUsername= p.Substring(3);
                                    iUserControlPersonalCenter.CurrentStatus= "此时段没有接入的权限";
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case CmdType.EapStop:
                        if (p == "100")
                        {
                            ShowPage(Pages.LoginTable);
                        }
                        break;
                }

            }
            else
            {
                //日志
                iLogQueue.Enqueue(x = string.Format("{0}  {1}", DateTime.Now.ToString("HH:mm:ss"), x));
                if (iUserControlPersonalCenter != null) iUserControlPersonalCenter.AppendLog(iLogQueue,x);
            }
        }
        

        #region 设置界面
        private void ClickSettings(object sender, RoutedEventArgs e)
        {
            ShowPage(Pages.Setting);
        }

        #endregion



        


    }
}
