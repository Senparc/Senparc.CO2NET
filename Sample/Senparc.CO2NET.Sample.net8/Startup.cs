//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Microsoft.OpenApi.Models;
//using Senparc.CO2NET.AspNet;
//using Senparc.CO2NET.Cache;
//using Senparc.CO2NET.Cache.Memcached;
//using Senparc.CO2NET.Helpers;
//using Senparc.CO2NET.RegisterServices;
//using Senparc.CO2NET.Sample.net8.Services;
//using Senparc.CO2NET.WebApi;
//using Senparc.CO2NET.WebApi.WebApiEngines;
//using Swashbuckle.AspNetCore.Annotations;
//using Swashbuckle.AspNetCore.SwaggerGen;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Reflection;

//namespace Senparc.CO2NET.Sample
//{
//    public class Startup
//    {
//        public Startup(IConfiguration configuration, IWebHostEnvironment env)
//        {
//            Configuration = configuration;
//            WebHostEnvironment = env;
//        }

//        public IConfiguration Configuration { get; }
//        public IWebHostEnvironment WebHostEnvironment { get; set; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {
//            services.AddControllersWithViews();

//            services.AddMemoryCache();// Required when using local cache
//            services.Add(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));// Required when using Memcached or Logger
//            var builder = services.AddMvcCore();
//            // Senparc.CO2NET global registration (required)
//            services.AddSenparcGlobalServices(Configuration);

//            #region WebApiEngine

//            // Ignore for testing; comment out the code below to see WeChat Official Account SDK APIs and comments
//            WebApi.Register.OmitCategoryList.Add(NeuChar.PlatformType.WeChat_OfficialAccount.ToString());

//            // Additional test entries
//            WebApi.Register.AdditionalClasses.Add(typeof(AdditionalType), "Additional");
//            WebApi.Register.AdditionalMethods.Add(typeof(AdditionalMethod).GetMethod("TestApi"), "Additional");
//            WebApi.Register.AdditionalMethods.Add(typeof(EncryptHelper).GetMethod("GetMD5", new[] { typeof(string), typeof(string) }), "Additional");

//            var docXmlPath = Path.Combine(WebHostEnvironment.ContentRootPath, "App_Data", "ApiDocXml");
//            services.AddAndInitDynamicApi(builder, options =>
//            {
//                options.DocXmlPath = docXmlPath;
//                options.DefaultRequestMethod = ApiRequestMethod.Get;
//                options.BaseApiControllerType = null;
//                options.CopyCustomAttributes = true;
//                options.TaskCount = Environment.ProcessorCount * 4;
//                options.ShowDetailApiLog = true;
//                options.AdditionalAttributeFunc = null;
//                options.ForbiddenExternalAccess = true;
//            });

//            #endregion

//            #region Standalone tests
//            services.AddScoped(typeof(ApiBindTestService));
//            services.AddScoped(typeof(EntityApiBindTestService));
//            var apiBindTestService = new ApiBindTestService();
//            apiBindTestService.DynamicBuild(services, builder);
//            #endregion

//            #region Swagger

//            //.NET Core 3.0 for Swagger https://www.thecodebuzz.com/swagger-api-documentation-in-net-core-3-0/

//            // Add Swagger
//            services.AddSwaggerGen(c =>
//            {
//                // Create documentation for each assembly
//                foreach (var apiAssembly in WebApiEngine.ApiAssemblyCollection)
//                {
//                    var version = WebApiEngine.ApiAssemblyVersions[apiAssembly.Key]; //neucharApiDocAssembly.Value.ImageRuntimeVersion;
//                    var docName = WebApiEngine.GetDocName(apiAssembly.Key);
//                    c.SwaggerDoc(docName, new OpenApiInfo
//                    {
//                        Title = $"CO2NET Dynamic WebApi Engine : {apiAssembly.Key}",
//                        //Version = $"v{version}",//"v16.5.4"
//                        Description = $"Senparc CO2NET WebApi dynamic engine ({apiAssembly.Key} - v{version})",
//                        //License = new OpenApiLicense()
//                        //{
//                        //    Name = "Apache License Version 2.0",
//                        //    Url = new Uri("https://github.com/JeffreySu/WeiXinMPSDK")
//                        //},

