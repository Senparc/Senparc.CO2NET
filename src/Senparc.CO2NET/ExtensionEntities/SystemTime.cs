/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName：SystemTime.cs
    File Function Description：Extension from DateTimeOffset for easier unit testing


    Creation Identifier：Senparc - 20181113

    Modification Identifier：Senparc - 20181226
    Modification Description：1. Changed DateTime to DateTimeOffset
              2. Added Today property


    Modification Identifier：Senparc - 20190427
    Modification Description：v0.6.1 Added NowTicks property

    Modification Identifier：Senparc - 20190507
    Modification Description：v0.7.1 Added NowDiff property
    
    Modification Identifier：Senparc - 20190914
    Modification Description：v0.9.0 Added SystemTime.UtcDateTime property

    Modification Identifier：Senparc - 20191001
    Modification Description：v1.0.102 Added more SystemTime helper methods

----------------------------------------------------------------*/

namespace System
{
    /// <summary>
    /// Time extension class
    /// </summary>
    public static class SystemTime
    {
        ///// <summary>
        ///// Returns Now method
        ///// </summary>
        //public static Func<DateTime> GetNow = () => SystemTime.Now;

        /// <summary>
        /// Current time
        /// </summary>
        public static DateTimeOffset Now => DateTimeOffset.Now;

        /// <summary>
        /// Current time in UTC DateTime type
        /// </summary>
        public static DateTime UtcDateTime => DateTimeOffset.Now.UtcDateTime;

        /// <summary>
        /// Midnight time of the day, obtained from SystemTime.Now.Date
        /// </summary>
        public static DateTime Today => Now.Date;

        /// <summary>
        /// Get current time Ticks
        /// </summary>
        public static long NowTicks => Now.Ticks;



        /// <summary>
        /// Get TimeSpan
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <returns></returns>
        public static TimeSpan NowDiff(DateTimeOffset compareTime)
        {
            return Now - compareTime;
        }

        /// <summary>
        /// Get TotalMilliseconds time difference
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <returns></returns>
        public static double DiffTotalMS(DateTimeOffset compareTime)
        {
            return NowDiff(compareTime).TotalMilliseconds;
        }


        /// <summary>
        /// Get TotalMilliseconds time difference
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <param name="format">Parameter for ToString([format]) on TotalMilliseconds result</param>
        /// <returns></returns>
        public static string DiffTotalMS(DateTimeOffset compareTime, string format)
        {
            return NowDiff(compareTime).TotalMilliseconds.ToString(format);
        }

        /// <summary>
        /// Get TimeSpan
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <returns></returns>
        public static TimeSpan NowDiff(DateTime compareTime)
        {
            return Now.DateTime - compareTime;
        }

        /// <summary>
        /// Get TotalMilliseconds time difference
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <returns></returns>
        public static double DiffTotalMS(DateTime compareTime)
        {
            return NowDiff(compareTime).TotalMilliseconds;
        }

        /// <summary>
        /// Get TotalMilliseconds time difference
        /// </summary>
        /// <param name="compareTime">Current time - compareTime</param>
        /// <param name="format">Parameter for ToString([format]) on TotalMilliseconds result</param>
        /// <returns></returns>
        public static string DiffTotalMS(DateTime compareTime, string format)
        {
            return NowDiff(compareTime).TotalMilliseconds.ToString(format);
        }

        //TODO: Add more utility methods

    }
}
