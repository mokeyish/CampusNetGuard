using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using CampusNetGuard.Libraries;

namespace CampusNetGuard.Code
{

    class ControlCore
    {
        private enum NetworkState : byte
        {
            /// <summary>
            /// 未设置
            /// </summary>
            None,
            /// <summary>
            /// 正常
            /// </summary>
            WebNormal,
            WebAbnormal,
            EapNormal,
            EapAbnormal,

        }

        public int EapRetryTimes = 0;
        public bool IsRememberPassword = false;
        public bool CmdWebOnline = false;
        public bool CmdEapOnline = false;

        public bool isEAPoLActive = false;
        private NetworkState iNetworkState;
        private EAPClient _iEAPClient;
        private EAPClient iEAPClient
        {
            get
            {
                if (_iEAPClient == null) _iEAPClient = new EAPClient(RunTime.Logger, RunTime.ConfigManager.Config.PcapName);
                return _iEAPClient;
            }
            set
            {
                _iEAPClient = value;
            }
        }

        private WebAuth _iWebAuth;
        private WebAuth iWebAuth { get { if (_iWebAuth == null) _iWebAuth = new WebAuth(); return _iWebAuth; } set { _iWebAuth = value; } }
        
        public ControlCore() { }
        
        private void Configure()
        {
            ConfigTimerChecker();
        }
        private async void HandlerCenter(object sender,byte flag)
        {
            await Task.Run(() => {

                //
                Action EAPContinue = () => {
                    if (CmdEapOnline)
                    {
                        iEAPClient.Login();
                    }
                    else
                    {
                        iEAPClient.Logout();
                    }
                };
                if (sender == iEAPClient)
                {
                    EAPoLAuthType t = (EAPoLAuthType)flag;
                    switch (t)
                    {
                        case EAPoLAuthType.GetIPAddressSuccess:
                            if (iEAPClient.Interface.GatewayAddress == IPAddress.Any)
                            {
                                iEAPClient.GetIPAddressFromServer();
                                break;
                            }
                            if (RunTime.ConfigManager.Config.IsAutoWeb)
                            {
                                SendStatus(NetworkState.EapNormal);
                                IntervalExecute(CmdType.WebLogin);
                            }
                            else
                            {
                                SendStatus(NetworkState.EapNormal);
                            }
                            break;
                        case EAPoLAuthType.Verifing:
                            RunTime.ClearMemory();
                            break;
                        case EAPoLAuthType.Running:
                            Output(CmdType.EapStart, "888");
                            break;
                        case EAPoLAuthType.Success:
                            Output(CmdType.EapStart, t.ToString());
                            EapRetryTimes = 0;
                            SendStatus(NetworkState.EapNormal);

                            if (iEAPClient.Interface.GatewayAddress == IPAddress.Any)
                            {
                                iEAPClient.GetIPAddressFromServer();
                                break;
                            }
                            if (RunTime.ConfigManager.Config.IsAutoWeb) IntervalExecute(CmdType.WebLogin);
                            OpenTimerChecker();
                            if (IsRememberPassword)
                            {
                                if (RunTime.ConfigManager.Config.Username != iEAPClient.Username || RunTime.ConfigManager.Config.Password != iEAPClient.Password)
                                {
                                    RunTime.ConfigManager.Config.Username = iEAPClient.Username;
                                    RunTime.ConfigManager.Config.Password = iEAPClient.Password;
                                    RunTime.ConfigManager.Save();
                                }
                                IsRememberPassword = false;
                            }
                            if (Utils.ComparisonAssign<string>(ref RunTime.ConfigManager.Config.PcapName, iEAPClient.PcapName)) RunTime.ConfigManager.Save();
                            break;
                        case EAPoLAuthType.Failure:
                            CLoseTimerChecker();
                            Output(CmdType.EapStart, t.ToString());
                            SendStatus(NetworkState.EapAbnormal);
                            if (CmdEapOnline)
                            {
                                if (EapRetryTimes < RunTime.ConfigManager.Config.EapRetryTimes)
                                {
                                    RunTime.Logger.Info(string.Format("稍后进行重试登陆认证.", EapRetryTimes));
                                    Thread.Sleep((int)Math.Pow(EapRetryTimes++, 2) * 1000);
                                    RunTime.Logger.Info(string.Format("正在进行第{0}次重试登录人证.", EapRetryTimes));
                                    iEAPClient.Login();
                                }
                                else
                                {
                                    RunTime.Logger.Info("认证失败");
                                    StartNextTime();
                                }
                            }
                            break;
                        case EAPoLAuthType.UsernameNotExist:
                        case EAPoLAuthType.PasswordWrong:
                            Output(CmdType.EapStart, t + iEAPClient.Username);
                            break;
                        case EAPoLAuthType.OperateTimeIntervalTooShort:
                            RunTime.Logger.Info("20秒后再自动登录.");
                            Thread.Sleep(20000);
                            iEAPClient.Login();
                            break;
                        case EAPoLAuthType.NoPermissionForTheTime:
                            StartNextTime();
                            break;
                        case EAPoLAuthType.ServerNoResponding:
                            RunTime.Logger.Info("服务器无响应!");
                            EAPContinue();
                            break;
                        case EAPoLAuthType.ClientSendFaild:
                            RunTime.Logger.Info("发送数据包失败,请检查网线是否连接好");
                            Thread.Sleep(15000);
                            EAPContinue();
                            break;
                    }
                }




                //


            });
        }

