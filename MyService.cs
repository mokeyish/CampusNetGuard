using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using CampusNetGuard.Code;

namespace CampusNetGuard
{
    partial class CampusNetGuardSvc : ServiceBase
    {
        public const string iServiceName = "CampusNetGuardSvc";
        public CampusNetGuardSvc()
        {
            InitializeComponent();
        }
        private async void StartApi()
        {
            await Task.Run(() => {
                CmdApi.Instance.SetMode(ApiMode.Server);
                CmdApi.Instance.Open();
            });
        }
        protected override void OnStart(string[] args)
        {
            StartApi();
        }
        
        protected override void OnStop()
        {
            CmdApi.Instance.Close();
        }
        protected override void OnSessionChange(SessionChangeDescription changeDescription)
        {
            RunTime.Logger.Info(changeDescription.Reason.ToString());
            base.OnSessionChange(changeDescription);
			//RemoteConnect
            if (changeDescription.Reason == SessionChangeReason.SessionUnlock)
            {
                CmdApi.Instance.Execute(CmdType.EapStart);
            }
        }
        protected override void OnShutdown()
        {
            base.OnShutdown();
            CmdApi.Instance.Close();
        }
    }
}
