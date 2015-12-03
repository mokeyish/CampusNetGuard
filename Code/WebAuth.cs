using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utilities;

namespace CampusNetGuard.Code
{
    public class WebAuth
    {
        public string LastError { get; private set; }
        private const string HostName = "enet.10000.gd.cn";
        private const string DefaultLoginUri = "http://{0}:10001/login.do?userName1={1}&password1={2}&eduuser={3}&edubas={4}";
        private const string DefaultLogoutUri = "http://{0}:10001/Logout.do?eduuser={1}&edubas={2}";
        private bool _IsDefalutWebUri = true;
        public bool IsDefalutWebUri
        {
            get { return _IsDefalutWebUri; }
            set { _IsDefalutWebUri = value; }
        }
        private string _IPAddress;
        public string IPAddress
        {
            get { return _IPAddress; }
            set { _IPAddress = value; }
        }
        private string _WlanAcIp;
        public string WlanAcIp
        {
            get { return _WlanAcIp; }
            set { _WlanAcIp = value; }
        }
        private string _Username;
        public string Username
        {
            get { return _Username; }
            set { _Username = value; }
        }
        private string _Password;
        public string Password
        {
            get { return _Password; }
            set { _Password = value; }
        }
        private string[] _UserWebUri;
        public string[] UserWebUri
        {
            get { return _UserWebUri; }
            set { _UserWebUri = value; }
        }
        private string[] _ExampleHosts;
        public string[] ExampleHosts
        {
            get { return _ExampleHosts; }
            set { _ExampleHosts = value; }
        }
        private void CheckArgument()
        {
            if (Username == null || Password == null || IPAddress == null)
            {
                LastError = "帐号或密码或IP地址未设置!";
                throw new ArgumentException(LastError);
            }
            if (WlanAcIp == null)
            {
                if (ExampleHosts == null)
                {
                    LastError = "示例域名未设置，不能自动获取区域IP!";
                    throw new ArgumentException(LastError);
                }
                WlanAcIp = GetWlanacip(ExampleHosts);
                if (WlanAcIp == null)
                {
                    LastError = "区域IP未设置,并且自动获取失败!";
                    throw new ArgumentException(LastError);
                }
            }

        }
        public bool Login()
        {
            string result;
            if (IsDefalutWebUri)
            {
                CheckArgument();
                RunTime.Logger.Info(string.Format("网页认证[{0}],登录中...", IPAddress));
                result = Post(string.Format(DefaultLoginUri, HostName, Username, Password, IPAddress, WlanAcIp));
                if (result.Contains("登录成功！"))
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],成功！", IPAddress));
                    return true;
                }
                else if (result.Contains("没有定购此产品"))
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],没有定购此产品", IPAddress));
                }
                else if (result.Contains("密码错误"))
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],密码错误", IPAddress));
                }else if (result.Contains("时长或余额不足"))
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],时长或余额不足", IPAddress));
                }
                else if (result.Contains("帐号状态为暂停"))
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],帐号状态为暂停，可能需要续费", IPAddress));
                }
                else
                {
                    RunTime.Logger.Info(string.Format("网页认证[{0}],失败,{1}", IPAddress, result));
                }
            }
            else
            {

                //用户自定义
                if (UserWebUri == null || UserWebUri.Length != 2)
                {
                    RunTime.Logger.Info("用户自定义登录地址未设置!");
                    return false;
                }

                RunTime.Logger.Info("网页认证(自定义地址),登录中...");
                result = Post(UserWebUri[0]);
                RunTime.Logger.Info("网页认证(自定义地址),登录完成...");
            }
            return false;
        }
        public bool Logout()
        {
            string result;
            if (IsDefalutWebUri)
            {
                RunTime.Logger.Info("正在网页注销...");
                if (IPAddress == null)
                {
                    RunTime.Logger.Info("网页注销未进行,IP地址为空!");
                    return false;
                }
                if (WlanAcIp == null)
                {
                    RunTime.Logger.Info("网页注销未进行,区域IP为空!");
                    RunTime.Logger.Info("区域IP，需要到登陆页自动获取!");
                    return false;
                }
                try
                {
                    result = Post(string.Format(DefaultLogoutUri, HostName, IPAddress, WlanAcIp));
                    if (result.Contains("下网成功！"))
                    {
                        RunTime.Logger.Info("网页注销,成功!");
                        return true;
                    }
                    else if (result.Contains("下网失败！"))
                    {
                        RunTime.Logger.Info("网页注销,失败!");
                    }
                }
                catch (Exception ex)
                {
                    RunTime.Logger.Info("网页注销,失败,原因:" + ex.Message);
                }
            }
            else
            {
                //自定义地址

                //用户自定义
                if (UserWebUri == null || UserWebUri.Length != 2)
                {
                    RunTime.Logger.Info("用户自定义登录地址未设置!");
                    return false;
                }

                RunTime.Logger.Info("网页认证(自定义地址),注销中...");
                result = Post(UserWebUri[0]);
                RunTime.Logger.Info("网页认证(自定义地址),注销完成...");
            }

            return false;
        }

        
        private string GetWlanacip(string[] hostNameOrAddressArray)
        {
            try
            {

                Utilities.HttpHelper http = new Utilities.HttpHelper();
                Utilities.HttpItem ite = new Utilities.HttpItem();
                Utilities.HttpResult result = null;
                
                foreach (var item in hostNameOrAddressArray)
                {
                    ite.URL = "http://" + item;
                    result = http.GetHtml(ite);
                    if (result.RedirectUrl.Contains(HostName)) break;
                }
                if (result == null) return null;
                string[] x;
                foreach (var item in result.RedirectUrl.Split('?')[1].Split('&'))
                {
                    x = item.Split('=');
                    if (x[0].ToLower() == "wlanacip")
                    {
                        return x[1];
                    }
                }
            }
            catch (Exception ex)
            {
                RunTime.Logger.Error("自动获取wlanacip出错" + ex.ToString());
            }
            return null;
        }

        public static string Post(string uri)
        {
            string[] u = uri.Split('?');
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = u[0],
                Method = "POST",
                Allowautoredirect = true,
                Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                UserAgent = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.80 Safari/537.36",
                ContentType = "application/x-www-form-urlencoded",
                Postdata = u[1]
            };
            HttpResult result = http.GetHtml(item);
            return result.Html;
        }
    }
}
