using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Senparc.CO2NET.WebApi.Tests
{
    [TestClass]
    public class BaseTest
    {
        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; set; }
        public IConfiguration Configuration { get; set; }

        protected IRegisterService registerService;
        protected SenparcSetting _senparcSetting;

        protected Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/> _env;

        public BaseTest()
        {
            _env = new Mock<Microsoft.Extensions.Hosting.IHostEnvironment/*IHostingEnvironment*/>();
            _env.Setup(z => z.ContentRootPath).Returns(() => Path.GetFullPath("..\\..\\..\\"));

            //SiteConfig.WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");
            //ServiceCollection = new ServiceCollection();
            //var result = ServiceCollection.StartEngine(Configuration);

        }

        public void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            RegisterServiceCollection();
            RegisterServiceStart();
            Console.WriteLine("完成 TaseTest 初始化");
        }

        /// <summary>
        /// 注册 IServiceCollection 和 MemoryCache
        /// </summary>
        public void RegisterServiceCollection()
        {
            var serviceCollection = new ServiceCollection();
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appsettings.json", false, false);
            var config = configBuilder.Build();
            Configuration = config;

            _senparcSetting = new SenparcSetting() { IsDebug = true };
            config.GetSection("SenparcSetting").Bind(_senparcSetting);

            serviceCollection.AddDatabase<SqliteMemoryDatabaseConfiguration>();//使用 SQLServer数据库


            SiteConfig.WebRootPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot");

            Senparc.Ncf.Core.Register.TryRegisterMiniCore(services => { });
            serviceCollection.AddSenparcGlobalServices(config);

            serviceCollection.AddMemoryCache();//使用内存缓存
            serviceCollection.AddRouting();
            var builder = serviceCollection.AddRazorPages();
            builder.AddNcfAreas(_env.Object);

            //自动依赖注入扫描
            serviceCollection.ScanAssamblesForAutoDI();
            //已经添加完所有程序集自动扫描的委托，立即执行扫描（必须）
            AssembleScanHelper.RunScan();


            var result = serviceCollection.StartEngine(Configuration);
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        /// <summary>
        /// 注册 RegisterService.Start()
        /// </summary>
        public void RegisterServiceStart(bool autoScanExtensionCacheStrategies = false)
        {
            //注册
            registerService = Senparc.CO2NET.AspNet.RegisterServices.RegisterService.Start(_env.Object, _senparcSetting)
                .UseSenparcGlobal(autoScanExtensionCacheStrategies);

            IApplicationBuilder app = new ApplicationBuilder(ServiceProvider);
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
            });

            Console.WriteLine("Senparc.Ncf.XncfBase.Register.UseXncfModules");
            //XncfModules（必须）
            Senparc.Ncf.XncfBase.Register.UseXncfModules(app, registerService);
        }
    }
}
