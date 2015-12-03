using System;
using System.Configuration.Install;
using System.ServiceProcess;

namespace CampusNetGuard.Libraries
{
    public class ServiceUtils
    {

        public void InstallService(string filepath)
        {
            using (AssemblyInstaller m = new AssemblyInstaller())
            {
                m.UseNewContext = true;
                m.Path = filepath;
                m.Install(null);
                m.Commit(null);
            }
        }
        public void UnInstallService(string filepath)
        {
            using (AssemblyInstaller m = new AssemblyInstaller())
            {
                m.UseNewContext = true;
                m.Path = filepath;
                m.Uninstall(null);
            }
        }
        public bool IsServiceExisted(string serviceName)
        {
            bool flag = false;
            ServiceController[] services = ServiceController.GetServices();
            foreach (var item in services)
            {
                if (!false)
                {
                    flag |= item.ServiceName.ToLower() == serviceName.ToLower();
                }
                item.Dispose();
            }
            return flag;
        }
        public bool IsServiceRunning(string serviceName)
        {
            using (ServiceController sc=new ServiceController(serviceName))
            {
                try
                {
                    return sc.Status.Equals(ServiceControllerStatus.Running);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }
            }
        }
    }
}
