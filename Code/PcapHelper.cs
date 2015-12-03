using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using LitePcap;

namespace CampusNetGuard.Code
{
    class PcapHelper : IPcapHelper
    {
        private readonly string name;
        private LitePcap.WinPcap.WinPcapDevice dev;

        public event EventHandler<CaptureEventArgs> OnPacketCaptured;

        public PcapHelper(string name)
        {
            this.name = name;
        }
        public LitePcap.LibPcap.PcapInterface Interface
        {
            get
            {
                return dev == null ? null : dev.Interface;
            }
        }
        public string Filter { get; set; }
        public string Name
        {
            get
            {
                return dev == null ? null : dev.Name;
            }
            
        }

        public PhysicalAddress MacAddress
        {
            get
            {
                return dev == null ? null : dev.MacAddress;
            }
        }
        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;
            if (disposing)
            {
            }
            if (dev != null) dev.Close();
            disposed = true;

        }
        ~PcapHelper()
        {
            Dispose(false);
        }
        private void OpenDevice(ICaptureDevice device)
        {
            try
            {
                device.Open();
                device.Filter = string.Format(Filter, PacketDotNet.Utils.HexPrinter.PrintMACAddress(device.MacAddress));
                device.OnPacketArrival += Handler;
                device.StartCapture();

            }
            catch (Exception)
            {

                throw;
            }
        }
        private void Handler(object sender,CaptureEventArgs e)
        {
            var m = OnPacketCaptured;
            if (m != null)
            {
                m(this, e);
            }
        }
        private void CloseDevice(ICaptureDevice device)
        {
            try
            {
                device.OnPacketArrival -= Handler;
                device.Close();
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("关闭pcap线程的错误"+ex.ToString());
            }
        }
        public void Open()
        {
            dev = GetDevice(name);

            if (dev != null) OpenDevice(dev);
        }

        public async void SelectDevice(ICaptureDevice device)
        {
            await Task.Run(() => {
                if (dev != null) return;
                foreach (var item in LitePcap.WinPcap.WinPcapDeviceList.Instance)
                {
                    if (item.Interface.Name == device.Name)
                    {
                        dev = item;
                    }
                    else
                    {
                        CloseDevice(item);
                    }
                }
                alldevsOpened = false;
            });
        }

        public void SendPacket(byte[] packet)
        {
            SendPacket(dev, packet);
        }
        private bool alldevsOpened = false;
        public void SendPacket(ICaptureDevice device, byte[] packet)
        {
            if (device != null)
            {
                InternalSendPacket(device, packet);
            }else if (dev != null)
            {
                RunTime.Logger.Debug("选择备" + dev.Name);
                InternalSendPacket(dev, packet);
            }
            else
            {
                RunTime.Logger.Debug(name);
                if (dev == null)
                {
                    dev = GetDevice(name);
                    if (dev != null)
                    {
                        RunTime.Logger.Debug("打开默认设备");
                        OpenDevice(dev);
                        InternalSendPacket(dev, packet);
                        return;
                    }

                }

                RunTime.Logger.Debug("打开全不 默认设备");
                foreach (var item in LitePcap.WinPcap.WinPcapDeviceList.Instance)
                {
                    if (!alldevsOpened) OpenDevice(item);
                    InternalSendPacket(item, packet);
                }
                alldevsOpened = true;
            }

        }
        private LitePcap.WinPcap.WinPcapDevice GetDevice(string name)
        {

            if (name == null) return null;
            LitePcap.WinPcap.WinPcapDeviceList.Instance.Refresh();
            foreach (var item in LitePcap.WinPcap.WinPcapDeviceList.Instance)
            {
                if (item.Name == name) return item;
            }
            return null;
        }
        private void InternalSendPacket(ICaptureDevice device, byte[] packet)
        {
            try
            {
                //存入当前设备的物理地址
                Buffer.BlockCopy(device.MacAddress.GetAddressBytes(), 0, packet, 6, 6);
                device.SendPacket(packet);
                RunTime.Logger.Debug("发送&&&成功");
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("发送数据包失败,将重新初始化,提示:" + ex.Message);
                switch (ex.Message)
                {
                    case "device not open":
                    case "Can't send packet: send error: PacketSendPacket failed":
                        CloseDevice(device);
                        device = dev = GetDevice(device.Name);
                        if (device != null)
                        {
                            OpenDevice(device);
                            device.SendPacket(packet);
                        }
                        else
                        {
                            RunTime.Logger.Error("重新初始化设备失败,设备不存在");
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        public void Close()
        {
            if (dev != null)
            {
                CloseDevice(dev);
                dev = null;
            }
        }

    }
}
