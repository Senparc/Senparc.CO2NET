using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Senparc.CO2NET.Tests
{
    public static class UnitTestHelper
    {
        /// <summary>
        /// Root directory address
        /// </summary>
        public static string RootPath => Path.GetFullPath("..\\..\\..\\");


        /// <summary>
        /// Check for keyword existence
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="keywords"></param>
        /// <returns></returns>
        public static bool CheckKeywordsExist(string filePath, params string[] keywords)
        {
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                using (var sr = new StreamReader(fs))
                {
                    var content = sr.ReadToEnd();
                    foreach (var item in keywords)
                    {
                        if (!content.Contains(item))
                        {
                            return false;
                        }
                    }
                    return true;
                }
            }
        }
    }
}
