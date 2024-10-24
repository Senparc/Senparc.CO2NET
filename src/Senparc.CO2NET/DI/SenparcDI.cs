
/*----------------------------------------------------------------
    Copyright (C) 2024 Senparc

    FileName: SenparcDI.cs
    File Function Description: Dependency injection extension class for .NET Core

    Creation Identifier: Senparc - 20180714


    Modification Identifier: Senparc - 20180802
    Modification Description: v0.2.5 Provide full support for netstandard2.0 in the methods of this class

    Modification Identifier: pengweiqhca - 20180802
    Modification Description: v0.2.8 Add SenparcDI.GetIServiceProvider() method to support other dependency injection frameworks

    Modification Identifier: Senparc - 20190118
    Modification Description: v0.5.2 Add SenparcDI.GetRequiredService() method to provide independent ServiceProvider instances within threads

    Modification Identifier: Senparc - 201901527
    Modification Description: v0.8.2 Add SenparcDI.ResetGlobalIServiceProvider(this IServiceCollection serviceCollection) method

    Modification Identifier: Senparc - 20210702
    Modification Description: 1.4.400.2 Add GetService() method

    Modification Identifier: Senparc - 20210702
    Modification Description: 1.4.400.2 Add GetService() method

----------------------------------------------------------------*/

#if !NET462
using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Senparc.CO2NET.Cache;

namespace Senparc.CO2NET
{
    /// <summary>
    /// Dependency injection extension class for .NET Core
    /// </summary>
    public static class SenparcDI
    {
        //public const string SENPARC_DI_THREAD_SERVICE_PROVIDER = "___SenparcDIThreadServiceProvider";
        public const string SENPARC_DI_THREAD_SERVICE_SCOPE = "___SenparcDIThreadScope";

        /// <summary>
        /// Global ServiceCollection
        /// </summary>
        public static IServiceCollection GlobalServiceCollection { get; set; }

        ///// <summary>
        ///// Create a new ServiceCollection object
        ///// </summary>
        ///// <returns></returns>
        //public static IServiceCollection GetServiceCollection()
        //{
        //    return GlobalServiceCollection;
        //}

        //private static object _globalIServiceProviderLock = new object();
        //private static object _threadIServiceProviderLock = new object();

        private static IServiceProvider _globalServiceProvider;


        /// <summary>
        /// Rebuild from GlobalServiceCollection to generate a new IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider()
        {
            return GetServiceProvider(false);
        }

        /// <summary>
        /// Rebuild from GlobalServiceCollection to generate a new IServiceProvider
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider GetServiceProvider(bool refresh = false)
        {
            if (_globalServiceProvider != null && !refresh)
            {
                return _globalServiceProvider;
            }
            _globalServiceProvider = GlobalServiceCollection.BuildServiceProvider();
            return _globalServiceProvider;
        }

