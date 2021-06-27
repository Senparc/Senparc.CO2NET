using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.AspNet.WebApi
{
    /// <summary>
    /// 自动绑定属性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class ApiBindAttribute : Attribute
    {
    }
}
