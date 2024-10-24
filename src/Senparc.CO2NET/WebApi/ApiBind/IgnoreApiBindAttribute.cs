/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: IgnoreApiBindAttribute.cs
    File Function Description: Ignore ApiBindAttribute


    Creation Identifier: Senparc - 20210714

----------------------------------------------------------------*/

using System;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Ignore ApiBind attribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = true)]
    public class IgnoreApiBindAttribute : ApiBindAttribute
    {
        public IgnoreApiBindAttribute() : base(true)
        { }
    }
}
