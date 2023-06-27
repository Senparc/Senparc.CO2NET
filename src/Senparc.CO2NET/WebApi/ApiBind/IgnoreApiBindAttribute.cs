/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    文件名：IgnoreApiBindAttribute.cs
    文件功能描述：忽略 ApiBindAttribute


    创建标识：Senparc - 20210714

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET
{
    /// <summary>
    /// 忽略 ApiBind 特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class IgnoreApiBindAttribute : ApiBindAttribute
    {
        public IgnoreApiBindAttribute() : base(true)
        { }
    }
}
