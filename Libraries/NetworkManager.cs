using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Management;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{
    public class NetworkManager
    {
        public const string SET = "Win32_NetworkAdapterConfiguration";
        private string ConverToMac(byte[] mac)
        {
            string macStr = string.Empty;
            foreach (var item in mac)
            {
                if (macStr != string.Empty)
                {
                    macStr += ":";
                }
                macStr += item.ToString("X").PadLeft(2, '0');
            }
            return macStr;

        }
        public void GetNetworkAdapterInformation(ref string ipAddress,ref string submask,ref string gateway,ref string dnsSt,ref string dnsRd, byte[] mac)
        {
            using (ManagementObjectCollection moc = new ManagementClass(SET).GetInstances())
            {
                foreach (var item in moc)
                {
                    using (item)
                    {
                        if (ConverToMac(mac) == item["MACAddress"] as string)
                        {
                            try
                            {

                                ipAddress = (item["IPAddress"] as string[])[0];
                                submask = (item["IPSubnet"] as string[])[0];
                                gateway = (item["DefaultIPGateway"] as string[])[0];
                                string[] s = item["DNSServerSearchOrder"] as string[];
                                dnsSt = s[0];
                                dnsRd = s.Length == 2 ? s[1] : string.Empty;
                            }
                            catch (Exception ex)
                            {
                                break;
                                throw ex;
                            }
                        }
                    }
                }
            }
        }
        public void SetNetworkAdapterInformation()
        {

        }

    }
}
