using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Senparc.CO2NET;
using Senparc.CO2NET.AspNet;
using Senparc.CO2NET.RegisterServices;
using Senparc.CO2NET.WebApi.WebApiEngines;
using System;
using System.IO;
using System.Text;

namespace Senparc.CO2NET.WebApi.Tests
{
    [TestClass]
    public class BaseTest
    {
        public IServiceCollection ServiceCollection { get; set; }
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }
        public IMvcCoreBuilder MvcCoreBuilder { get; set; }

        protected IRegisterService registerService;
        protected SenparcSetting _senparcSetting;

        protected Mock<Microsoft.Extensions.Hosting.IHostEnvironment /*IHostingEnvironment*/> _env;

        public BaseTest(bool initDynamicApi = false)
        {
            _env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            _env.Setup(z => z.ContentRootPath).Returns(() => Path.GetFullPath("..\\..\\..\\"));

            Init(initDynamicApi);
        }

        public void Init(bool initDynamicApi = false)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            RegisterServiceCollection(initDynamicApi);
            RegisterServiceStart();
            Console.WriteLine("Finish TaseTest Init");
        }

        /// <summary>
        /// Register IServiceCollection with MemoryCache
        /// </summary>
        public void RegisterServiceCollection(bool initDynamicApi = false)
        {
            ServiceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            //configBuilder.AddJsonFile("appsettings.json", false, false);
            var config = configBuilder.Build();
            Configuration = config;


            ServiceCollection.AddSenparcGlobalServices(config);

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);

            ServiceCollection.AddMemoryCache();//Use memory cache


            MvcCoreBuilder = ServiceCollection.AddMvcCore();

            if (initDynamicApi)
            {
                ServiceCollection.AddAndInitDynamicApi(MvcCoreBuilder);
            }

            //WebApiEngine wae = new WebApiEngine(new FindWeixinApiService(), 400);
            //wae.InitDynamicApi(builder, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "App_Data"));

            ServiceProvider = ServiceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// Register RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(bool autoScanExtensionCacheStrategies = false)
        {
            IApplicationBuilder app = new ApplicationBuilder(ServiceProvider);

            // Complete CO2NET global registration, done!
            app.UseSenparcGlobal(_env.Object, _senparcSetting, register =>
            {
            });
        }
    }
}
