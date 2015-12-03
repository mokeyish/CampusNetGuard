using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{

    internal static class GlobalStatus
    {
        internal static ApiMode Mode { get { return CmdApi.Instance.Mode; } }

        internal static bool IsSystemAccout { get { return WindowsIdentity.GetCurrent().IsSystem; } }
        internal static bool IsServiceExisted { get { return new CampusNetGuard.Libraries.ServiceUtils().IsServiceExisted(CampusNetGuardSvc.iServiceName); } }

        internal static bool IsAdminstrator { get { return CampusNetGuard.Libraries.CommandLine.IsAdministrator; } }
        internal static bool IsSystemJustStart { get { return Environment.TickCount < 99000; } }

    }
}
