using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code.Log
{
    static class LogManager
    {
        public static ILog GetLogger(string x)
        {
            return new LogImpl();
        }
    }
}
