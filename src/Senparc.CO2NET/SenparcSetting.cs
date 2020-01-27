/*----------------------------------------------------------------
    Copyright (C) 2020 Senparc

    文件名：SenparcSetting.cs
    文件功能描述：CO2NET 全局设置

    创建标识：Senparc - 20180704

    修改标识：Senparc - 20180707
    修改描述：v0.1.9 增加带 isDebug 参数的构造函数

    修改标识：Senparc - 20180707
    修改描述：v0.1.11 提供 BuildFromWebConfig() 方法

----------------------------------------------------------------*/

namespace Senparc.CO2NET
{
    /// <summary>
    /// CO2NET 全局设置
    /// </summary>
    public class SenparcSetting
    {
        /// <summary>
        /// 是否出于Debug状态
        /// </summary>
        public bool IsDebug { get; set; }

        /// <summary>
        /// 默认缓存键的第一级命名空间，默认值：DefaultCache
        /// </summary>
        public string DefaultCacheNamespace { get; set; }

        /// <summary>
        /// Senparc 统一代理标识
        /// </summary>
        public string SenparcUnionAgentKey { get; set; }


        #region 分布式缓存

        /// <summary>
        /// Redis连接字符串
        /// </summary>
        public string Cache_Redis_Configuration { get; set; }

        /// <summary>
        /// Memcached连接字符串
        /// </summary>
        public string Cache_Memcached_Configuration { get; set; }


        #endregion


        /// <summary>
        /// SenparcSetting 构造函数
        /// </summary>
        public SenparcSetting() : this(false)
        {

        }

        /// <summary>
        /// SenparcSetting 构造函数
        /// </summary>
        public SenparcSetting(bool isDebug)
        {
            IsDebug = isDebug;
        }

#if NET45
        /// <summary>
        /// 从 Web.Config 文件自动生成 SenparcSetting
        /// </summary>
        /// <param name="isDebug">设置 CO2NET 全局的 Debug 状态 </param>
        /// <returns></returns>
        public static SenparcSetting BuildFromWebConfig(bool isDebug)
        {
            var senparcSetting = new SenparcSetting(isDebug);

            senparcSetting.DefaultCacheNamespace = System.Configuration.ConfigurationManager.AppSettings["DefaultCacheNamespace"];
            senparcSetting.SenparcUnionAgentKey = System.Configuration.ConfigurationManager.AppSettings["SenparcUnionAgentKey"];
            senparcSetting.Cache_Redis_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Redis_Configuration"];
            senparcSetting.Cache_Memcached_Configuration = System.Configuration.ConfigurationManager.AppSettings["Cache_Memcached_Configuration"];
            return senparcSetting;
        }
#endif
    }
}
