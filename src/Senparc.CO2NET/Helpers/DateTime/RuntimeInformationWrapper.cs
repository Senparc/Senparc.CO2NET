using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Helpers
{
#if !NET462
    public interface IRuntimeInformation
    {
        bool IsOSPlatform(OSPlatform osPlatform);
    }

    public class RuntimeInformationWrapper : IRuntimeInformation
    {
        public bool IsOSPlatform(OSPlatform osPlatform)
        {
            return RuntimeInformation.IsOSPlatform(osPlatform);
        }
    }
#endif
}