//                        Contact = new OpenApiContact()
//                        {
//                            Email = "zsu@senparc.com",
//                            Name = "Senparc Team",
//                            Url = new Uri("https://www.senparc.com")
//                        },
//                        //TermsOfService = new Uri("https://github.com/JeffreySu/WeiXinMPSDK")
//                    });

//                    //c.DocumentFilter<TagDescriptionsDocumentFilter>();
//                    var docXmlFile = Path.Combine(WebApiEngine.GetDynamicFilePath(docXmlPath), $"{WebApiEngine.ApiAssemblyNames[apiAssembly.Key]}.xml");
//                    if (File.Exists(docXmlFile))
//                    {
//                        c.IncludeXmlComments(docXmlFile);
//                    }
//                }

//                // Group display https://www.cnblogs.com/toiv/archive/2018/07/28/9379249.html
//                c.DocInclusionPredicate((docName, apiDesc) =>
//                {
//                    if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
//                    {
//                        return false;
//                    }

//                    // Get attributes on the method
//                    var catalogNames = methodInfo.GetCustomAttributes(true)
//                                              .OfType<SwaggerOperationAttribute>()
//                                              .Select(z => z.Tags[0].Split(':')[0]);

//                    if (catalogNames?.Count() == 0)
//                    {
//                        // Get attributes on the class
//                        catalogNames = methodInfo.DeclaringType.GetCustomAttributes(true)
//                        .OfType<SwaggerOperationAttribute>()
//                          .Select(z => z.Tags[0].Split(':')[0]);
//                    }

//                    if (catalogNames?.Count() == 0)
//                    {
//                        return false;// Hide entries that do not match
//                    }


//                    //docName: $"{neucharApiDocAssembly.Key}-v1"
//                    return catalogNames.Any(z => docName.StartsWith(z));
//                });

//                c.OrderActionsBy(z => z.RelativePath);
//                //c.DescribeAllEnumsAsStrings();// Display enums as strings
//                c.EnableAnnotations();
//                c.DocumentFilter<RemoveVerbsFilter>();
//                c.CustomSchemaIds(x => x.FullName);// Avoid error:InvalidOperationException: Can't use schemaId "$JsApiTicketResult" for type "$Senparc.Weixin.Open.Entities.JsApiTicketResult". The same schemaId was already used for type "$Senparc.Weixin.MP.Entities.JsApiTicketResult"

//                /* Login required; not considered for now — Jeffrey Su 2021.06.17
//                var oAuthDocName = "oauth2";// WeixinApiService.GetDocName(PlatformType.WeChat_OfficialAccount);

//                // Add authorization
//                var authorizationUrl = NeuChar.App.AppStore.Config.IsDebug
//                                               // Below is appPurchase Id; should be appId in production
//                                               //? "http://localhost:12222/App/LoginOAuth/Authorize/1002/"
//                                               //: "https://www.neuchar.com/App/LoginOAuth/Authorize/4664/";
//                                               // Correct appId below
//                                               ? "http://localhost:12222/App/LoginOAuth/Authorize?appId=xxx"
//                                               : "https://www.neuchar.com/App/LoginOAuth/Authorize?appId=3035";

//                c.AddSecurityDefinition(oAuthDocName,//"Bearer" 
//                    new OpenApiSecurityScheme
//                    {
//                        Description = "Please enter a Token starting with Bearer",
//                        Name = oAuthDocName,// "Authorization",
//                        In = ParameterLocation.Header,
//                        Type = SecuritySchemeType.OAuth2,
//                        //OpenIdConnectUrl = new Uri("https://www.neuchar.com/"),
//                        Flows = new OpenApiOAuthFlows()
//                        {
//                            Implicit = new OpenApiOAuthFlow()
//                            {
//                                AuthorizationUrl = new Uri(authorizationUrl),
//                                Scopes = new Dictionary<string, string> { { "swagger_api", "Demo API - full access" } }
//                            }
//                        }
//                    });

