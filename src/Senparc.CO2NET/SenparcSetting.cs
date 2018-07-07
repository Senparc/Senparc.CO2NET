/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SenparcSetting.cs
    文件功能描述：CO2NET 全局设置

    创建标识：Senparc - 20180704

    修改标识：Senparc - 20180707
    修改描述：v0.1.9 增加带 isDebug 参数的构造函数

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
            IsDebug = isDebug   ;
        }
    }
}
