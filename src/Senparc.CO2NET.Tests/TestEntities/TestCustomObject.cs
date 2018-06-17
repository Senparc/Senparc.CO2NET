using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests.TestEntities
{
    /// <summary>
    /// 测试用的自定义对象
    /// </summary>
    public class TestCustomObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime AddTime { get; set; }

        public TestCustomObject()
        {
            AddTime = DateTime.Now;
        }
    }
}