//                // Authentication; applied globally
//                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
//                {
//                    { new OpenApiSecurityScheme(){ Name =oAuthDocName//"Bearer"
//                    }, new List<string>() }
//                    //{ "Bearer", Enumerable.Empty<string>() }
//                });

//                //c.OperationFilter<AuthResponsesOperationFilter>();// AuthorizeAttribute filter

//                */

//            });
//            #endregion

//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IOptions<SenparcSetting> senparcSetting)
//        {
//            if (env.IsDevelopment())
//            {
//                app.UseDeveloperExceptionPage();
//            }
//            else
//            {
//                app.UseExceptionHandler("/Home/Error");
//                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//                app.UseHsts();
//            }
//            //app.UseHttpsRedirection();
//            app.UseStaticFiles();
//            app.UseRouting();

//            app.UseAuthorization();

//            // Start CO2NET global registration (required)
//            app.UseSenparcGlobal(env, senparcSetting.Value, register =>
//                {
//                    #region CO2NET global configuration

//                    #region Global cache configuration (as needed)

//                    // When one distributed cache serves multiple sites (app pools), use a namespace to isolate them (optional)
//                    register.ChangeDefaultCacheNamespace("CO2NETCache.net8.0");

//                    #region Configure and use Redis

//                    // Configure global Redis cache (optional, independent)
//                    var redisConfigurationStr = senparcSetting.Value.Cache_Redis_Configuration;
//                    var useRedis = !string.IsNullOrEmpty(redisConfigurationStr) && redisConfigurationStr != "Redis configuration";
//                    if (useRedis)// For convenience across environments this is conditional; in production the if can usually be ignored
//                    {
//                        /* Notes:
//                         * 1. Redis connection string is read from Config.SenparcSetting.Cache_Redis_Configuration automatically; skip SetConfigurationOption if unchanged
//                        /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
//                         */
//                        Senparc.CO2NET.Cache.CsRedis.Register.SetConfigurationOption(redisConfigurationStr);

//                        // Immediately switch global cache to Redis
//                        Senparc.CO2NET.Cache.CsRedis.Register.UseKeyValueRedisNow();// Key-value cache strategy (recommended)
//                        //Senparc.CO2NET.Cache.Redis.Register.UseHashRedisNow();// HashSet storage cache strategy

//                        // Or register a custom cache strategy explicitly
//                        //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisObjectCacheStrategy.Instance);// Key-value
//                        //CacheStrategyFactory.RegisterObjectCacheStrategy(() => RedisHashSetObjectCacheStrategy.Instance);//HashSet
//                    }
//                    // If Redis is not enabled here, in-memory cache remains the default 

//                    #endregion

//                    #region Configure and use Memcached

//                    // Configure Memcached cache (optional, independent)
//                    var memcachedConfigurationStr = senparcSetting.Value.Cache_Memcached_Configuration;
//                    var useMemcached = !string.IsNullOrEmpty(memcachedConfigurationStr) && memcachedConfigurationStr != "Memcached configuration";

//                    if (useMemcached) // For convenience across environments this is conditional; in production the if can usually be ignored
//                    {
//                        app.UseEnyimMemcached();

//                        /* Notes:
//                        * 1. Memcached connection string is read from Config.SenparcSetting.Cache_Memcached_Configuration automatically; skip SetConfigurationOption if unchanged
//                       /* 2. To override manually, use SetConfigurationOption below (config only, does not enable immediately)
//                        */
//                        Senparc.CO2NET.Cache.Memcached.Register.SetConfigurationOption(redisConfigurationStr);

//                        // Immediately switch global cache to Memcached
//                        Senparc.CO2NET.Cache.Memcached.Register.UseMemcachedNow();

//                        // Or register a custom cache strategy explicitly
//                        CacheStrategyFactory.RegisterObjectCacheStrategy(() => MemcachedObjectCacheStrategy.Instance);
//                    }

//                    #endregion

