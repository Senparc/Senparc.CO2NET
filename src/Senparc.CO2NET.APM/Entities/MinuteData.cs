#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2023 Suzhou Senparc Network Technology Co.,Ltd.

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

    FileName：MinuteData.cs
    File Function Description：Packaged statistical data per minute


    Creation Identifier：Senparc - 20181117

    Modification Identifier：Senparc - 20181226
    Modification Description：v0.4.3 changed DateTime to DateTimeOffset

 ----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    /// <summary>
    /// Packaged statistical data per minute
    /// </summary>
    public class MinuteData
    {
        public string KindName { get; set; }
        /// <summary>
        /// Statistical time period, accurate to the minute
        /// </summary>
        public DateTimeOffset Time { get; set; }

        /// <summary>
        /// Starting value
        /// </summary>
        public double StartValue { get; set; }
        /// <summary>
        /// Ending value
        /// </summary>
        public double EndValue { get; set; }
        /// <summary>
        /// Maximum value
        /// </summary>
        public double HighestValue { get; set; }
        /// <summary>
        /// Minimum value
        /// </summary>
        public double LowestValue { get; set; }
        /// <summary>
        /// Total value
        /// </summary>
        public double SumValue { get; set; }
        /// <summary>
        /// Number of statistical value samples
        /// </summary>
        public int SampleSize { get; set; }
    }
}
