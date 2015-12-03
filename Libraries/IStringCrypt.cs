using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Libraries
{
    interface IStringCrypt
    {
        string Encode(string x);
        string Decode(string x);
    }
}