//                    #endregion

//                    #region Register trace log (optional, recommended)

//                    register.RegisterTraceLog(ConfigTraceLog);// Configure TraceLog

//                    #endregion

//                    #endregion
//                },

//            #region Scan custom extension cache

//                // Auto-scan custom extension cache (choose one)
//                autoScanExtensionCacheStrategies: true // Default true; can be omitted
//                                                       // Specify custom extension cache (choose one)
//                                                       //autoScanExtensionCacheStrategies: false, extensionCacheStrategiesFunc: () => GetExCacheStrategies(senparcSetting.Value)

//            #endregion
//            );

//            app.UseSwagger();
//            app.UseSwaggerUI(c =>
//            {
//                //c.DocumentTitle = "Senparc Weixin SDK Demo API";
//                c.InjectJavascript("/lib/jquery/dist/jquery.min.js");
//                c.InjectJavascript("/js/swagger.js");
//                //c.InjectJavascript("/js/tongji.js");
//                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);

//                foreach (var co2netApiDocAssembly in WebApiEngine.ApiAssemblyCollection)
//                {
//                    // TODO: actual dynamic version number
//                    var version = WebApiEngine.ApiAssemblyVersions[co2netApiDocAssembly.Key]; //neucharApiDocAssembly.Value.ImageRuntimeVersion;
//                    var docName = WebApiEngine.GetDocName(co2netApiDocAssembly.Key);

//                    //Console.WriteLine($"\tAdd {docName}");

//                    c.SwaggerEndpoint($"/swagger/{docName}/swagger.json", $"{co2netApiDocAssembly.Key}");
//                }

//                // OAuth https://www.cnblogs.com/miskis/p/10083985.html
//                c.OAuthClientId("e65ea785b96b442a919965ccf857aba3");// Client name
//                c.OAuthAppName("WeChat API Swagger Docs "); // Description
//            });


//            app.UseEndpoints(endpoints =>
//            {
//                endpoints.MapControllerRoute(
//                    name: "default",
//                    pattern: "{controller=Home}/{action=Index}/{id?}");
//            });

//        }

//        /// <summary>
//        /// Configure global trace log
//        /// </summary>
//        private void ConfigTraceLog()
//        {
//            // When Debug is enabled, logs are written under /App_Data/SenparcTraceLog/; disable in production

//            // If global IsDebug (Senparc.CO2NET.Config.IsDebug) is false, set true here; otherwise it stays true
//            CO2NET.Trace.SenparcTrace.SendCustomLog("System log", "System started");// Only effective when Senparc.CO2NET.Config.IsDebug = true

//            // Global custom log callback
//            CO2NET.Trace.SenparcTrace.OnLogFunc = () =>
//            {
//                // Code to run after each log event
//            };

//            CO2NET.Trace.SenparcTrace.OnBaseExceptionFunc = ex =>
//            {
//                // Code to run after each BaseException
//            };
//        }

//        /// <summary>
//        /// Get extension cache strategies
//        /// </summary>
//        /// <returns></returns>
//        private IList<IDomainExtensionCacheStrategy> GetExCacheStrategies(SenparcSetting senparcSetting)
//        {
//            var exContainerCacheStrategies = new List<IDomainExtensionCacheStrategy>();
//            senparcSetting = senparcSetting ?? new SenparcSetting();

//            // Note: the two if blocks below are demos for adding custom extension cache strategies,

//            #region Demo extension cache registration

//            /*

//            // Check whether Redis is available
//            var redisConfiguration = senparcSetting.Cache_Redis_Configuration;
//            if ((!string.IsNullOrEmpty(redisConfiguration) && redisConfiguration != "Redis configuration"))
//            {
//                exContainerCacheStrategies.Add(RedisContainerCacheStrategy.Instance);// Custom extension cache
//            }

//            // Check whether Memcached is available
//            var memcachedConfiguration = senparcSetting.Cache_Memcached_Configuration;
//            if ((!string.IsNullOrEmpty(memcachedConfiguration) && memcachedConfiguration != "Memcached configuration"))
//            {
//                exContainerCacheStrategies.Add(MemcachedContainerCacheStrategy.Instance);// TODO: throws if not configured
//            }
//            */

