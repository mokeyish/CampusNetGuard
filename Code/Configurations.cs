using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{
    public class Configurations
    {
        public string UniqueId;
        public int ver = 0;
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username;
        /// <summary>
        /// 密码
        /// </summary>
        public string Password;
        public string WebUsername;
        public string WebPassword;
        public string NetStartTime = "06:30";
        public string PcapName;
        public string WlanAcIp;
        public string[] ExampleHosts = new string[] { "www.iqiyi.com", "www.baidu.com", "tieba.baidu.com", "baidu.com", "www.qq.com","qq.com", "weibo.com", "sina.cn" , "www.sohu.com" };
        public string[] WebUris = new string[] { null, null };
        public bool IsFirstRun = true;
        public bool IsAutoRun = false;
        public bool IsAutoEap = false;
        public bool IsAutoWeb = false;
        public bool IsAutoCheck = true;
        public bool IsEnabledDhcp;
        public bool IsDefaultWebUri = true;
        /// <summary>
        /// 内存紧凑是否开始
        /// </summary>
        public bool IsCompactMemory = true;
        public int CheckTimeInterval = 10;
        public int EapRetryTimes = 3;
        public override string ToString()
        {
            return fastJSON.JSON.ToJSON(this);
        }
    }
}
