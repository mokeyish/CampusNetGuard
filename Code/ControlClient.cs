using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{
    public class ControlClient
    {
        private readonly string Name;
        private NamedPipeClientStream iNamedPipeClientStream;
        private StreamReader iStreamReader;
        private StreamWriter iStreamWriter;
        public ControlClient(string name)
        {
            Name = name + "Pipe";
            iNamedPipeClientStream = new NamedPipeClientStream(".", Name, PipeDirection.InOut, PipeOptions.Asynchronous, System.Security.Principal.TokenImpersonationLevel.Anonymous);
        }
        private byte retry = 2;
        public void Connect()
        {
            if (retry == 0) return;
            try
            {
                iNamedPipeClientStream.Connect(100);
                iStreamReader = new StreamReader(iNamedPipeClientStream);

                if (Name == iStreamReader.ReadLine())
                {
                    ListenToServer();
                }
                else
                {
                    throw new Exception("不一样啊");
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Debug(ex.Message);
                if (ex.Message.Contains("信号灯超时时间已到"))
                {
                    RunTime.Logger.Info("后台已有人连接");
                    retry--;
                    Connect();
                }
                else
                {
                    throw ex;
                }
            }
        }

        private async void ListenToServer()
        {
            await Task.Run(() => {
                while (!isDisposed&&iNamedPipeClientStream.IsConnected)
                {
                    if (OnReceived != null)
                    {
                        OnReceived(this, iStreamReader.ReadLine());
                    }
                }

                if (!iNamedPipeClientStream.IsConnected&& OnDisconnected!=null)
                {
                    OnDisconnected(this, EventArgs.Empty);
                }

            });
        }
        public event EventHandler OnDisconnected;
        public event EventHandler<string> OnReceived;

        public void Send(object sender, string cmdStr)
        {
            try
            {
                if (iNamedPipeClientStream != null && iStreamWriter == null)
                {
                    iStreamWriter = new StreamWriter(iNamedPipeClientStream);
                    iStreamWriter.AutoFlush = true;
                }
                iStreamWriter.WriteLine(cmdStr);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        private bool isDisposed = false;
        public void Dispose()
        {

        }

    }
}
