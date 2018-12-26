/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：SystemTime.cs
    文件功能描述：用于从 DateTimeOffset 进行扩展，方便进行单元测试


    创建标识：Senparc - 20181113

    修改标识：Senparc - 20181226
    修改描述：1、将 DateTime 改为 DateTimeOffset
              2、添加 Today 属性

----------------------------------------------------------------*/

namespace System
{
    /// <summary>
    /// 时间扩展类
    /// </summary>
    public static class SystemTime
    {
        ///// <summary>
        ///// 返回 Now 方法
        ///// </summary>
        //public static Func<DateTime> GetNow = () => SystemTime.Now;

        /// <summary>
        /// 当前时间
        /// </summary>
        public static DateTimeOffset Now => DateTimeOffset.Now;

        /// <summary>
        /// 当天零点时间
        /// </summary>
        public static DateTimeOffset Today => new DateTimeOffset(DateTimeOffset.Now.Year, DateTimeOffset.Now.Month, DateTimeOffset.Now.Day, 0, 0, 0, TimeSpan.Zero);
    }
}
