using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LitePcap;

namespace CampusNetGuard.Code
{
    interface IPcapHelper
    {
        void Open();
        void Close();
        string Name { get; }
        string Filter { get; set; }
        LitePcap.LibPcap.PcapInterface Interface { get; }
        void SendPacket(byte[] packet);
        void SendPacket(ICaptureDevice device, byte[] packet);
        void SelectDevice(ICaptureDevice device);
        event EventHandler<CaptureEventArgs> OnPacketCaptured;
    }
}
