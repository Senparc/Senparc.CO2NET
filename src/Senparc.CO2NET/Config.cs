#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc
  
    文件名：Config.cs
    文件功能描述：全局配置文件
    
    
    创建标识：Senparc - 20180602
 

    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Config.cs  ----

    修改标识：Senparc - 201806021
    修改描述：v0.1.2 为 DefaultCacheNamespace 设置默认值
   
    修改标识：Senparc - 20180704
    修改描述：v0.1.4 添加 SenparcSetting 全局配置属性
 
    修改标识：Senparc - 20180830
    修改描述：v0.2.9 优化 Config.RootDictionaryPath 方法，可自动获取默认值

    修改标识：Senparc - 20180911
    修改描述：v0.8.10 提供 Config.HostingEnvironment 属性 
   
----------------------------------------------------------------*/


using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Senparc.CO2NET 全局设置
    /// </summary>
    public class Config
    {

        /// <summary>
        /// <para>全局配置</para>
        /// <para>在 startup.cs 中运行 RegisterServiceExtension.AddSenparcGlobalServices() 即可自动注入</para>
        /// </summary>
        public static SenparcSetting SenparcSetting { get; set; } = new SenparcSetting();//TODO:需要考虑分布式的情况，后期需要储存在缓存中

        /// <summary>
        /// 指定是否是Debug状态，如果是，系统会自动输出日志
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
        /// 请求超时设置（以毫秒为单位），默认为10秒。
        /// 说明：此处常量专为提供给方法的参数的默认值，不是方法内所有请求的默认超时时间。
        /// </summary>
        public const int TIME_OUT = 10000;

        /// <summary>
        /// JavaScriptSerializer 类接受的 JSON 字符串的最大长度
        /// </summary>
        public static int MaxJsonLength = int.MaxValue;//TODO:需要考虑分布式的情况，后期需要储存在缓存中


        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
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

        private static string _rootDictionaryPath = null;

        /// <summary>
        /// 网站根目录绝对路径
        /// </summary>
        public static string RootDictionaryPath
        {
            get
            {
                if (_rootDictionaryPath == null)
                {
#if NET45
                    var appPath = AppDomain.CurrentDomain.BaseDirectory;

                    if (Regex.Match(appPath, $@"[\\/]$", RegexOptions.Compiled).Success)
                    {
                        _rootDictionaryPath = appPath;//
                        //_rootDictionaryPath = appPath.Substring(0, appPath.Length - 1);

                    }
#else
                    _rootDictionaryPath = AppContext.BaseDirectory;
#endif
                }

                return _rootDictionaryPath;
            }
            set
            {
                _rootDictionaryPath = value;
            }
        }
    }
}
