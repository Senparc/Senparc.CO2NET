#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2025 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2025 Senparc
    
    FileName：ValidationHelper.cs
    File Function Description：Validation Helper
    
    
    Creation Identifier：Senparc - 20250128

----------------------------------------------------------------*/


using Senparc.CO2NET.Exceptions;
using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Helpers.Validations
{
    /// <summary>
    /// 验证帮助类
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// 检查对象是否为 null，如果是 null，则抛出异常并包含调用者信息。
        /// </summary>
        /// <param name="obj">要检查的对象</param>
        /// <param name="memberName">调用者的成员名称（自动填充）</param>
        /// <param name="filePath">调用者的文件路径（自动填充）</param>
        /// <param name="lineNumber">调用者的行号（自动填充）</param>
        /// <param name="throwException">是否抛出异常</param>
        /// <exception cref="ArgumentNullException">当对象为 null 时抛出</exception>
        public static void CheckNull(this object obj,
            [CallerMemberName] string memberName = "",
            [CallerFilePath] string filePath = "",
            [CallerLineNumber] int lineNumber = 0, bool throwException = false)
        {
            if (obj == null)
            {
                var msg = $"Parameter '{nameof(obj)}' is null. " +
                    $"Called from '{memberName}' in '{filePath}' at line {lineNumber}.";
                var ex = new SenparcNullReferenceException(msg);
                SenparcTrace.BaseExceptionLog(ex);
                if (throwException)
                {
                    throw ex;
                }
            }
        }
    }
}
