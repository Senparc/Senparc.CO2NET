#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file
except in compliance with the License. You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the
License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND,
either express or implied. See the License for the specific language governing permissions
and limitations under the License.

Detail: https://github.com/Senparc/Senparc.CO2NET/blob/master/LICENSE

----------------------------------------------------------------*/
#endregion Apache License Version 2.0

/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc
    
    FileName：ServerUtility.cs
    File Function Description：Server utility class
    
    
    Creation Identifier：Senparc - 20180819

    Modification Identifier：Senparc - 20181225
    Modification Description：v0.4.2 Optimize path recognition in docker or linux environment in ServerUtility class methods


----------------------------------------------------------------*/


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Senparc.CO2NET.Utilities
{
    /// <summary>
    /// Server utility class
    /// </summary>
    public class ServerUtility
    {
        private static string _appDomainAppPath;

        /// <summary>
        /// dll project root directory
        /// </summary>
        public static string AppDomainAppPath
        {
            get
            {
                if (_appDomainAppPath == null)
                {
#if NET462
                    _appDomainAppPath = HttpRuntime.AppDomainAppPath;
#else
                    _appDomainAppPath = AppContext.BaseDirectory; //dll directory：;
#endif
                }
                return _appDomainAppPath;
            }
            set
            {
                _appDomainAppPath = value;
#if !NET462
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                var altPathSeparator = Path.AltDirectorySeparatorChar.ToString();
                if (!_appDomainAppPath.EndsWith(pathSeparator) && !_appDomainAppPath.EndsWith(altPathSeparator))
                {
                    _appDomainAppPath += pathSeparator;
                }
#endif
            }
        }

        /// <summary>
        /// Get the file path relative to the website root directory
        /// </summary>
        /// <param name="virtualPath"></param>
        /// <returns></returns>
        public static string ContentRootMapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                return "";
            }
            else
            {
                //if (!Config.RootDictionaryPath.EndsWith("/") || Config.RootDictionaryPath.EndsWith("\\"))
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                var altPathSeparator = Path.AltDirectorySeparatorChar.ToString();
                if (!Config.RootDirectoryPath.EndsWith(pathSeparator) && !Config.RootDirectoryPath.EndsWith(altPathSeparator))
                {
                    Config.RootDirectoryPath += pathSeparator;
                }

                if (virtualPath.StartsWith("~/"))
                {
                    return virtualPath.Replace("~/", Config.RootDirectoryPath).Replace("/", pathSeparator);
                }
                else
                {
                    return Path.Combine(Config.RootDirectoryPath, virtualPath);
                }
            }
        }

        /// <summary>
        /// Get the absolute file path relative to the dll directory
        /// </summary>
        /// <param name="virtualPath">Virtual path, such as ~/App_Data/</param>
        /// <returns></returns>
        public static string DllMapPath(string virtualPath)
        {
            if (virtualPath == null)
            {
                return "";
            }
            else if (virtualPath.StartsWith("~/"))
            {
                var pathSeparator = Path.DirectorySeparatorChar.ToString();
                return virtualPath.Replace("~/", AppDomainAppPath).Replace("/", pathSeparator);
            }
            else
            {
                return Path.Combine(AppDomainAppPath, virtualPath);
            }
        }

    }
}