        /// <summary>
        /// Single Scope ServiceScope within the thread
        /// </summary>
        public static IServiceScope ThreadServiceScope
        {
            get
            {
                var threadServiceScope = Thread.GetData(Thread.GetNamedDataSlot(SENPARC_DI_THREAD_SERVICE_SCOPE)) as IServiceScope;
                return threadServiceScope;
            }
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return GetServiceProvider().GetService(type);
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetService<T>()
        {
            return GetServiceProvider().GetService<T>();
        }


        /// <summary>
        /// Execute .GetRequiredService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static T GetRequiredService<T>()
        {
            return GetServiceProvider().GetRequiredService<T>();
        }


        /// <summary>
        /// Execute .GetRequiredKeyedService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetRequiredKeyedService(Type serviceType, object? serviceKey)
        {
            return GetServiceProvider().GetRequiredKeyedService(serviceType, serviceKey);
        }

        ///// <summary>
        ///// Use .net core default DI method to get instance (recommended)
        ///// <para>If not registered, throw an exception </para>
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T GetService<T>(this IServiceProvider serviceProvider)
        //{
        //    return (T)serviceProvider.GetService(typeof(T));
        //}


        /* Obsolete method

        /// <summary>
        /// Global IServiceCollection object
        /// </summary>
        [Obsolete("Obsolete, please use the system injection method", true)]
        public static IServiceProvider GlobalServiceProvider
        {
            get
            {
                return _globalServiceProvider ?? throw new Exception("Please use the services.AddSenparcGlobalServices() method to provide a globally unified ServiceProvider during the registration process in Startup.cs");
            }
            set
            {
                _globalServiceProvider = value;
            }
        }

        /// <summary>
        /// Execute .GetService() method via GetServiceProvider() method
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetService(Type type)
        {
            return GetServiceProvider().GetService(type);
        }


        ///// <summary>
        ///// Use .net core default DI method to get instance (recommended)
        ///// <para>If not registered, throw an exception </para>
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <returns></returns>
        //public static T GetService<T>(this IServiceProvider serviceProvider)
        //{
        //    return (T)serviceProvider.GetService(typeof(T));
        //}


        /* Obsolete method
        /// <summary>
        /// Get ServiceProvider
        /// </summary>
        /// <param name="useGlobalScope">Whether to use a globally unique ServiceScope object.
        /// <para>Default is true, which means using a globally unique ServiceScope.</para>
        /// <para>If false, use a thread-unique ServiceScope object</para>
        /// </param>
        /// <returns></returns>
        [Obsolete("No longer store this object, directly use the globally unified GlobalIServiceProvider", true)]
        public static IServiceProvider GetIServiceProvider(bool useGlobalScope = true)
        {
            if (useGlobalScope)
            {
                if (GlobalServiceProvider == null)
                {
                    //Lock to ensure uniqueness
                    lock (_globalIServiceProviderLock)
                    {
                        if (GlobalServiceProvider == null)
                        {
                            //Note: The BuildServiceProvider() method generates a different ServiceProvider object each time!
                            GlobalServiceProvider = GlobalServiceCollection.BuildServiceProvider();
                        }
                    }
                }
                return GlobalServiceProvider;
            }
            else
            {
                if (ThreadServiceScope == null)
                {
                    //Lock to ensure uniqueness
                    lock (_threadIServiceProviderLock)
                    {
                        if (ThreadServiceScope == null)
                        {
                            //Note: The BuildServiceProvider() method generates a different ServiceProvider object each time!
                            //GlobalIServiceProvider = GetServiceCollection().BuildServiceProvider();

                            var globalServiceProvider = GetIServiceProvider(true);

                            Thread.SetData(Thread.GetNamedDataSlot(SENPARC_DI_THREAD_SERVICE_Scope), globalServiceProvider.CreateScope());
                        }
                    }
                }
                return ThreadServiceScope.ServiceProvider;
            }
        }


        /// <summary>
        /// Use .net core default DI method to get instance
        /// <para>If not registered, return null</para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <param name="useGlobalScope">Whether to use a globally unique ServiceScope object, default is false, which means using a thread-unique ServiceScope object</param>
        [Obsolete("No longer store this object, directly use the globally unified GlobalIServiceProvider", true)]
        public static T GetService<T>(bool useGlobalScope = true)
        {
            return GetIServiceProvider(useGlobalScope).GetService<T>();
        }

        /// <summary>
        /// Use .net core default DI method to get instance (recommended)
        /// <para>If not registered, throw an exception </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("Obsolete", true)]
        public static T GetRequiredService<T>()
        {
            return GlobalServiceProvider.GetRequiredService<T>();
        }

        /// <summary>
        /// Use .net core default DI method to get instance (recommended)
        /// <para>If not registered, throw an exception </para>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        [Obsolete("Obsolete", true)]
        public static T GetService<T>()
        {
            return GlobalServiceProvider.GetService<T>();
        }


        /// <summary>
        /// Reset GlobalIServiceProvider object, regenerate the object from serviceCollection.BuildServiceProvider()
        /// </summary>
        [Obsolete("Obsolete", true)]
        public static IServiceProvider ResetGlobalIServiceProvider(this IServiceCollection serviceCollection)
        {
            GlobalServiceProvider = serviceCollection.BuildServiceProvider();
            return GlobalServiceProvider;
        }
        */
    }
}
#endif
