using System.Diagnostics;
using System.Security.Principal;

namespace CampusNetGuard.Libraries
{
    public static class CommandLine
    {
        public static bool IsAdministrator
        {
            get
            {
                return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        public static void RunAsAdministrator(string filename, string[] args)
        {
            RunAsAdministrator(filename, string.Join(" ", args));
        }
        public static void RunAsAdministrator(string filename, string args)
        {
            try
            {

                using (Process cmd = new Process())
                {
                    cmd.StartInfo.FileName = filename;
                    cmd.StartInfo.Arguments = args;
                    cmd.StartInfo.Verb = "runas";
                    //cmd.StartInfo.UseShellExecute = false;
                    cmd.StartInfo.CreateNoWindow = true;//不显示窗口（控制台程序是黑屏） 
                    cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    cmd.Start();
                    cmd.WaitForExit();
                    cmd.Close();
                }
            }
            catch (System.Exception)
            {
            }
        }
        public static string Run(string filename, string[] args)
        {
            return Run(filename, string.Join(" ", args));
        }
        public static void AsyncRun(string filename, string args)
        {
            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = filename;
                cmd.StartInfo.Arguments = args;
                cmd.Start();
            }
        }
        public static string Run(string filename,string args)
        {
            using (Process cmd = new Process())
            {
                cmd.StartInfo.FileName = filename;//设置程序名   
                cmd.StartInfo.Arguments = args;  //参数  
                //重定向标准输出 
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.RedirectStandardInput = true;

                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.CreateNoWindow = true;//不显示窗口（控制台程序是黑屏）   
                cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                //暂时不明白什么意思   
                /*收集一下 有备无患关于:ProcessWindowStyle.Hidden隐藏后如何再显示？ 
                hwndWin32Host = Win32Native.FindWindow(null, win32Exinfo.windowsName); 
                Win32Native.ShowWindow(hwndWin32Host, 1);     //先FindWindow找到窗口后再ShowWindow 
                */
                cmd.Start();
                string info = cmd.StandardOutput.ReadToEnd();
                cmd.WaitForExit();
                cmd.Close();
                return info;
            }

        }
    }
}
