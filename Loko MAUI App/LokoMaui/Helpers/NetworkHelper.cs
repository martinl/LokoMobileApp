using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace loko.Helpers
{
    public static class NetworkHelper
    {
        public static bool IsNetworkAvailable()
        {
            return Connectivity.NetworkAccess == NetworkAccess.Internet;
        }
    }
}