//            #endregion

//            // Extend with custom cache strategies

//            return exContainerCacheStrategies;
//        }


//        class RemoveVerbsFilter : IDocumentFilter
//        {
//            //public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
//            //{
//            //    foreach (PathItem path in swaggerDoc.paths.Values)
//            //    {
//            //        path.delete = null;
//            //        //path.get = null; // leaving GET in
//            //        path.head = null;
//            //        path.options = null;
//            //        path.patch = null;
//            //        path.post = null;
//            //        path.put = null;
//            //    }
//            //}

//            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
//            {
//                // Each definition switch takes a long time to reach here
//                return;
//                string platformType;
//                var title = swaggerDoc.Info.Title;

//                //if (title.Contains(PlatformType.WeChat_OfficialAccount.ToString()))
//                //{
//                //    platformType = PlatformType.WeChat_OfficialAccount.ToString();
//                //}
//                //else if (title.Contains(PlatformType.WeChat_Work.ToString()))
//                //{
//                //    platformType = PlatformType.WeChat_Work.ToString();
//                //}
//                //else if (title.Contains(PlatformType.WeChat_Open.ToString()))
//                //{
//                //    platformType = PlatformType.WeChat_Open.ToString();
//                //}
//                //else if (title.Contains(PlatformType.WeChat_MiniProgram.ToString()))
//                //{
//                //    platformType = PlatformType.WeChat_MiniProgram.ToString();
//                //}
//                ////else if (title.Contains(PlatformType.General.ToString()))
//                ////{
//                ////    platformType = PlatformType.General.ToString();
//                ////}
//                //else
//                //{
//                //    throw new NotImplementedException($"Unsupported PlatformType, Title: {title}");
//                //}

//                //var pathList = swaggerDoc.Paths.Keys.ToList();

//                //foreach (var path in pathList)
//                //{
//                //    if (!path.Contains(platformType))
//                //    {
//                //        // Remove API entries outside the current module
//                //        swaggerDoc.Paths.Remove(path);
//                //    }
//                //}

//                //SwaggerOperationAttribute
//                // Remove Schema entries
//                //var toRemoveSchema = context.SchemaRepository.Schemas.Where(z => !z.Key.Contains(platformType)).ToList();// Result is full deletion; test only
//                //foreach (var schema in toRemoveSchema)
//                //{
//                //    context.SchemaRepository.Schemas.Remove(schema.Key);
//                //}
//            }
//        }

//        //public class AuthResponsesOperationFilter : IOperationFilter
//        //{
//        //    public void Apply(OpenApiOperation operation, OperationFilterContext context)
//        //    {
//        //        // Check whether login attribute is present
//        //        var authAttributes = context.MethodInfo.DeclaringType.GetCustomAttributes(true)
//        //         .Union(context.MethodInfo.GetCustomAttributes(true))
//        //         .OfType<AuthorizeAttribute>().Any();

//        //        if (authAttributes)
//        //        {
//        //            operation.Responses.Add("401", new OpenApiResponse { Description = "Unauthorized" });
//        //            operation.Responses.Add("403", new OpenApiResponse { Description = "Forbidden" });
//        //            operation.Security = new List<OpenApiSecurityRequirement>
//        //            {
//        //                new OpenApiSecurityRequirement { { new OpenApiSecurityScheme() {  Name= "oauth2" }, new[] { "swagger_api" } }}
//        //            };
//        //        }
//        //    }
//        //}

//        // public class TagDescriptionsDocumentFilter : IDocumentFilter
//        // {
//        //     public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
//        //     {
//        //         swaggerDoc.Tags = new List<OpenApiTag> {
//        //     new OpenApiTag { Name = "Products", Description = "Browse/manage the product cata,og" },
//        //     new OpenApiTag { Name = "Orders", Description = "Submit orders" },
//        //};
//        //     }
//        // }

//    }
//}
