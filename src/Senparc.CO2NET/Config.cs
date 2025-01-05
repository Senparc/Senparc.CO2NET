#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc
  
    FileName: Config.cs
    File Function Description: Global configuration file
    
    
    Creation Identifier: Senparc - 20180602
 

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Config.cs  ----

    Modification Identifier: Senparc - 201806021
    Modification Description: v0.1.2 Set default value for DefaultCacheNamespace
   
    Modification Identifier: Senparc - 20180704
    Modification Description: v0.1.4 Add SenparcSetting global configuration property
 
    Modification Identifier: Senparc - 20180830
    Modification Description: v0.2.9 Optimize Config.RootDirectoryPath method, can automatically get default value

    Modification Identifier: Senparc - 20180911
    Modification Description: v0.8.10 Provide Config.HostingEnvironment property 

    Modification Identifier: Senparc - 20211101
    Modification Description: v1.6 Rename RootDictionaryPath to RootDirectoryPath
 
    Modification Identifier: Senparc - 20221219
    Modification Description: v2.1.4 Set RootDictionaryPath to throw error when expired
  
----------------------------------------------------------------*/


using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Senparc.CO2NET global settings
    /// </summary>
    public class Config
    {

        /// <summary>
        /// <para>Global configuration</para>
        /// <para>Run RegisterServiceExtension.AddSenparcGlobalServices() in startup.cs to automatically inject</para>
        /// </summary>
        public static SenparcSetting SenparcSetting { get; internal set; } = new SenparcSetting();//TODO: Need to consider distributed scenarios, later need to store in cache

        /// <summary>
        /// Specify whether it is in Debug mode, if true, the system will automatically output logs
        /// </summary>
        public static bool IsDebug
        {
            get
            {
                return SenparcSetting.IsDebug;
            }
            set
            {
                SenparcSetting.IsDebug = value;

                //if (_isDebug)
                //{
                //    SenparcTrace.Open();
                //}
                //else
                //{
                //    SenparcTrace.Close();
                //}
            }
        }

        /// <summary>
        /// Request timeout setting (in milliseconds), default is 10 seconds.
        /// Note: This constant is specifically provided as a default value for method parameters, not the default timeout for all requests within the method.
        /// </summary>
        public const int TIME_OUT = 10000;

        /// <summary>
        /// Maximum length of JSON string accepted by JavaScriptSerializer class
        /// </summary>
        public static int MaxJsonLength = int.MaxValue;//TODO: Need to consider distributed scenarios, later need to store in cache


        /// <summary>
        /// First-level namespace for default cache keys, default value: DefaultCache
        /// </summary>
        public static string DefaultCacheNamespace
        {
            get
            {
                return SenparcSetting.DefaultCacheNamespace ?? "DefaultCache";
            }
            set
            {
                SenparcSetting.DefaultCacheNamespace = value;
            }
        }

        private static string _rootDirectoryPath = null;

        /// <summary>
        /// Absolute path of the website root directory
        /// </summary>
        public static string RootDirectoryPath
        {
            get
            {

                if (_rootDirectoryPath == null)
                {
#if NET462
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;

                    if (Regex.Match(appPath, $@"[\\/]$", RegexOptions.Compiled).Success)
                    {
                        _rootDirectoryPath = appPath;//
                        //_rootDictionaryPath = appPath.Substring(0, appPath.Length - 1);

                    }
#else
                    _rootDirectoryPath = AppContext.BaseDirectory;
#endif
                }

                return _rootDirectoryPath;
            }
            set
            {
                _rootDirectoryPath = value;
            }
        }

        /// <summary>
        /// Absolute path of the website root directory
        /// </summary>
        [Obsolete("请使用 RootDirectoryPath 属性", true)]
        public static string RootDictionaryPath
        {
            get => RootDirectoryPath;
            set => RootDirectoryPath = value;
        }
    }
}
