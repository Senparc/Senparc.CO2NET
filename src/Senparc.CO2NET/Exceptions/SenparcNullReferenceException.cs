/*----------------------------------------------------------------
    Copyright(C) 2019 Senparc

    文件名：SenparcNullReferenceException.cs
    文件功能描述：对象为 null 的异常


    创建标识：Senparc - 20200506

----------------------------------------------------------------*/


using System;

namespace Senparc.CO2NET.Exceptions
{
    public class SenparcNullReferenceException : BaseException
    {
        /// <summary>
        /// 上一级不为null的对象（或对调试很重要的对象）。
        /// 如果需要调试多个对象，可以传入数组，如：new {obj1, obj2}
        /// </summary>
        public object ParentObject { get; set; }
        public SenparcNullReferenceException(string message)
            : this(message, null, null)
        {
        }

        public SenparcNullReferenceException(string message, object parentObject)
            : this(message, parentObject, null)
        {
            ParentObject = parentObject;
        }

        public SenparcNullReferenceException(string message, object parentObject, Exception inner)
            : base(message, inner)
        {
            ParentObject = parentObject;
        }
    }
}
