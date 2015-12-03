using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Libraries
{
    class CmdNativeMethods
    {
        public static void SetAddressByDhcp(string name)
        {
            CommandLine.Run("netsh", string.Format("interface ip set address \"{0}\" dhcp", name));
        }
        public static void SetDnsByDhcp(string name)
        {
            CommandLine.Run("netsh", string.Format("interface ip set dns \"{0}\" dhcp", name));
        }
        public static void SetAddress(string name,string ip,string netmask,string gateway)
        {
            CommandLine.Run("netsh", string.Format("interface ip set address \"{0}\" static {1} {2} {3}", name, ip, netmask, gateway));
        }
        public static void GetIPByDHCP()
        {
            CommandLine.Run("ipconfig", "/renew");
        }
    }
}
