using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace CampusNetGuard.Code
{
    public class NetCardInterface
    {
        public string FriendlyName;
        public IPAddress IPAddress;
        public IPAddress Netmask;
        public IPAddress GatewayAddress;
        public IPAddress[] Dns;
        public PhysicalAddress MacAddress;
        public int LeaseTime;
        public NetCardInterface()
        {
            IPAddress = IPAddress.Any;
            Netmask = IPAddress.Any;
            GatewayAddress = IPAddress.Any;
            Dns = new IPAddress[] { IPAddress.Any, IPAddress.Any };
            MacAddress = PhysicalAddress.None;
        }
        public void Refresh(string id)
        {
            if (id == null) return;
            foreach (var item in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (id.EndsWith(item.Id))
                {
                    FriendlyName = item.Name;
                    MacAddress = item.GetPhysicalAddress();
                    if (item.GetIPProperties().UnicastAddresses.Count == 2)
                    {
                        IPAddress = item.GetIPProperties().UnicastAddresses[1].Address;
                        Netmask = item.GetIPProperties().UnicastAddresses[1].IPv4Mask;
                    }
                    else if (item.GetIPProperties().UnicastAddresses.Count == 1)
                    {
                        IPAddress = item.GetIPProperties().UnicastAddresses[0].Address;
                        Netmask = item.GetIPProperties().UnicastAddresses[0].IPv4Mask;
                    }
                    GatewayAddress = item.GetIPProperties().GatewayAddresses.Count == 1 ? item.GetIPProperties().GatewayAddresses[0].Address : IPAddress.Any;
                    for (int i = 0; i < Dns.Length; i++) Dns[i] = item.GetIPProperties().DnsAddresses.Count >= 1 + i ? item.GetIPProperties().DnsAddresses[i] : IPAddress.Any;
                    break;
                }
            }


        }
    }
}
