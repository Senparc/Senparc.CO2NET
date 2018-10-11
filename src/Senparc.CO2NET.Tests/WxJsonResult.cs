using System;
using System.Collections.Generic;
using System.Text;

namespace Senparc.CO2NET.Tests
{
    public class WxJsonResult
    {
        public int errcode { get; set; }


        /// <summary>
        /// 返回结果信息
        /// </summary>
        public virtual string errmsg { get; set; }

        public virtual object P2PData { get; set; }

        public override string ToString()
        {

            return string.Format("WxJsonResult：{{errcode:'{0}',errcode_name:'{1}',errmsg:'{2}'}}",
                (int)errcode, errcode.ToString(), errmsg);
        }
    }
}
