using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET.RegisterServices;

namespace Senparc.CO2NET.Tests
{
    //[TestClass]
    public class BaseTest
    {
        public static IServiceProvider serviceProvider;
        protected static IRegisterService registerService;
        protected static SenparcSetting _senparcSetting;

        public BaseTest()
        {
            RegisterServiceCollection();

            RegisterServiceStart();
        }

        /// <summary>
        /// Register IServiceCollection and MemoryCache
        /// </summary>
        public static void RegisterServiceCollection()
        {
            var serviceCollection = new ServiceCollection();

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", false, false);
            var config = configBuilder.Build();
            serviceCollection.AddSenparcGlobalServices(config);

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);

            serviceCollection.AddMemoryCache();//Use memory cache

            serviceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Register RegisterService.Start()
        /// </summary>
        public static void RegisterServiceStart(bool autoScanExtensionCacheStrategies = false)
        {
            //Register
            var mockEnv = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            mockEnv.Setup(z => z.ContentRootPath).Returns(() => UnitTestHelper.RootPath);

            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(mockEnv.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            registerService.ChangeDefaultCacheNamespace("Senparc.CO2NET Tests");

            //Global use of Redis cache (recommended)
            var redisConfigurationStr = _senparcSetting.Cache_Redis_Configuration;
            var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "#{Cache_Redis_Configuration}#"/*Default value configuration*/;
            if (useRedis)//This is to handle different project requirements and judgment methods, providing a precise model, avoiding if statements and later modifications
            {
                /* Explanation
                 * 1. Redis configuration string information is automatically retrieved from Config.SenparcSetting.Cache_Redis_Configuration, no need to modify, update is easy
                /* 2. If manual modification is needed, use the SetConfigurationOption method to manually set Redis configuration information, easy to modify and update
                 */
                Senparc.CO2NET.Cache.Redis.Register.SetConfigurationOption(redisConfigurationStr);
                Console.WriteLine("Finish Redis Config");


                //Global cache strategy is set to Redis
                Senparc.CO2NET.Cache.Redis.Register.UseKeyValueRedisNow();//Recommended for high concurrency scenarios
                Console.WriteLine("Start Redis UseKeyValue Strategy");

                //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow(); // Use HashSet method for caching

                //You can also use the following method to set the current cache strategy
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance); // Default
                //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance); // HashSet
            }
        }
    }
}
