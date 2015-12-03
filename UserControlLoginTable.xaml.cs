using CampusNetGuard.Code;
using CampusNetGuard.Libraries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Windows.Controls;
using System.IO;
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
    /// UserControlLoginTable.xaml 的交互逻辑
    /// </summary>
    public partial class UserControlLoginTable : UserControl
    {
        public bool IsFirstRun { get; set; }
        public UserControlLoginTable()
        {
            InitializeComponent();
            ReadHistoryAccount();
        }

        /// <summary>
        /// 更新登录中的进度
        /// </summary>
        public void UpdateRateOfProgress()
        {
            //如果在登录中，迭代进度
            if (_B_Area.IsVisible)
            {
                Dispatcher.Invoke(() => {
                    Brush b = _B_Area_P1.Fill;
                    _B_Area_P1.Fill = _B_Area_P3.Fill;
                    _B_Area_P3.Fill = _B_Area_P2.Fill;
                    _B_Area_P2.Fill = b;
                });
                timeoutFlag += 3;
    }
        }
        /// <summary>
        /// 返回登录界面并,显示提示信息
        /// </summary>
        /// <param name="str"></param>
        public void ShowNote(string str)
        {
            Dispatcher.Invoke(() => {
                WPFBaseUtils.HideUIElement(Dispatcher, _B_Area);
                WPFBaseUtils.ShowUIElement(Dispatcher, _A_Area);
                if (!_note.IsVisible) _note.Visibility = Visibility.Visible;
                _note.Text = str;
            });
        }
        private int timeoutFlag = 25;
        private bool isSuccess = false;
        private async void ShowProgress()
        {
            await Task.Run(() => {
                WPFBaseUtils.HideUIElement(Dispatcher, _A_Area);
                WPFBaseUtils.ShowUIElement(Dispatcher, _B_Area);
                int i = 3;
                int j = 0;
                while (!isSuccess&&_B_Area.IsVisible)
                {
                    if (j++ == timeoutFlag)
                    {
                        WPFBaseUtils.ShowUIElement(Dispatcher, _A_Area);
                        ShowNote("超时,服务器无响应!");
                    }
                    switch (i)
                    {
                        case 3:
                            Dispatcher.Invoke(() => {
                                _B_Note.Text = "登录中";
                            });
                            break;
                        default:
                            if (i == 7)
                            {
                                i = 3;
                                goto case 3;
                            }
                            else
                            {
                                Dispatcher.Invoke(() => {
                                    _B_Note.Text = "登录中".PadRight(i, '.');
                                });
                            }
                            break;
                    }
                    Thread.Sleep(750);
                    i++;
                }
            });
        }
        private void ClickLogin(object sender, RoutedEventArgs e)
        {
            if (RunTime.ConfigManager.Config.Username == null && acc.Text == "201237060113")
            {
                MessageBox.Show("你是傻逼吗？还用我的软件.");
            }
            // 在此处添加事件处理程序实现。
            if (acc.Text.Length == 0 || pwd.Password.Length == 0)
            {
                ShowNote("账号或密码不能为空!");
            }
            else
            {
                ShowProgress();
                HistoryAccount.Add(acc.Text, RmbPwd.IsChecked == true ? pwd.Password : string.Empty);
                CmdApi.Instance.Execute(CmdType.EapStart, string.Format("{0}\t{1}\t{2}\t{3}", acc.Text, pwd.Password, RmbPwd.IsChecked == true ? "1" : "1", autoLogin.IsChecked == true ? "1" : "0"));

            }
        }

        #region 历史帐号
        private readonly string filename = RunTime.Directory.AppData + "UserDataInfo.ini";
        private ListKeyValue historyAccount;
        private ListKeyValue HistoryAccount
        {
            get
            {
                ReadHistoryAccount();
                return historyAccount;
            }
        }

        //异步读取历史帐号
        private async void ReadHistoryAccount()
        {
            await Task.Run(() => {
                //
                if (historyAccount == null)
                {
                    historyAccount = new ListKeyValue();
                    historyAccount.SetTitile("历史账号信息列表", "账号", "密码");
                    #region 从文件中读取历史账号
                    if (File.Exists(filename))
                    {
                        try
                        {
                            using (Base64Crypt v = new Base64Crypt())
                            {
                                historyAccount.Crypt = v;
                                historyAccount.Load(File.ReadAllText(filename, RunTime.Encode));
                                historyAccount.Crypt = null;
                            }
                        }
                        catch (Exception)
                        {
                            RunTime.Logger.Error("读取历史账户信息出错");
                        }
                    }

                    #endregion

                    Dispatcher.Invoke(() => {
                        //绑定到显示控件
                        acc.ItemsSource = historyAccount.ListKey;
                        if (historyAccount.Count != 0)
                        {
                            var x = historyAccount[0];
                            acc.Text = x.Key;
                            pwd.Password = x.Value;
                        }
                        if (IsFirstRun && RunTime.ConfigManager.Config.IsAutoEap && RunTime.ConfigManager.Config.Username != null && RunTime.ConfigManager.Config.Password != null)
                        {
                            CmdApi.Instance.Execute(CmdType.EapStart);
                            IsFirstRun = false;
                        }
                        WaterMaskShow(this, null);
                        //刷新列表
                        RefreshUsernameList(historyAccount, EventArgs.Empty);
                        //注册事件
                        historyAccount.OnChanged += RefreshUsernameList;
                    });
                }
            });

        }
        private void SaveHistoryAccount()
        {

            if (historyAccount != null && historyAccount.IsChanged)
            {
                //如果有改变就保存当前历史帐号
                try
                {
                    using (Base64Crypt v = new Base64Crypt())
                    {
                        historyAccount.Crypt = v;
                        File.WriteAllText(filename, historyAccount.ToString(), RunTime.Encode);
                        historyAccount.Crypt = null;
                    }
                }
                catch (Exception)
                {
                    RunTime.Logger.Error("保存历史账户信息出错");
                }
                historyAccount.OnChanged -= RefreshUsernameList;
                historyAccount = null;
            }
        }
        private void RefreshUsernameList(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                acc.Items.Refresh();
            });
        }
        #endregion

        private void UserNameSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 在此处添加事件处理程序实现。
            if (acc.SelectedIndex != -1 && acc.SelectedIndex < HistoryAccount.Count)
            {
                pwd.Password = HistoryAccount[acc.SelectedIndex].Value;
                if (pwd.Password == string.Empty)
                {
                    RmbPwd.IsChecked = false;
                }
            }
            else
            {
                pwd.Password = string.Empty;
            }
            WaterMaskShow(pwd, null);
        }

        //隐藏无条件
        private void WaterMaskHide(object sender, RoutedEventArgs e)
        {
            // 在此处添加事件处理程序实现。
            if (sender.Equals(acc))
            {
                accWaterMask.Visibility = Visibility.Collapsed;
            }
            else if (sender.Equals(pwd))
            {
                pwdWaterMask.Visibility = Visibility.Collapsed;
            }
            if (_note.IsVisible) _note.Visibility = Visibility.Collapsed;
        }
        //显示有条件
        private void WaterMaskShow(object sender, RoutedEventArgs e)
        {
            if (this.Equals(sender))
            {
                WaterMaskShow(acc, null);
                WaterMaskShow(pwd, null);
                return;
            }
            // 在此处添加事件处理程序实现。
            if (sender.Equals(acc))
            {
                if (acc.Text.Length == 0)
                {
                    accWaterMask.Visibility = Visibility.Visible;
                }
                else
                {
                    accWaterMask.Visibility = Visibility.Collapsed;

                }
            }
            else if (sender.Equals(pwd))
            {
                if (pwd.Password.Length == 0)
                {
                    pwdWaterMask.Visibility = Visibility.Visible;
                }
                else
                {
                    pwdWaterMask.Visibility = Visibility.Collapsed;
                }
            }

        }
        public void Dispose()
        {
            isSuccess = true;
            SaveHistoryAccount();
        }

        private void accPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Back)
            {
                if (acc.Text.Length >= 20 || !WPFBaseUtils.isNumbericInput(e.Key))
                {
                    e.Handled = true;
                }
            }
        }

        private void pwdPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                case Key.ImeProcessed:
                    e.Handled = true;
                    break;
                default:
                    break;
            }
        }

        private void CheckedAutoLogin(object sender, RoutedEventArgs e)
        {
            RmbPwd.IsChecked = true;
        }

        private void UnCheckedRmdPwd(object sender, RoutedEventArgs e)
        {
            autoLogin.IsChecked = false;
        }
    }
}
