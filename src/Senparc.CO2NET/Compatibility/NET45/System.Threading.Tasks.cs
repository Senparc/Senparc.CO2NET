#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2019 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2020 Senparc

    文件名：System.Threading.Tasks.cs
    文件功能描述：为了弥补NET3.5没有System.Threading.Tasks命名空间下某些方法的问题


    创建标识：Senparc - 20190412

----------------------------------------------------------------*/

#if !NET35 && !NET40
namespace System.Threading.Tasks
{
    public static class TaskExtension
    {
        /// <summary>
        /// 使 Task.Completed 兼容 .NET 4.5 的方法
        /// </summary>
        /// <returns></returns>
        public static Task CompletedTask()
        {
#if NET45
            return Task.FromResult<object>(null);
#else
            return Task.CompletedTask;
#endif

        }
    }
}
#endif