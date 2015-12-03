using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CampusNetGuard.Code
{
    enum DhcpType:byte
    {

        None,
        Discover=0x01,
        Offer = 0x02,
        Request=0x03,
        Ack=0x05,
    }
}
