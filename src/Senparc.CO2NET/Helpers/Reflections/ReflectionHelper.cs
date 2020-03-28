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
    
    文件名：ReflectionHelper.cs
    文件功能描述：反射帮助类
    参考文章：http://www.cnblogs.com/zfanlong1314/p/4197383.html
    
    创建标识：Senparc - 20170702


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/ReflectionHelper.cs  ----

    修改标识：Senparc - 20180602
    修改描述：v0.1.0 移植 ReflectionHelper

    修改标识：Senparc - 20180602
    修改描述：v0.1.6.2 扩展 ReflectionHelper.GetStaticMember() 方法

    修改标识：Senparc - 20200228
    修改描述：v1.3.102 提供 ReflectionHelper 异常是否记录日志的选项

----------------------------------------------------------------*/

using Senparc.CO2NET.Trace;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Senparc.CO2NET.Helpers
{
    /// <summary>
    /// 反射帮助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullName">命名空间.类型名</param>
        /// <param name="assemblyName">程序集</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string fullName, string assemblyName)
        {
            string path = fullName + "," + assemblyName;//命名空间.类型名,程序集
            Type o = Type.GetType(path);//加载类型
            object obj = Activator.CreateInstance(o, true);//根据类型创建实例
            return (T)obj;//类型转换并返回
        }

        /// <summary>
        /// 创建对象实例
        /// </summary>
        /// <typeparam name="T">要创建对象的类型</typeparam>
        /// <param name="assemblyName">类型所在程序集名称</param>
        /// <param name="nameSpace">类型所在命名空间</param>
        /// <param name="className">类型名</param>
        /// <param name="recordLog">是否记录日志</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string assemblyName, string nameSpace, string className, bool recordLog = false)
        {
            try
            {
                string fullName = nameSpace + "." + className;//命名空间.类型名
                                                              //此为第一种写法
#if !NET45
                //object ect = Assembly.Load(new AssemblyName(assemblyName)).CreateInstance(fullName);//加载程序集，创建程序集里面的 命名空间.类型名 实例s

                //.net core 2.1这种方法也已经支持
                object ect = Assembly.Load(assemblyName).CreateInstance(fullName);//加载程序集，创建程序集里面的 命名空间.类型名 实例s
#else
                object ect = Assembly.Load(assemblyName).CreateInstance(fullName);//加载程序集，创建程序集里面的 命名空间.类型名 实例
#endif
                return (T)ect;//类型转换并返回
                //下面是第二种写法
                //string path = fullName + "," + assemblyName;//命名空间.类型名,程序集
                //Type o = Type.GetType(path);//加载类型
                //object obj = Activator.CreateInstance(o, true);//根据类型创建实例
                //return (T)obj;//类型转换并返回
            }
            catch (Exception ex)
            {
                if (recordLog)
                {
                    SenparcTrace.BaseExceptionLog(ex);
                }                //发生异常，返回类型的默认值
                return default(T);
            }
        }

        /// <summary>
        /// 获取静态类属性
        /// </summary>
        /// <param name="assemblyName">类型所在程序集名称</param>
        /// <param name="nameSpace">类型所在命名空间</param>
        /// <param name="className">类型名</param>
        /// <param name="memberName">属性名称（忽略大小写）</param>
        /// <param name="recordLog">是否记录日志</param>
        /// <returns></returns>
        public static object GetStaticMember(string assemblyName, string nameSpace, string className,string memberName,bool recordLog=false)
        {
            try
            {
                string fullName = nameSpace + "." + className;//命名空间.类型名
                string path = fullName + "," + assemblyName;//命名空间.类型名,程序集
                var type = Type.GetType(path);
                PropertyInfo[] props = type.GetProperties();
                var prop = props.FirstOrDefault(z => z.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));
                return prop.GetValue(null,null);
            }
            catch (Exception ex)
            {
                if (recordLog)
                {
                    SenparcTrace.BaseExceptionLog(ex);
                }
                return null;
            }
        }

        /// <summary>
        /// 获取静态类属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="memberName">属性名称（忽略大小写）</param>
        /// <param name="recordLog">是否记录日志</param>
        /// <returns></returns>
        public static object GetStaticMember(Type type, string memberName, bool recordLog = false)
        {
            try
            {
                PropertyInfo[] props = type.GetProperties();
                var prop = props.FirstOrDefault(z => z.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));
                return prop.GetValue(null, null);
            }
            catch (Exception ex)
            {
                if (recordLog)
                {
                    SenparcTrace.BaseExceptionLog(ex);
                }
                return null;
            }
        }
    }
}
