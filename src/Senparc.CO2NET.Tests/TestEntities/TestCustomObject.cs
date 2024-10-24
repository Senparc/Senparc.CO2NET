using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntities
{
    /// <summary>
    /// Custom object for testing
    /// </summary>
    [Serializable]
    public class TestCustomObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset AddTime { get; set; }

        public int? Markers { get; set; }
        public long[] ArrLong { get; set; }
        public int[] ArrInt { get; set; }


        public TestCustomObject()
        {
            AddTime = SystemTime.Now;
        }
    }
}
