using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using CampusNetGuard.Libraries;
using System.Threading.Tasks;
using System.ServiceProcess;

namespace CampusNetGuard.Code
{

    public enum ApiMode : byte
    {
        /// <summary>
        /// 默认模式
        /// </summary>
        Default,
        /// <summary>
        /// 本端模式，直接接收指令并执行
        /// </summary>
        None,
        /// <summary>
        /// 服务端模式，接收客户端指令并执行
        /// </summary>
        Server,
        /// <summary>
        /// 客户端模式，仅将指令发送给服务端执行，
        /// 注意，当服务端不可用时，将自动转化为本端模式
        /// </summary>
        Client
    }
    public class CmdApi 
    {

        private ControlCore controlCore;
        private ControlServer controlServer;
        private ControlClient controlClient;
        /// <summary>
        /// 当前的模式
        /// </summary>
        public ApiMode Mode { get; private set; }
        private CmdApi()
        {
            RunTime.Logger.OnInfoed += OutputHandler;
            //处理非UI线程异常
            AppDomain.CurrentDomain.UnhandledException += (o,e)=> {
                RunTime.Logger.Error("############Back#################");
                RunTime.Logger.Error(e.ExceptionObject.ToString());
            };
        }

        public static readonly CmdApi Instance = new CmdApi();
        /// <summary>
        /// 设置模式
        /// </summary>
        /// <param name="mode"></param>
        public void SetMode(ApiMode mode)
        {
            this.Mode = mode;
        }

        
        private void WhileClientOnline(object o,EventArgs e)
        {
            Execute(this, CmdHelper.GetString(CmdType.ClientConnect));
        }
        private void WhileClientOffline(object o,EventArgs e)
        {
            Execute(this, CmdHelper.GetString(CmdType.ClientDisConnect));
        }
        private void WhileServerOffline(object o,EventArgs e)
        {
            ReInitialize(ApiMode.None);
        }
        private async void WaitForServiceStoped()
        {
            await Task.Run(() => {
                using (ServiceController sc = new ServiceController(CampusNetGuardSvc.iServiceName))
                {
                    if (sc.Status == ServiceControllerStatus.Running)
                    {
                        sc.WaitForStatus(ServiceControllerStatus.Stopped);
                        ReInitialize(ApiMode.None);
                    }
                }
            });
        }



        private bool initilized = false;
        private void ReInitialize(ApiMode mode)
        {
            if (Monitor.TryEnter(this))
            {
                try
                {
                    if (Mode != mode)
                    {
                        Mode = mode;
                        initilized = false;
                        Initialize();
                        OnModeChanged(this, EventArgs.Empty);
                    }
                    Open();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
        }
        /// <summary>
        /// 同步初始化
        /// </summary>
        public void Initialize()
        {
            lock (this)
            {
                if (initilized) return;
                switch (Mode)
                {
                    case ApiMode.None:
                        if (controlCore == null)
                        {
                            controlCore = new ControlCore();
                            controlCore.OnOutputed += OutputHandler;
                            controlCore.Open();
                        }
                        if (controlClient != null)
                        {
                            controlClient.OnReceived -= OutputHandler;
                            controlClient.OnDisconnected -= WhileServerOffline;
                            controlClient.Dispose();
                            controlClient = null;
                        }
                        break;
                    case ApiMode.Server:
                        RunTime.Logger.InfoFileName = RunTime.Directory.AppData + string.Format("{0}.log", DateTime.Now.ToString("yyyy-MM-dd"));
                        if (controlCore == null)
                        {
                            controlCore = new ControlCore();
                            controlCore.OnOutputed += OutputHandler;
                            controlCore.Open();
                        }
                        if (controlServer == null)
                        {
                            controlServer = new ControlServer(RunTime.AppName);
                            controlServer.OnClientOnline += WhileClientOnline;
                            controlServer.OnClientOffline += WhileClientOffline;
                            controlServer.OnReceived += controlCore.Execute;
                            controlServer.Listen();
                        }
                        break;
                    case ApiMode.Client:
                        if (controlClient == null)
                        {
                            controlClient = new ControlClient(RunTime.AppName);
                            controlClient.OnDisconnected += WhileServerOffline;
                            controlClient.OnReceived += OutputHandler;
                            try
                            {
                                controlClient.Connect();
                            }
                            catch (Exception ex)
                            {
                                RunTime.Logger.Info("后台服务连接失败" + ex.Message);
                                Mode = ApiMode.None;
                                goto case ApiMode.None;
                            }
                        }
                        break;

                    case ApiMode.Default:
                        if (GlobalStatus.IsServiceExisted && new ServiceUtils().IsServiceRunning(CampusNetGuardSvc.iServiceName))
                        {
                            WaitForServiceStoped();
                            ReInitialize(ApiMode.Client);
                        }
                        else
                        {
                            ReInitialize(ApiMode.None);
                        }
                        break;
                }
                initilized = true;
            }
        }


        private void OutputHandler(object sender,string str)
        {
            var m = OnOutputed;
            if (m != null)
            {
                m(sender, str);
            }
            if (Mode==ApiMode.Server&&controlServer!=null)
            {
                controlServer.Send(sender, str);
            }
        }

        public event EventHandler<string> OnOutputed;
        public event EventHandler OnModeChanged;

        public void Execute(CmdType cmd)
        {
            Execute(cmd, null);
        }
        public void Execute(CmdType cmd,string param)
        {
            if (controlCore != null)
            {
                controlCore.Execute(cmd, param);
            }
            else
            {
                Execute(this, CmdHelper.GetString(cmd, param));
            }
        }
        /// <summary>
        /// 异步执行方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="cmdStr"></param>
        public async void Execute(object sender, string cmdStr)
        {
            await Task.Run(() => {
                if (initilized) Initialize();
                switch (Mode)
                {
                    case ApiMode.None:
                    case ApiMode.Server:
                        if (controlCore != null)
                        {
                            controlCore.Execute(sender, cmdStr);
                        }
                        else
                        {
                            RunTime.Logger.Error("controlCore为null");
                        }
                        break;
                    case ApiMode.Client:
                        try
                        {
                            controlClient.Send(sender, cmdStr);
                        }
                        catch (Exception)
                        {
                            ReInitialize(Mode = ApiMode.None);
                            controlClient.Send(sender, cmdStr);
                        }
                        break;
                }
            });
        }
        /// <summary>
        /// 启动
        /// </summary>
        public  void Open()
        {
            if (!initilized) Initialize();
            switch (Mode)
            {
                case ApiMode.None:
                case ApiMode.Server:
                    controlCore.Open();
                    break;
                case ApiMode.Client:
                    controlClient.Send(this, CmdHelper.GetString(CmdType.Open));
                    break;
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public void Close()
        {
            switch (Mode)
            {
                case ApiMode.None:
                    controlCore.Execute(this,CmdHelper.GetString(CmdType.Close));
                    break;
                case ApiMode.Server:
                    controlServer.Dispose();
                    controlCore.Execute(this, CmdHelper.GetString(CmdType.Close));
                    break;
                case ApiMode.Client:
                    break;
                default:
                    break;
            }
        }
    }
}