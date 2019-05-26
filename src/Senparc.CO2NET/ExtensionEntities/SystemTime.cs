/*----------------------------------------------------------------
    Copyright (C) 2019 Senparc

    文件名：SystemTime.cs
    文件功能描述：用于从 DateTimeOffset 进行扩展，方便进行单元测试


    创建标识：Senparc - 20181113

    修改标识：Senparc - 20181226
    修改描述：1、将 DateTime 改为 DateTimeOffset
              2、添加 Today 属性


    修改标识：Senparc - 20190427
    修改描述：v0.6.1 添加 NowTicks 属性

    修改标识：Senparc - 20190507
    修改描述：v0.7.1 添加 NowDiff 属性

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
        /// 当天零点时间，从 SystemTime.Now.Date 获得
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// 获取当前时间的 Ticks
        /// </summary>
        public static long NowTicks => Now.Ticks;


        //TODO：添加更多实用方法

        /// <summary>
        /// 获取 TimeSpan
        /// </summary>
        /// <param name="compareTime">当前时间 - compareTime</param>
        /// <returns></returns>
        public static TimeSpan NowDiff(DateTimeOffset compareTime)
        {
            return Now - compareTime;
        }

        /// <summary>
        /// 获取 TimeSpan
        /// </summary>
        /// <param name="compareTime">当前时间 - compareTime</param>
        /// <returns></returns>
        public static TimeSpan NowDiff(DateTime compareTime)
        {
            return Now.DateTime - compareTime;
        }


    }
}
