using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code.Log
{
    public interface ILog
    {
        void Debug(string message);
        void Info(string message);
        void Warn(string message);
        void Error(string message);
        void Fatal(string message);
        string InfoFileName { get; set; }

        event EventHandler<string> OnInfoed;
    }
}
