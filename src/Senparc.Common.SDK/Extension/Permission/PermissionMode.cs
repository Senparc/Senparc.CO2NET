/*----------------------------------------------------------------
    Copyright (C) 2018 Senparc

    文件名：PermissionMode.cs
    文件功能描述：Senparc.Common.SDK 权限认证模式类


    创建标识：MartyZane - 20181030

----------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Senparc.Common.SDK
{
    /// <summary>
    /// 权限认证模式
    /// </summary>
    public enum PermissionMode
    {
        /// <summary>执行</summary>
        Enforce,
        /// <summary>忽略</summary>
        Ignore
    }
}
