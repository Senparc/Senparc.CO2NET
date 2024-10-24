#region Apache License Version 2.0
/*----------------------------------------------------------------

Copyright 2024 Suzhou Senparc Network Technology Co.,Ltd.

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
    Copyright (C) 2024 Senparc
    
    FileName: ReflectionHelper.cs
    File Function Description: Reflection helper class
    Reference article: http://www.cnblogs.com/zfanlong1314/p/4197383.html
    
    Creation Identifier: Senparc - 20170702


    ----  CO2NET   ----
    ----  split from Senparc.Weixin/Utilities/ReflectionHelper.cs  ----

    Modification Identifier: Senparc - 20180602
    Modification Description: v0.1.0 Port ReflectionHelper

    Modification Identifier: Senparc - 20180602
    Modification Description: v0.1.6.2 Extend ReflectionHelper.GetStaticMember() method

    Modification Identifier: Senparc - 20200228
    Modification Description: v1.3.102 Provide option to log exceptions in ReflectionHelper

    Modification Identifier: Senparc - 20230110
    Modification Description: v2.1.6 Add null check for target object not found in ReflectionHelper.GetStaticMember() method, no longer throw exception

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
    /// Reflection helper class
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// Create object instance
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="fullName">Namespace.TypeName</param>
        /// <param name="assemblyName">Assembly</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string fullName, string assemblyName)
        {
            string path = fullName + "," + assemblyName;//Namespace.TypeName, Assembly
            Type o = Type.GetType(path);//Load type
            object obj = Activator.CreateInstance(o, true);//Create instance based on type
            return (T)obj;//Type conversion and return
        }

        /// <summary>
        /// Create object instance
        /// </summary>
        /// <typeparam name="T">Type of object to create</typeparam>
        /// <param name="assemblyName">Assembly name where the type is located</param>
        /// <param name="nameSpace">Namespace where the type is located</param>
        /// <param name="className">Type name</param>
        /// <param name="recordLog">Whether to log</param>
        /// <returns></returns>
        public static T CreateInstance<T>(string assemblyName, string nameSpace, string className, bool recordLog = false)
        {
            try
            {
                string fullName = nameSpace + "." + className;//Namespace.TypeName
                                                              //This is the first way
#if !NET462
                //object ect = Assembly.Load(new AssemblyName(assemblyName)).CreateInstance(fullName);//Load assembly, create instance of Namespace.TypeName in the assembly

                //.net core 2.1 also supports this method
                object ect = Assembly.Load(assemblyName).CreateInstance(fullName);//Load assembly, create instance of Namespace.TypeName in the assembly
#else
                object ect = Assembly.Load(assemblyName).CreateInstance(fullName);//Load assembly, create instance of Namespace.TypeName in the assembly
#endif
                return (T)ect;//Type conversion and return
                //Below is the second way
                //string path = fullName + "," + assemblyName;//Namespace.TypeName, Assembly
                //Type o = Type.GetType(path);//Load type
                //object obj = Activator.CreateInstance(o, true);//Create instance based on type
                //return (T)obj;//Type conversion and return
            }
            catch (Exception ex)
            {
                if (recordLog)
                {
                    SenparcTrace.BaseExceptionLog(ex);
                }                //In case of exception, return default value of the type
                return default(T);
            }
        }

        /// <summary>
        /// Get static class property
        /// </summary>
        /// <param name="assemblyName">Assembly name where the type is located</param>
        /// <param name="nameSpace">Namespace where the type is located</param>
        /// <param name="className">Type name</param>
        /// <param name="memberName">Property name (case insensitive)</param>
        /// <param name="recordLog">Whether to log</param>
        /// <returns></returns>
        public static object GetStaticMember(string assemblyName, string nameSpace, string className, string memberName, bool recordLog = false)
        {
            try
            {
                string fullName = nameSpace + "." + className;//Namespace.TypeName
                string path = fullName + "," + assemblyName;//Namespace.TypeName, Assembly
                var type = Type.GetType(path);

                if (type == null)
                {
                    return null;
                }

                PropertyInfo[] props = type.GetProperties();
                var prop = props.FirstOrDefault(z => z.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));

                if (prop == null)
                {
                    return null;
                }

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

        /// <summary>
        /// Get static class property
        /// </summary>
        /// <param name="type">Type</param>
        /// <param name="memberName">Property name (case insensitive)</param>
        /// <param name="recordLog">Whether to log</param>
        /// <returns></returns>
        public static object GetStaticMember(Type type, string memberName, bool recordLog = false)
        {
            try
            {
                PropertyInfo[] props = type.GetProperties();
                var prop = props.FirstOrDefault(z => z.Name.Equals(memberName, StringComparison.OrdinalIgnoreCase));

                if (prop == null)
                {
                    return null;
                }
                
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

        /// <summary>
        /// Check if the type has a parameterless constructor
        /// </summary>
        /// <param name="type">Type to check</param>
        /// <returns></returns>
        public static bool HasParameterlessConstructor(Type type)
        {
            // Get all public and non-public constructors  
            ConstructorInfo[] constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            // Check if there is a parameterless constructor  
            foreach (ConstructorInfo constructor in constructors)
            {
                if (constructor.GetParameters().Length == 0)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
