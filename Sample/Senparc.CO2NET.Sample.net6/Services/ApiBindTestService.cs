using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Sample.net6.Services
{
    public class ApiBindTestService
    {
        [ApiBind("CO2NET", "ApiBindTest.TestApi")]
        public string TestApi(string name, int value)
        {
            return $"{name}:{value}";
        }
    }

    public static class StaticApiBindTestService
    {
        [ApiBind("CO2NETStatic", "StaticApiBindTest.TestApi")]
        public static string TestApi(string name, int value)
        {
            return $"{name}:{value}";
        }


    }
}
