using CampusNetGuard.Code;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CampusNetGuard
{
    /// <summary>
    /// UserControlPersonalCenter.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlPersonalCenter : UserControl
    {
        public UserControlPersonalCenter()
        {
            InitializeComponent();
        }
        public override void EndInit()
        {
            base.EndInit();
            if (CmdApi.Instance.Mode == ApiMode.Client)
            {
                CurrentStatus = "正在获取后台台状态";
                CmdApi.Instance.Execute(CmdType.Status);
            }
        }
        public string CurrentUsername
        {
            set
            {
                Dispatcher.Invoke(() => {
                    _CurrentUsername.Text = string.Format("帐号：{0}", value);
                });
            }
        }
        public string CurrentStatus
        {
            set
            {
                Dispatcher.Invoke(() => {
                    //顺带更新按钮
                    _CurrentStatus.Text = string.Format("状态：{0}", value);
                });
            }
        }
        public string CurrentIpAddress
        {
            set
            {
                Dispatcher.Invoke(() => {
                    _CurrentIpAddress.Text = string.Format("IP地址：{0}", value);
                });
            }
        }
        private Storyboard FindStoryboard(string name)
        {
            return FindResource(name) as Storyboard;
        }
        #region 自动更新在线时间
        Timer ElapsedTime;
        private void OpenRefreshElapsedTimer()
        {
            if (ElapsedTime == null)
            {
                ElapsedTime = new Timer((o) => {
                    if (_C_Area.IsVisible && onlineTimeSpan.Ticks != 0)
                    {
                        Dispatcher.Invoke(() => {
                            TimeSpan ts = new TimeSpan(DateTime.Now.Ticks).Subtract(onlineTimeSpan).Duration();
                            _CurrentElapsedTime.Text = string.Format("在线时间：{0}:{1}:{2}", ts.Hours.ToString().PadLeft(2, '0'), ts.Minutes.ToString().PadLeft(2, '0'), ts.Seconds.ToString().PadLeft(2, '0'));
                        });
                    }
                });
            }
            ElapsedTime.Change(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(1));
        }
        private void CloseRefreshElapsedTimer()
        {
            if (ElapsedTime != null)
            {
                ElapsedTime.Dispose();
                ElapsedTime = null;
            }
        }

        #endregion

        private string username;
        private string ipAddress;
        public bool IsWebOnline;
        public bool IsEapOnline;
        private TimeSpan onlineTimeSpan;
        public void UpdateSatus(string username, string ipAddress, bool isEapOn, bool isWebOn, long timeTicks)
        {
            onlineTimeSpan = new TimeSpan(timeTicks);


            if (this.username != username) this.username = CurrentUsername = username;

            if (this.ipAddress != ipAddress) this.ipAddress = ipAddress = CurrentIpAddress = ipAddress;

            if (this.IsEapOnline != isEapOn)
            {
                this.IsEapOnline = isEapOn;
                CurrentStatus = isEapOn ? "在线" : "离线";
                Dispatcher.Invoke(() => {
                    _ButtonEap.Content = isEapOn ? "注销认证" : "登录认证";
                });
            }
            if (this.IsEapOnline)
            {
                OpenRefreshElapsedTimer();
            }
            else
            {
                CloseRefreshElapsedTimer();
            }

            if (IsWebOnline != isWebOn)
            {
                IsWebOnline = isWebOn;
                Dispatcher.Invoke(() => {
                    _ButtonWeb.Content = isWebOn ? "网页注销" : "网页登录";
                });
            }
        }

        private byte LogNum = 0;
        public void AppendLog(Queue<string> logQueue,string log)
        {
            //输出日志
            try
            {
                Dispatcher.Invoke(() => {
                    if (LogNum > 10 && LogNum < 21)
                    {
                        _LogTextBox.AppendText(log + "\n");
                    }
                    else
                    {
                        if (LogNum == 21)
                        {
                            _LogTextBox.Document.Blocks.Clear();
                            _LogTextBox.AppendText("\n");
                            LogNum = 0;
                        }
                        while (logQueue.Count > 0)
                        {
                            _LogTextBox.AppendText(logQueue.Dequeue() + "\n");
                        }
                    }
                    _LogTextBox.ScrollToEnd();
                    LogNum++;
                });
            }
            catch (TaskCanceledException)
            {
                //路过
            }
        }

        private void ClickWeb(object sender, RoutedEventArgs e)
        {
            CmdApi.Instance.Execute(CmdType.Status);
            if (RunTime.ConfigManager.Config.WebUsername == null || RunTime.ConfigManager.Config.WebPassword == null)
            {
                MessageBox.Show("网页认证未配置,请到设置界面设置!");
                return;
            }
            if (IsWebOnline)
            {
                CmdApi.Instance.Execute(CmdType.WebLogout);
            }
            else
            {
                CmdApi.Instance.Execute(CmdType.WebLogin);
            }
        }
        
        private void ClickCheckNetwork(object sender, RoutedEventArgs e)
        {
            CmdApi.Instance.Execute(CmdType.CheckNetwork);
        }
        private void ClickLog(object sender, RoutedEventArgs e)
        {
            if (_LogTextBox.IsVisible)
            {
                _ButtonLog.Content = "显示日志";
                BeginStoryboard(FindStoryboard("StoryboardLogBoxHide"));
            }
            else
            {
                _ButtonLog.Content = "隐藏日志";
                BeginStoryboard(FindStoryboard("StoryboardLogBoxShow"));
            }
        }

        private void ClickEap(object sender, RoutedEventArgs e)
        {
            CmdApi.Instance.Execute(CmdType.Status);
            if (IsEapOnline)
            {
                CmdApi.Instance.Execute(CmdType.EapStop);
            }
            else
            {
                CmdApi.Instance.Execute(CmdType.EapStart);
            }
        }
    }
}
