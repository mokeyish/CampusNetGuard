using CampusNetGuard.Code;
using CampusNetGuard.Libraries;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CampusNetGuard
{
    /// <summary>
    /// UserControlSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlSetting : UserControl
    {
        public UserControlSetting()
        {
            InitializeComponent();

            //自动运行
            _CheckBoxAutoRun.IsChecked = RunTime.ConfigManager.Config.IsAutoRun;

            //自动认证
            _CheckBoxAutoEap.IsChecked = RunTime.ConfigManager.Config.IsAutoEap;

            //自动检测
            _CheckBoxAutoCheck.IsChecked = RunTime.ConfigManager.Config.IsAutoCheck;

            //自动检测时间
            _checkTimeInterval.Text = RunTime.ConfigManager.Config.CheckTimeInterval.ToString();


            //服务
            _CheckBoxAutoSvc.IsChecked = new ServiceUtils().IsServiceExisted(CampusNetGuardSvc.iServiceName);

            //开始服务时间
            _netStartTime.Text = RunTime.ConfigManager.Config.NetStartTime == null ? string.Empty : RunTime.ConfigManager.Config.NetStartTime;

            //自动登录天翼
            _CheckBoxAutoTianYi.IsChecked = RunTime.ConfigManager.Config.IsAutoWeb;
            if (RunTime.ConfigManager.Config.IsAutoWeb)
            {
                _AreaTianYi.Visibility = Visibility.Visible;
            }
            else
            {
                _AreaTianYi.Visibility = Visibility.Collapsed;
            }
            //赋值天翼账户
            _webAcc.Text = RunTime.ConfigManager.Config.WebUsername;
            _webPwd.Password = RunTime.ConfigManager.Config.WebPassword;

            //自定义登录地址
            _CheckBoxUserDefined.IsChecked = !RunTime.ConfigManager.Config.IsDefaultWebUri;
            if (!RunTime.ConfigManager.Config.IsDefaultWebUri)
            {
                _AreaWebUserDefined.Visibility = Visibility.Visible;
            }
            else
            {
                _AreaWebUserDefined.Visibility = Visibility.Collapsed;

            }
            //赋值自定义网址
            if (RunTime.ConfigManager.Config.WebUris != null && RunTime.ConfigManager.Config.WebUris.Length == 2)
            {
                _webUri0.Text = RunTime.ConfigManager.Config.WebUris[0];
                _webUri1.Text = RunTime.ConfigManager.Config.WebUris[1];
            }
        }
        public event EventHandler OnDisposed;

        private void CheckedAutoTianYi(object sender, RoutedEventArgs e)
        {
            _AreaTianYi.Visibility = Visibility.Visible;
        }

        private void UnCheckedAutoTianYi(object sender, RoutedEventArgs e)
        {
            _AreaTianYi.Visibility = Visibility.Collapsed;
        }

        private void CheckedUserDefined(object sender, RoutedEventArgs e)
        {
            _AreaWebUserDefined.Visibility = Visibility.Visible;
        }

        private void UnCheckedUserDefined(object sender, RoutedEventArgs e)
        {
            _AreaWebUserDefined.Visibility = Visibility.Collapsed;
        }


        private async void ServiceOperate(bool isInstallOrUnInstall)
        {
            await Task.Run(() => {

                ServiceUtils su = new ServiceUtils();
                if (isInstallOrUnInstall)
                {
                    //安装服务
                    if (!su.IsServiceExisted(CampusNetGuardSvc.iServiceName))
                    {
                        CommandLine.Run(su.GetType().Assembly.Location, "s/i");
                        if (su.IsServiceExisted(CampusNetGuardSvc.iServiceName))
                        {
                            iMessageBox.Show("提示", "服务安装成功!");
                        }
                        else
                        {
                            iMessageBox.Show("提示", "服务安装失败!");
                        }
                    }
                }
                else
                {
                    //卸载服务
                    if (su.IsServiceExisted(CampusNetGuardSvc.iServiceName))
                    {
                        CommandLine.Run(su.GetType().Assembly.Location, "s/u");
                        if (su.IsServiceExisted(CampusNetGuardSvc.iServiceName))
                        {
                            iMessageBox.Show("提示", "服务卸载失败!");
                        }
                        else
                        {
                            iMessageBox.Show("提示", "服务卸载成功!");

                        }
                    }
                }
            });
        }
        private async void ClickConfirm(object sender, RoutedEventArgs e)
        {
            bool __AutoRun = _CheckBoxAutoRun.IsChecked == true;
            bool __AutoEap = _CheckBoxAutoEap.IsChecked == true;
            bool __AutoCheck = _CheckBoxAutoCheck.IsChecked == true;
            bool __AutoSvc = _CheckBoxAutoSvc.IsChecked == true;
            int __TimeInterval = Convert.ToInt32(_checkTimeInterval.Text);
            string __NetStartTime = _netStartTime.Text;

            bool __AutoTianYi = _CheckBoxAutoTianYi.IsChecked == true;
            string __webAcc = _webAcc.Text;
            string __webPwd = _webPwd.Password;

            bool __UserDefined = _CheckBoxUserDefined.IsChecked == true;
            string __webUri0 = _webUri0.Text;
            string __webUri1 = _webUri1.Text;

            await Task.Run(() => {
                //

                bool isChanged;
                //自动运行
                isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsAutoRun, __AutoRun);

                //自动认证
                isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsAutoEap, __AutoEap) || isChanged;

                //自动检测
                isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsAutoCheck, __AutoCheck) || isChanged;

                //检测时间
                isChanged = Utils.ComparisonAssign<int>(ref RunTime.ConfigManager.Config.CheckTimeInterval, __TimeInterval) || isChanged;


                //服务操作,在后面

                //开网时间
                isChanged = Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.NetStartTime, __NetStartTime) || isChanged;



                //如果是要登录天翼，保存当前账户
                if (__AutoTianYi && !string.IsNullOrEmpty(__webAcc) && !string.IsNullOrEmpty(__webPwd))
                {

                    //天翼账号自动登录
                    isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsAutoWeb, true) || isChanged;

                    isChanged = Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.WebUsername, __webAcc) || isChanged;

                    isChanged = Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.WebPassword, __webPwd) || isChanged;

                }
                else
                {
                    //天翼账号自动登录
                    isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsAutoWeb, false) || isChanged;

                }


                //如果是要自定义网页登陆地址，保存当前地址
                if (__UserDefined && !string.IsNullOrEmpty(__webUri0) && !string.IsNullOrEmpty(__webUri1))
                {
                    //自定义网页登陆地址
                    isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsDefaultWebUri, false) || isChanged;

                    if (RunTime.ConfigManager.Config.WebUris == null)
                    {
                        isChanged = true;
                        RunTime.ConfigManager.Config.WebUris = new string[] { __webUri0, __webUri1 };
                    }
                    else
                    {
                        //
                        isChanged = Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.WebUris[0], __webUri0) || isChanged;

                        isChanged = Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.WebUris[1], __webUri1) || isChanged;

                    }
                }
                else
                {
                    //自定义网页登陆地址
                    isChanged = Utils.ComparisonAssign<bool>(ref RunTime.ConfigManager.Config.IsDefaultWebUri, true) || isChanged;

                }
                if (isChanged)
                {
                    RunTime.Logger.Debug("改变了");
                    //保存配置
                    if (CmdApi.Instance.Mode == ApiMode.Client)
                    {
                        CmdApi.Instance.Execute(CmdType.ReadConfig);
                    }
                    RunTime.ConfigManager.Save();
                }

                
                if (__AutoSvc)
                {
                    //安装服务
                    if (!GlobalStatus.IsServiceExisted)
                    {
                        CommandLine.Run(this.GetType().Assembly.Location, "s/i");
                        iMessageBox.Show("提示", GlobalStatus.IsServiceExisted ? "服务安装成功!" : "服务安装失败!");
                    }
                }
                else
                {
                    //卸载服务
                    if (GlobalStatus.IsServiceExisted)
                    {
                        CommandLine.Run(this.GetType().Assembly.Location, "s/u");
                        iMessageBox.Show("提示", GlobalStatus.IsServiceExisted ? "服务卸载失败!" : "服务卸载成功!");
                    }
                }
                if (OnDisposed != null) OnDisposed(this, EventArgs.Empty);


            });

        }

        private void ClickCancel(object sender, RoutedEventArgs e)
        {
            if (OnDisposed != null) OnDisposed(this, EventArgs.Empty);
        }

        private void CheckedAutoSvc(object sender, RoutedEventArgs e)
        {
            _CheckBoxAutoEap.IsChecked = true;
            _Area_NetStartTime.Visibility = Visibility.Visible;
            if (_CheckBoxAutoCheck.IsChecked != true)
            {
                _Area_TimeInput.Visibility = Visibility.Visible;
            }
        }

        private void UnCheckedAutoSvc(object sender, RoutedEventArgs e)
        {
            _Area_NetStartTime.Visibility = Visibility.Collapsed;
            if (_CheckBoxAutoCheck.IsChecked != true)
            {
                _Area_TimeInput.Visibility = Visibility.Collapsed;
            }
        }

        private void CheckedAutoCheck(object sender, RoutedEventArgs e)
        {
            _Area_checkTime.Visibility = Visibility.Visible;
            if (_CheckBoxAutoSvc.IsChecked != true)
            {
                _Area_TimeInput.Visibility = Visibility.Visible;
            }

        }

        private void UnCheckedAutoCheck(object sender, RoutedEventArgs e)
        {
            _Area_checkTime.Visibility = Visibility.Collapsed;
            if (_CheckBoxAutoSvc.IsChecked != true)
            {
                _Area_TimeInput.Visibility = Visibility.Collapsed;
            }

        }
        private void CheckTimeIntervalPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back)
            {
                if (_checkTimeInterval.Text.Length>=3|| !WPFBaseUtils.isNumbericInput(e.Key))
                {
                    e.Handled = true;
                }
            }
        }
        

        private void NetStartTimePreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back)
            {
                if (_netStartTime.Text.Length >= 5 ||!WPFBaseUtils.isNumbericInput(e.Key))
                {
                    e.Handled = true;
                }
            }
        }

        private void NetStartTimePreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back)
            {
                //

                byte m = 0;
                switch (_netStartTime.Text.Length)
                {
                    case 1:
                        m = Convert.ToByte(_netStartTime.Text);
                        if (m > 2)
                        {
                            _netStartTime.Text = "0" + m+":";
                        }
                        break;
                    case 2:
                        m = Convert.ToByte(_netStartTime.Text.Substring(0, 2));
                        if (m > 23)
                        {
                            _netStartTime.Text = "00";
                        }
                        _netStartTime.AppendText(":");
                        break;
                    case 4:
                        m = Convert.ToByte(_netStartTime.Text.Substring(3, 1));
                        if (m > 5)
                        {
                            _netStartTime.Text = _netStartTime.Text.Substring(0, 3) + "0" + m;
                        }
                        break;
                    case 5:
                        m = Convert.ToByte(_netStartTime.Text.Substring(3, 2));
                        if (m > 59)
                        {
                            _netStartTime.Text = _netStartTime.Text.Substring(0, 3) + "00";
                        }
                        break;
                }
                _netStartTime.SelectionStart = _netStartTime.Text.Length;

            }
        }
    }
}
