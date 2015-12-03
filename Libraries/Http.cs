using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public static class Http
    {



        /// <summary>
        /// 并发式判断网络是否连通
        /// </summary>
        /// <param name="hostNameOrAddressArray">服务器组</param>
        /// <param name="timeOut">超时</param>
        /// <returns></returns>
        public static bool Ping(string[] hostNameOrAddressArray, int timeOut)
        {
            int x;
            return Ping(hostNameOrAddressArray[x = NextRandom(0, hostNameOrAddressArray.Length, hostNameOrAddressArray.Length)], timeOut) || Ping(hostNameOrAddressArray[x], timeOut);
        }
        private static int NextRandom(int a, int z, int last)
        {
            Random m = new Random();
            int n;
            while ((n = m.Next(a, z)) == last) { }
            return n;
        }
        public static bool Ping(string hostNameOrAddress, int timeOut)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    return IPStatus.Success == ping.Send(hostNameOrAddress, timeOut).Status;
                }
            }
            catch (PingException)
            {
                return false;
            }
        }
    }
}