        #region 定时器

        private Timer iTimer;

        private void OpenTimerChecker()
        {
            if (RunTime.ConfigManager.Config.IsAutoCheck)
            {
                if (iTimer == null)
                {
                    iTimer = new Timer(CheckNetworkMethod);
                    RunTime.Logger.Info("启用定时检查.");
                }
                iTimer.Change(TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(RunTime.ConfigManager.Config.CheckTimeInterval));
            }
            else
            {
                CLoseTimerChecker();
            }
        }
        private void CLoseTimerChecker()
        {
            try
            {
                if (iTimer != null)
                {
                    iTimer.Dispose();
                    iTimer = null;
                    RunTime.Logger.Info("关闭定时检查.");
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("关闭定时检查出错"+ex.ToString());
            }
        }
        private void ConfigTimerChecker()
        {
            if (iTimer != null)
            {
                if (RunTime.ConfigManager.Config.IsAutoCheck)
                {
                    iTimer.Change(5000, RunTime.ConfigManager.Config.CheckTimeInterval * 1000);
                }
                else
                {
                    CLoseTimerChecker();
                }
            }
        }

        private void StartNextTime()
        {
            try
            {
                if (GlobalStatus.Mode == ApiMode.Server)
                {
                    //关闭设备
                    Close();
                    DateTime dt = DateTime.Parse(RunTime.ConfigManager.Config.NetStartTime);
                    while (dt.Ticks < DateTime.Now.Ticks)
                    {
                        dt = dt.AddDays(1);
                    }
                    RunTime.Logger.Info(string.Format("程序将在{0},开始运行!", dt.ToString()));
                    iTimer = new Timer(NextTimeRun);
                    iTimer.Change(new TimeSpan(dt.Ticks - DateTime.Now.Ticks), TimeSpan.FromDays(1));
                    RunTime.ClearMemory();
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("计划启动出错!"+ex.ToString());
            }
        }
        private void NextTimeRun(object o)
        {
            CLoseTimerChecker();
            Open();
        }

        #endregion
        


        private void SendStatus(NetworkState currentSate)
        {
            if (iNetworkState == currentSate) return;
            iNetworkState = currentSate;
            IntervalExecute(CmdType.Status);
        }
        private bool IsGatewayNormal
        {
            get
            {
                return iEAPClient.Interface.GatewayAddress == IPAddress.Any ? false : Utilities.Http.Ping(iEAPClient.Interface.GatewayAddress.ToString(), 60);
            }
        }
        private bool IsNetworkNormal
        {
            get
            {
                return Utilities.Http.Ping(RunTime.ConfigManager.Config.ExampleHosts, 80);
            }
        }
        private object checkLocker = new object();
        private void CheckNetworkMethod(object o)
        {
            //非阻塞单个线程进入
            //命令是不认证
            RunTime.Logger.Debug("检测网络AAAAAAAA"+Environment.CurrentManagedThreadId + iNetworkState);

            if (RunTime.ConfigManager.Config.IsAutoCheck && Monitor.TryEnter(checkLocker))
            {
                RunTime.Logger.Debug("####检测网络进入" + Environment.CurrentManagedThreadId + iNetworkState);
                try
                {
                    switch (iNetworkState)
                    {
                        //处理网页异常
                        case NetworkState.WebAbnormal:
                            //先判断需不需要处理此事
                            if (!CmdWebOnline) break;
                            if (IsNetworkNormal)
                            {
                                SendStatus(NetworkState.WebNormal);
                                break;
                            }
                            if (IsGatewayNormal)
                            {
                                RunTime.Logger.Info("检测,网页异常,将重新登录");
                                InternalWebAuthLogin();
                            }
                            else
                            {
                                SendStatus(NetworkState.EapAbnormal);
                                goto case NetworkState.EapAbnormal;
                            }
                            break;
                        //处理网关异常
                        case NetworkState.EapAbnormal:
                            if (!CmdEapOnline) break;
                            iEAPClient.RefreshInterface();
                            if (iEAPClient.Interface.GatewayAddress == IPAddress.Any)
                            {
                                CLoseTimerChecker();
                                iEAPClient.GetIPAddressFromServer();
                                break;
                            }
                            if (IsGatewayNormal)
                            {
                                SendStatus(NetworkState.EapNormal);
                                break;
                            }
                            if (iTimer != null)
                            {
                                RunTime.Logger.Info("检测,认证异常,将重新认证.");
                                CLoseTimerChecker();
                                iEAPClient.Login();
                            }
                            break;
                        //检查网页
                        case NetworkState.WebNormal:
                            if (!CmdWebOnline) goto case NetworkState.EapNormal;
                            //检查全部
                            //正常不作为
                            if (IsNetworkNormal) { RunTime.Logger.Debug("检测，正常"); break; }
                            RunTime.Logger.Debug("无法连接公网");
                            SendStatus(NetworkState.WebAbnormal);
                            goto case NetworkState.WebAbnormal;
                        //检查网关
                        case NetworkState.EapNormal:
                            if (!CmdEapOnline) break;
                            RunTime.Logger.Debug("&&&EapNormal");
                            //检查网关
                            if (!IsGatewayNormal)
                            {
                                RunTime.Logger.Info(string.Format("网关[{0}]:异常", iEAPClient.Interface.GatewayAddress));
                                SendStatus(NetworkState.EapAbnormal);
                                goto case NetworkState.EapAbnormal;
                            }else if (CmdWebOnline)
                            {
                                SendStatus(NetworkState.WebAbnormal);
                                goto case NetworkState.WebAbnormal;
                            }
                            break;
                    }
                    RunTime.ClearMemory();
                }
                finally
                {
                    Monitor.Exit(checkLocker);
                    RunTime.Logger.Debug("#####检测网络退出" + Environment.CurrentManagedThreadId + iNetworkState);
                }
            }
            RunTime.Logger.Debug("检测网络BBBBBBBBBBB" + Environment.CurrentManagedThreadId + iNetworkState);

        }
        
        public void Execute(string cmdStr)
        {
            if (!CmdHelper.IsCmd(cmdStr)) return;
            CmdType c;
            string p;
            CmdHelper.Parse(cmdStr, out c, out p);
            Execute(c, p);
        }
        public void Execute(object sender, string cmdStr)
        {
            Execute(cmdStr);
        }
        public async void Execute(CmdType cmd, string param)
        {
            await Task.Run(() => {
                IntervalExecute(cmd, param);
            });
        }

        private void IntervalExecute(CmdType cmd)
        {
            IntervalExecute(cmd, null);
        }
        private void IntervalExecute(CmdType cmd,string param)
        {
            //
            RunTime.Logger.Debug("执行命令"+cmd+"参数"+param);
            if (!opened) Open();
            bool flag;
            switch (cmd)
            {
                case CmdType.ReadConfig:
                    RunTime.ConfigManager.Read();
                    RunTime.Logger.Info("已重新读取配置");
                    Configure();
                    break;
                case CmdType.EapStart:
                    #region Eapol认证
                    CmdEapOnline = true;
                    CLoseTimerChecker();
                    //配置参数
                    if (param != null)
                    {
                        string[] tmp = param.Split('\t');
                        if (tmp.Length == 4)
                        {
                            iEAPClient.Username = tmp[0];
                            iEAPClient.Password = tmp[1];
                            IsRememberPassword = tmp[2] == "1";
                            if (RunTime.ConfigManager.Config.IsAutoEap != (tmp[3] == "1"))
                            {
                                RunTime.ConfigManager.Config.IsAutoEap = !RunTime.ConfigManager.Config.IsAutoEap;
                                RunTime.ConfigManager.Save();
                            }
                        }
                        else
                        {
                            RunTime.Logger.Info("EAPoL认证的参数错误!");
                        }
                    }
                    else
                    {
                        iEAPClient.Username = RunTime.ConfigManager.Config.Username;
                        iEAPClient.Password = RunTime.ConfigManager.Config.Password;

                    }

                    RunTime.Logger.Info("开始EAPoL认证...");
                    if (iEAPClient.Interface.GatewayAddress == IPAddress.Any)
                    {
                        iEAPClient.Login();
                    }
                    else
                    {
                        if (IsGatewayNormal)
                        {
                            if (iEAPClient.OnlineTimeTicks == 0) iEAPClient.SetNow();
                            RunTime.Logger.Info("已经认证成功,无需重复认证!");
                            SendStatus(NetworkState.EapNormal);
                            if (RunTime.ConfigManager.Config.IsAutoWeb)
                            {
                                goto case CmdType.WebLogin;
                            }
                            else
                            {
                                OpenTimerChecker();
                            }
                        }
                        else
                        {
                            iEAPClient.Login();
                        }

                    }
                    #endregion
                    break;
                case CmdType.EapStop:
                    CmdEapOnline = false;
                    CLoseTimerChecker();
                    iEAPClient.Logout();
                    break;
                case CmdType.WebLogin:
                    #region 网页登陆
                    CmdWebOnline = true;
                    CLoseTimerChecker();
                    if (IsNetworkNormal)
                    {
                        RunTime.Logger.Info("网页登录开始...");
                        RunTime.Logger.Info("已经在线无需登录网页!");
                        SendStatus(NetworkState.WebNormal);
                        OpenTimerChecker();
                    }
                    else
                    {
                        InternalWebAuthLogin();
                    }
                    #endregion
                    break;
                case CmdType.WebLogout:
                    CmdWebOnline = false;
                    if (iWebAuth == null) iWebAuth = new WebAuth();
                    flag = new WebAuth()
                    {
                        IsDefalutWebUri = RunTime.ConfigManager.Config.IsDefaultWebUri,
                        IPAddress = iEAPClient.Interface.IPAddress.ToString(),
                        WlanAcIp = RunTime.ConfigManager.Config.WlanAcIp
                    }.Logout();
                    if (flag)
                    {
                        SendStatus(NetworkState.WebAbnormal);
                    }
                    break;
                case CmdType.IpRenew:
                    RunTime.Logger.Info("正在获取Ip中...");
                    CmdNativeMethods.GetIPByDHCP();
                    break;
                case CmdType.Status:
                    string a = string.Empty;
                    string b = string.Empty;
                    switch (iNetworkState)
                    {
                        case NetworkState.WebNormal:
                            a = b = "1";
                            break;
                        case NetworkState.EapAbnormal:
                            a = b = "0";
                            break;
                        case NetworkState.WebAbnormal:
                        case NetworkState.EapNormal:
                            a = "1";
                            b = "0";
                            break;
                    }
                    Output(CmdType.Status, string.Format("{0}\t{1}\t{2}\t{3}\t{4}", iEAPClient.Username == null ? RunTime.ConfigManager.Config.Username : iEAPClient.Username, iEAPClient.Interface.IPAddress, a, b, iEAPClient.OnlineTimeTicks.ToString()));
                    break;
                case CmdType.Open:
                    Open();
                    break;
                case CmdType.Close:
                    Close();
                    break;
                case CmdType.ClientConnect:
                    RunTime.Logger.Info("桌面C端连接...");
                    break;
                case CmdType.ClientDisConnect:
                    RunTime.Logger.Info("桌面C端离开...");
                    break;
                default:
                    break;
            }
            RunTime.ClearMemory();
        }
        
        private void InternalWebAuthLogin()
        {
            if (iEAPClient.Interface.IPAddress == IPAddress.Any)
            {
                RunTime.Logger.Info(string.Format("IP地址{0},不能进行网页登陆",IPAddress.Any));
                return;
            }
            WebAuth auth = iWebAuth;
            auth.IsDefalutWebUri = RunTime.ConfigManager.Config.IsDefaultWebUri;
            auth.ExampleHosts = RunTime.ConfigManager.Config.ExampleHosts;
            auth.Username = RunTime.ConfigManager.Config.WebUsername;
            auth.Password = RunTime.ConfigManager.Config.WebPassword;
            auth.IPAddress = iEAPClient.Interface.IPAddress.ToString();
            auth.WlanAcIp = RunTime.ConfigManager.Config.WlanAcIp;
            string lasterror = auth.LastError;
            try
            {
                RunTime.Logger.Info("网页登录开始...");
                if (auth.Login())
                {
                    SendStatus(NetworkState.WebNormal);
                    if (RunTime.ConfigManager.Config.WlanAcIp != auth.WlanAcIp)
                    {
                        RunTime.ConfigManager.Config.WlanAcIp = auth.WlanAcIp;
                        RunTime.ConfigManager.Save();
                    }
                    //成功释放对象
                    iWebAuth = null;
                    OpenTimerChecker();
                }
                else
                {
                    SendStatus(NetworkState.WebAbnormal);
                }
            }
            catch (ArgumentException ex)
            {
                if (lasterror != ex.Message)
                {
                    RunTime.Logger.Info("网页登录失败" + ex.Message);
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("网页登录失败，未知异常" + ex.ToString());
            }
        }
        
        private void Output(CmdType type,string str)
        {
            if (OnOutputed != null)
            {
                OnOutputed(this, CmdHelper.GetString(type,str));
            }
        }
        public event EventHandler<string> OnOutputed;

        bool opened = false;

        public void Open()
        {
            lock (this)
            {
                if (opened) return;
                iEAPClient.OnEventFired += HandlerCenter;
                //初始化网卡信息
                if (GlobalStatus.Mode == ApiMode.Server)
                {
                    RunTime.Logger.Info("后台服务已启动");
                    iEAPClient.Open();
                    if (RunTime.ConfigManager.Config.IsAutoEap)
                    {
                        RunTime.Logger.Info("将进行自动认证.");
                        if (GlobalStatus.IsSystemJustStart)
                        {
                            iEAPClient.Username = RunTime.ConfigManager.Config.Username;
                            iEAPClient.Password = RunTime.ConfigManager.Config.Password;
                            CmdEapOnline = true;
                            iEAPClient.Login();
                        }
                        else
                        {
                            Execute(CmdType.EapStart, null);
                        }
                    }
                    else
                    {
                        RunTime.Logger.Info("未设置自动认证,请先配置好!");
                    }
                }
                else
                {
                    RunTime.Logger.Info("初始化完成.");
                    iEAPClient.Open();
                }
                opened = true;
            }
        }
        public void Close()
        {
            if (!opened) return;
            CLoseTimerChecker();
            iEAPClient.Close();
            iEAPClient = null;
            if (GlobalStatus.Mode== ApiMode.Server)
            {
                RunTime.Logger.Info("后台服务已关闭");
            }
            opened = false;
        }
    }
}
