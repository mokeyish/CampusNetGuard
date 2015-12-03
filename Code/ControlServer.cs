using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{
    public class ControlServer 
    {
        private readonly string Name;
        private StreamWriter SW;
        public ControlServer(string name)
        {
            Name = name + "Pipe";
        }
        public event EventHandler OnClientOnline;
        public event EventHandler OnClientOffline;
        public async void Listen()
        {
            await Task.Run(() => {
                PipeSecurity ps = new PipeSecurity();
                ps.AddAccessRule(new PipeAccessRule("Users", PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));
                while (!isDisposed)
                {
                    try
                    {
                        using (NamedPipeServerStream NPS = new NamedPipeServerStream(Name, PipeDirection.InOut, 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous,1024,1024,ps))
                        {
                            NPS.WaitForConnection();
                            using (SW = new StreamWriter(NPS))
                            {
                                SW.AutoFlush = true;
                                SW.WriteLine(Name);
                                if (NPS.IsConnected && OnClientOnline != null) OnClientOnline(this, EventArgs.Empty);
                                using (StreamReader sr = new StreamReader(NPS))
                                {
                                    while (NPS.IsConnected)
                                    {
                                        if (OnReceived != null)
                                        {
                                            OnReceived(this, sr.ReadLine());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (UnauthorizedAccessException ex)
                    {
                        RunTime.Logger.Error("服务管道被占用"+ex.Message);
                        Listen();
                        break;
                    }catch (ObjectDisposedException)
                    {
                        if (OnClientOffline != null) OnClientOffline(this, EventArgs.Empty);
                    }
                }

            });
        }
        public event EventHandler<string> OnReceived;

        public void Send(object sender, string cmdStr)
        {
            try
            {
                if (SW!=null) SW.WriteLine(cmdStr);
            }
            catch (Exception)
            {

            }
        }
        private bool isDisposed = false;
        public void Dispose()
        {
        }

    }
}
