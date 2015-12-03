using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{

    public enum CmdType : byte
    {
        ReadConfig,
        Open,
        Close,
        Status,
        EapStart,
        EapStop,
        WebLogin,
        WebLogout,
        DchpEnabled,
        CheckNetwork,
        IpRenew,
        ClientConnect,
        ClientDisConnect
    }
    public static class CmdHelper
    {
        public static bool IsCmd(string cmdStr)
        {
            return cmdStr != null && cmdStr[0] == '~';
        }
        /// <summary>
        /// 匹配命令,调用之前必须先判断IsCmd
        /// </summary>
        /// <param name="cmdStr"></param>
        /// <param name="c"></param>
        /// <param name="p"></param>
        public static void Parse(string cmdStr, out CmdType c,out string p)
        {
            c = (CmdType)Enum.Parse(typeof(CmdType), cmdStr.Substring(1, 3));
            if (cmdStr.Length != 4)
            {
                p = cmdStr.Substring(4);
            }
            else
            {
                p = null;
            }
        }
        private static string Transfer(CmdType cmdType)
        {
            return ((byte)cmdType).ToString().PadLeft(3, '0');
        }
        public static string GetString(CmdType cmdType)
        {
            return string.Format("~{0}", Transfer(cmdType));
        }
        public static string GetString(CmdType cmdType, string paramter)
        {
            if (paramter == null) return GetString(cmdType);
            return string.Format("~{0}{1}", Transfer(cmdType), paramter);
        }
    }
    
}
