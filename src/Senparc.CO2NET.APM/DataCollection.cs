using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Senparc.CO2NET.APM
{
    public class DataOperation
    {
        const string  CACHE_NAMESPACE= "SENPARC_APM";

        private string _domain;

        /// <summary>
        /// DataOperation 构造函数
        /// </summary>
        /// <param name="domain">域，统计的最小单位，可以是一个网站，也可以是一个模块</param>
        public DataOperation(string domain)
        {
            _domain = domain;
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="kindName">统计类别名称</param>
        /// <param name="value">统计值</param>
        /// <param name="data">复杂类型数据</param>
        /// <returns></returns>
        public DataItem Set(string kindName, double value, object data = null,DateTime? dateTime=null)
        {
            var dateItem = new DataItem() {
                KindName = kindName,
                Value = value,
                Data = data,
                DateTime = dateTime ?? DateTime.Now;
            };
        }
    }
}