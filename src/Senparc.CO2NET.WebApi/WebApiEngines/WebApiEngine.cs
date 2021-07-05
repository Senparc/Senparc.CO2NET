using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Senparc.CO2NET.WebApi
{
    public class WebApiEngine
    {
        //private static ConcurrentDictionary<string, Dictionary<string, ApiBindInfo>> _apiCollection = new ConcurrentDictionary<string, Dictionary<string, ApiBindInfo>>();

        public static ConcurrentDictionary<string, Assembly> ApiAssemblyCollection { get; set; } = new ConcurrentDictionary<string, Assembly>();

        public static ConcurrentDictionary<string, string> ApiAssemblyNames { get; private set; } = new ConcurrentDictionary<string, string>(); //= "WeixinApiAssembly";
        public static ConcurrentDictionary<string, string> ApiAssemblyVersions { get; private set; } = new ConcurrentDictionary<string, string>(); //= "WeixinApiAssembly";

        /// <summary>
        /// API 方法附加属性
        /// </summary>
        public static Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> AdditionalAttributeFunc { get; internal set; }

        private bool _showDetailApiLog = false;
        private readonly Lazy<FindApiService> _findWeixinApiService;
        private readonly ApiRequestMethod _defaultRequestMethod;
        private readonly bool _copyCustomAttributes;
        private int _taskCount;
        private Type _typeOfApiBind = typeof(ApiBindAttribute);

        /// <summary>
        /// WebApiEngine
        /// </summary>
        /// <param name="taskCount">同时执行线程数</param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="copyCustomAttributes"></param>
        /// <param name="defaultAction">默认请求类型，如 Post，Get</param>
        public WebApiEngine(ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, bool copyCustomAttributes = true, int taskCount = 4, bool showDetailApiLog = false)
        {
            _findWeixinApiService = new Lazy<FindApiService>(new FindApiService());
            _defaultRequestMethod = defaultRequestMethod;
            _copyCustomAttributes = copyCustomAttributes;
            _taskCount = taskCount;
            _showDetailApiLog = showDetailApiLog;

            //测试时关闭部分模块

            //TODO：注册预载入模块

            //WeixinApiAssemblyNames[PlatformType.WeChat_OfficialAccount] = $"NeuCharDocApi.{PlatformType.WeChat_OfficialAccount}";
            //WeixinApiAssemblyNames[PlatformType.WeChat_MiniProgram] = $"NeuCharDocApi.{PlatformType.WeChat_MiniProgram}";
            //WeixinApiAssemblyNames[PlatformType.WeChat_Open] = $"NeuCharDocApi.{PlatformType.WeChat_Open}";
            //WeixinApiAssemblyNames[PlatformType.WeChat_Work] = $"NeuCharDocApi.{WeCPlatformType.WeChat_Work}";
            //WeixinApiAssemblyNames[PlatformType.General] = $"NeuCharDocApi.{PlatformType.General}";
            //WeixinApiAssemblyNames[PlatformType.WeChat_Tenpay] = $"NeuCharDocApi.{PlatformType.WeChat_Tenpay}";
        }

        /// <summary>
        /// 控制台打印日志
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="hideLog"></param>
        internal void WriteLog(string msg, bool hideLog = false)
        {
            if (!hideLog || _showDetailApiLog)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] {SystemTime.Now:yyyy-MM-dd HH:ss:mm.ffff}\t\t{msg}");
            }
        }

        /// <summary>
        /// 获取全局统一 docName
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public static string GetDocName(string category)
        {
            return $"{category}-v{ApiAssemblyVersions[category]}";
        }


        /// <summary>
        /// 检查并创建物理目录
        /// </summary>
        /// <param name="appDataPath">App_Data 文件夹路径</param>
        internal void TryCreateDir(string appDataPath)
        {
            var dir = Path.Combine(/*SiteConfig.WebRootPath, "..", "App_Data",*/ appDataPath, "ApiDocXml");// ServerUtility.ContentRootMapPath("~/App_Data/ApiDocXml");
            WriteLog($"检查目录：{dir}");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
                WriteLog($"创建目录：{dir}");
            }
        }

        /// <summary>
        /// 从 xml 中获取方法名和参数的正则
        /// </summary>
        private static Regex regex = new Regex(@"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})", RegexOptions.Compiled);


        /// <summary>
        /// 获取 DocName
        /// </summary>
        /// <param name="nameAttr"></param>
        /// <returns></returns>
        public DocMethodInfo GetDocMethodInfo(XAttribute nameAttr)
        {
            var pattern = @"(M\:)(?<docName>[^(]+)(?<paramsPart>\({1}.+\){1})";
            var result = regex.Match(pattern);
            if (result.Success && result.Groups["docName"] != null && result.Groups["paramsPart"] != null)
            {
                return new DocMethodInfo(result.Groups["docName"].Value, result.Groups["paramsPart"].Value);
            }

            return new DocMethodInfo(null, null);

            //以下方法速度略慢：
            /*
            if (xmlAttr.StartsWith("M:"))
            {
                var methodLastIndex = xmlAttr.IndexOf("(");
                return (xmlAttr[2..methodLastIndex], xmlAttr[methodLastIndex..]);
            }

            return (null, null);
            */
        }

        /// <summary>
        /// 生成单个线程（或 Task任务）的 API 操作（最小粒度）
        /// </summary>
        private void BuildApiForOneThread(IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup,
            KeyValuePair<string, ApiBindInfo>[] apiBindInfoBlock, ConcurrentDictionary<string, string> apiMethodName, string keyName,
            TypeBuilder tb, FieldBuilder fbServiceProvider, ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection, int apiIndex)
        {
            foreach (var apiBindInfo in apiBindInfoBlock)
            {
                #region ApiBuild

                try
                {
                    //if (apiIndex > 9999)//200-250
                    //{
                    //    return;//用于小范围分析
                    //}

                    var category = apiBindGroup.Key;

                    //定义版本号
                    if (!ApiAssemblyVersions.ContainsKey(category))
                    {
                        ApiAssemblyVersions[category] = apiBindInfo.Value.MethodInfo.DeclaringType.Assembly.GetName().Version.ToString(3);
                    }

                    //当前方法名称
                    var methodName = apiBindInfo.Value.ApiBindAttribute.Name.Replace(".", "_").Replace("-", "_").Replace("/", "_");
                    var apiBindGroupName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    var indexOfApiGroupDot = apiBindInfo.Value.ApiBindAttribute.Name.IndexOf(".");
                    var apiName = apiBindInfo.Value.ApiBindAttribute.Name.Substring(indexOfApiGroupDot + 1, apiBindInfo.Value.ApiBindAttribute.Name.Length - indexOfApiGroupDot - 1);

                    //确保名称不会有重复
                    while (apiMethodName.ContainsKey(methodName))
                    {
                        methodName += "0";
                        apiName += "0";
                    }
                    apiMethodName[methodName] = apiName;

                    //当前 API 的 MethodInfo
                    MethodInfo apiMethodInfo = apiBindInfo.Value.MethodInfo;
                    //当前 API 的所有参数信息
                    var parameters = apiMethodInfo.GetParameters();

                    WriteLog($"\t search API[{apiIndex}]: {keyName} > {apiBindInfo.Key} -> {methodName} \t\t Parameters Count: {parameters.Count()}\t\t", true);

                    MethodBuilder setPropMthdBldr =
                        tb.DefineMethod(methodName + (apiMethodInfo.IsStatic ? "_StaticApi" : "_NotStaticApi"), MethodAttributes.Public | MethodAttributes.Virtual,
                        apiMethodInfo.ReturnType, //返回类型
                        parameters.Select(z => z.ParameterType).ToArray()//输入参数
                        );

                    //Controller已经使用过一次SwaggerOperationAttribute
                    var t2_3 = typeof(SwaggerOperationAttribute);
                    var tagName = new[] { $"{keyName}:{apiBindGroupName}" };
                    var tagAttrBuilder = new CustomAttributeBuilder(t2_3.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                        new object[] { (string)null, (string)null },
                        new[] { t2_3.GetProperty("Tags") }, new[] { tagName });
                    setPropMthdBldr.SetCustomAttribute(tagAttrBuilder);
                    //其他Method排序方法参考：https://stackoverflow.com/questions/34175018/grouping-of-api-methods-in-documentation-is-there-some-custom-attribute

                    //TODO:

                    //[Route("/wxapi/...", Name="xxx")]
                    var t2_4 = typeof(RouteAttribute);
                    //var routeName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    var showStaticApiState = $"{(apiMethodInfo.IsStatic ? "_StaticApi" : "_NonStaticApi")}";
                    var apiPath = $"/wxapi/{keyName}/{apiBindGroupName}/{apiName}{showStaticApiState}";
                    var routeAttrBuilder = new CustomAttributeBuilder(t2_4.GetConstructor(new Type[] { typeof(string) }),
                        new object[] { apiPath }/*, new[] { t2_2.GetProperty("Name") }, new[] { routeName }*/);
                    setPropMthdBldr.SetCustomAttribute(routeAttrBuilder);

                    //TODO:从ApiBind中自定义

                    WriteLog($"added Api path: {apiPath}", true);

                    //[HttpPost]
                    var specialMethod = apiBindInfo.Value.ApiBindAttribute.ApiRequestMethod;
                    if (specialMethod == ApiRequestMethod.GlobalDefault)
                    {
                        specialMethod = _defaultRequestMethod;//使用全局默认
                    }
                    Type tActionMethod = GetRequestMethodAttribute(specialMethod);

                    setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(tActionMethod.GetConstructor(new Type[0]), new object[0]));

                    //TODO：复制 Class 特性 -> 创建接口或类进行集中接口定义

                    //添加默认已有特性
                    if (_copyCustomAttributes)
                    {
                        var customAttrs = CustomAttributeData.GetCustomAttributes(apiMethodInfo);

                        foreach (var item in customAttrs)
                        {
                            if (item.AttributeType == _typeOfApiBind)
                            {
                                continue;
                            }

                            var attrBuilder = new CustomAttributeBuilder(item.Constructor, item.ConstructorArguments.Select(z => z.Value).ToArray());
                            setPropMthdBldr.SetCustomAttribute(attrBuilder);
                        }
                    }

                    //添加用户自定义特性
                    if (AdditionalAttributeFunc != null)
                    {
                        var additionalAttrs = AdditionalAttributeFunc(apiMethodInfo);
                        if (additionalAttrs != null)
                        {
                            foreach (var item in additionalAttrs)
                            {
                                setPropMthdBldr.SetCustomAttribute(item);
                            }
                        }
                    }


                    //用户限制  ——  Jeffrey Su 2021.06.18
                    //var t4 = typeof(UserAuthorizeAttribute);//[UserAuthorize("UserOnly")]
                    //setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(t4.GetConstructor(new Type[] { typeof(string) }), new[] { "UserOnly" }));


                    //设置返回类型
                    //setPropMthdBldr.SetReturnType(apiMethodInfo.ReturnType);

                    //设置参数
                    var boundType = false;
                    //定义其他参数
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        ParameterBuilder pb = setPropMthdBldr.DefineParameter(i + 1/*从1开始，0为返回值*/, p.Attributes, p.Name);
                        //处理参数，反之出现复杂类型的参数，抛出异常：InvalidOperationException: Action 'WeChat_OfficialAccountController.CardApi_GetOrderList (WeixinApiAssembly)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:
                        if (p.ParameterType.IsClass)
                        {
                            if (boundType == false)
                            {
                                //第一个绑定，可以不处理
                                boundType = true;
                            }
                            else
                            {
                                //第二个开始使用标签
                                var tFromQuery = typeof(FromQueryAttribute);
                                pb.SetCustomAttribute(new CustomAttributeBuilder(tFromQuery.GetConstructor(new Type[0]), new object[0]));
                            }
                        }
                        try
                        {
                            //设置默认值
                            if (p.HasDefaultValue)
                            {
                                pb.SetConstant(p.DefaultValue);
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }

                    //执行具体方法（body）
                    var il = setPropMthdBldr.GetILGenerator();

                    //FieldBuilder fb = tb.DefineField("id", typeof(System.String), FieldAttributes.Private);

                    var methodInfo = apiMethodInfo;

                    LocalBuilder local = null;
                    if (methodInfo.ReturnType != typeof(void))
                    {
                        //il.Emit(OpCodes.Ldstr, "The I.M implementation of C");
                        local = il.DeclareLocal(apiMethodInfo.ReturnType); // create a local variable
                                                                           //il.Emit(OpCodes.Ldarg_0);
                                                                           //动态创建字段   
                    }

                    if (methodInfo.IsStatic)
                    {
                        //静态方法


                        //il.Emit(OpCodes.Ldarg_0); // this  //静态方法不需要使用this
                        //il.Emit(OpCodes.Ldarg_1); // the first one in arguments list
                        il.Emit(OpCodes.Nop); // the first one in arguments list
                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var p = parameters[i];
                            //WriteLog($"\t\t Ldarg: {p.Name}\t isOptional:{p.IsOptional}\t defaultValue:{p.DefaultValue}");
                            il.Emit(OpCodes.Ldarg, i + 1); // the first one in arguments list
                        }

                        //WriteLog($"\t get static method: {methodInfo.Name}\t returnType:{methodInfo.ReturnType}");

                        il.Emit(OpCodes.Call, methodInfo);

                        ////if (apiMethodInfo.GetType() == apiMethodInfo.DeclaringType)//注意：此处使用不同的方法，会出现不同的异常
                        //if (typeof(Senparc.Weixin.MP.CommonAPIs.CommonApi) == methodInfo.DeclaringType)
                        //    il.Emit(OpCodes.Call, methodInfo);
                        //else
                        //    il.Emit(OpCodes.Callvirt, methodInfo);

                        if (methodInfo.ReturnType != typeof(void))
                        {
                            il.Emit(OpCodes.Stloc, local); // set local variable
                            il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                                                           //il.Emit(OpCodes.Pop);
                        }

                        il.Emit(OpCodes.Ret);
                    }
                    else
                    {
                        //非静态方法
                        var invokeClassType = methodInfo.DeclaringType;


                        il.Emit(OpCodes.Nop); // the first one in arguments list
                        il.Emit(OpCodes.Ldarg_0); // this  //静态方法不需要使用this
                                                  //il.Emit(OpCodes.Ldarg_1); // the first one in arguments list


                        //il.Emit(OpCodes.Nop);
                        //il.Emit(OpCodes.Ldarg, 0);


                        il.Emit(OpCodes.Ldfld, fbServiceProvider);

                        il.Emit(OpCodes.Ldtoken, invokeClassType);
                        il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));

                        il.Emit(OpCodes.Callvirt, typeof(IServiceProvider).GetMethod("GetService"));
                        il.Emit(OpCodes.Isinst, invokeClassType);
                        il.Emit(OpCodes.Stloc, 0);
                        il.Emit(OpCodes.Ldloc, 0);

                        for (int i = 0; i < parameters.Length; i++)
                        {
                            var p = parameters[i];
                            //WriteLog($"\t\t Ldarg: {p.Name}\t isOptional:{p.IsOptional}\t defaultValue:{p.DefaultValue}");
                            il.Emit(OpCodes.Ldarg, i + 1); // the first one in arguments list
                        }
                        il.Emit(OpCodes.Callvirt, methodInfo);
                        //il.Emit(OpCodes.Stloc, local);
                        //il.Emit(OpCodes.Ldloc, local);
                        //il.Emit(OpCodes.Ret);

                        if (methodInfo.ReturnType != typeof(void))
                        {
                            il.Emit(OpCodes.Stloc, local); // set local variable
                            il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                                                           //il.Emit(OpCodes.Pop);
                        }

                        il.Emit(OpCodes.Ret);
                    }


                    //var dt1 = SystemTime.Now;
                    //修改XML文档
                    BuildXmlDoc(methodName, methodInfo, tb, docMembersCollection);
                    //WriteLog($"methodName 文档修改耗时：{SystemTime.DiffTotalMS(dt1)}ms");
                }
                catch (Exception ex)
                {
                    //遇到错误
                    WriteLog($"==== Error ====\r\n \t{ex.ToString()}");
                }
                #endregion

            }
        }

        /// <summary>
        /// 创建动态程序集
        /// </summary>
        /// <param name="assembleName"></param>
        /// <param name="apiBindGroup"></param>
        /// <returns></returns>
        private BuildDynamicAssemblyResult BuildDynamicAssembly(string assembleName, IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup)
        {
            var category = apiBindGroup.Key;

            //动态创建程序集
            AssemblyName dynamicApiAssembly = new AssemblyName(assembleName); //Assembly.GetExecutingAssembly().GetName();// new AssemblyName("DynamicAssembly");
                                                                              //AppDomain currentDomain = Thread.GetDomain();
            AssemblyBuilder dynamicAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicApiAssembly, AssemblyBuilderAccess.RunAndCollect);

            //动态创建模块
            ModuleBuilder mb = dynamicAssemblyBuilder.DefineDynamicModule(dynamicApiAssembly.Name);

            //储存 API
            //_apiCollection[category] = new Dictionary<string, ApiBindInfo>(apiBindGroup);
            var controllerKeyName = category.ToString();//不要随意改规则，全局需要保持一致

            WriteLog($"search key: {category} -> {controllerKeyName}", true);

            //动态创建类 XXController
            var controllerClassName = $"{controllerKeyName}Controller";
            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, typeof(ControllerBase) /*typeof(Controller)*/);

            //私有变量
            var fbServiceProvider = tb.DefineField("_serviceProvider", typeof(IServiceProvider), FieldAttributes.Private | FieldAttributes.InitOnly);

            //设置构造函数
            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(IServiceProvider) });
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg, 0);
            //Define the reflection ConstructorInfor for System.Object
            ConstructorInfo conObj = typeof(object).GetConstructor(new Type[0]);
            ctorIl.Emit(OpCodes.Call, conObj);//调用base的默认ctor
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Ldarg, 0);
            ctorIl.Emit(OpCodes.Ldarg, 1);
            ctorIl.Emit(OpCodes.Stfld, fbServiceProvider);
            ctorIl.Emit(OpCodes.Ret);

            //ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //多个Get参数放在同一个Controller中可能发生问题：NotSupportedException: HTTP method "GET" & path "wxapi/WeChat_OfficialAccount/CommonApi_CreateMenu" overloaded by actions - WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly),WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly). Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround

            /*以下方法如果遇到参数含有 IEnumerable<T> 参数，会抛出异常：
             * InvalidOperationException: Action 'WeChat_MiniProgramController.TcbApi_UpdateIndex (NeuCharDocApi.WeChat_MiniProgram)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:
            IEnumerable<CreateIndex> create_indexes
            IEnumerable<DropIndex> drop_indexes
             */
            //var t = typeof(ApiControllerAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t.GetConstructor(new Type[0]), new object[0]));


            //暂时取消登录验证  —— Jeffrey Su 2021.06.18
            //var t_0 = typeof(AuthorizeAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t_0.GetConstructor(new Type[0]), new object[0]));


            var t2 = typeof(RouteAttribute);
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"/wxapi/{controllerKeyName}" }));

            //添加Controller级别的分类（暂时无效果）

            //TODO:外部注入

            //var t2_0 = typeof(SwaggerOperationAttribute);
            //var t2_0_tagName = new[] { controllerKeyName };
            //var t2_0_tagAttrBuilder = new CustomAttributeBuilder(t2_0.GetConstructor(new Type[] { typeof(string), typeof(string) }),
            //    new object[] { (string)null, (string)null },
            //    new[] { t2_0.GetProperty("Tags") }, new[] { t2_0_tagName });
            //tb.SetCustomAttribute(t2_0_tagAttrBuilder);

            ////添加返回类型标签 https://docs.microsoft.com/zh-cn/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.2&tabs=visual-studio
            //var t2_1 = typeof(ProducesAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t2_1.GetConstructor(new Type[] { typeof(string), typeof(string[]) }), new object[] { "application/json", (string[])null }));

            ////添加针对Controller的GroupName
            //var t2_2 = typeof(ApiExplorerSettingsAttribute);
            //var t2_2_groupName = category.ToString();
            //var t2_2_tagAttrBuilder = new CustomAttributeBuilder(t2_2.GetConstructor(new Type[0]), new object[0],
            //    new[] { t2_0.GetProperty("GroupName") }, new[] { t2_2_groupName });
            //tb.SetCustomAttribute(t2_2_tagAttrBuilder);

            return new BuildDynamicAssemblyResult(dynamicAssemblyBuilder, mb, tb, fbServiceProvider, controllerKeyName);
        }

        /// <summary>
        /// 获取 XML 中所有 Member
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private async IAsyncEnumerable<XElement> GetXmlMembers(XElement root)
        {
            foreach (var item in root.Element("members").Elements("member"))
            {
                yield return item;
            }
        }

        /// <summary>
        /// 创建动态 WebApi
        /// </summary>
        public async Task<int> BuildWebApi(IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup)
        {
            var category = apiBindGroup.Key;
            var dt1 = SystemTime.Now;
            WriteLog("", true);
            WriteLog($"==== Begin BuildWebApi for {category} ====", true);

            if (apiBindGroup.Count() == 0 || !ApiAssemblyNames.ContainsKey(category))
            {
                WriteLog($"apiBindGroup 不存在可用对象: {category}");
                return 0;
            }

            var groupStartTime = SystemTime.Now;

            var xmlFileName = $"{apiBindGroup.First().Value.MethodInfo.DeclaringType.Assembly.GetName().Name}.xml";//XML 文件名
            var sourceName = $"Senparc.Xncf.WeixinManager.App_Data.ApiDocXml.{xmlFileName}";//嵌入资源地址
            var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(sourceName);

            var useXml = sourceStream?.Length > 0;
            var assembleName = ApiAssemblyNames[category];
            ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection = new ConcurrentDictionary<string, DocMembersCollectionValue>();
            XDocument document = null;
            if (useXml)
            {

                document = await XDocument.LoadAsync(sourceStream, LoadOptions.None, Task.Factory.CancellationToken);
                //XDocument document = XDocument.Load(sourceStream, LoadOptions.None);
                var root = document.Root;
                root.Element("assembly").Element("name").Value = assembleName;
                var docMembers = GetXmlMembers(root);// root.Element("members").Elements("member");

                double dtlong = 0;

                //var dtDoc = SystemTime.Now;
                await foreach (var x in docMembers)
                {
                    if (x.HasAttributes)
                    {
                        var nameAttr = x.FirstAttribute;
                        var dt00 = SystemTime.Now;
                        var docMethodInfo = GetDocMethodInfo(nameAttr);
                        dtlong += SystemTime.DiffTotalMS(dt00);
                        if (docMethodInfo.MethodName != null && docMethodInfo.ParamsPart != null)
                        {
                            //记录索引信息
                            docMembersCollection[docMethodInfo.MethodName] = new DocMembersCollectionValue(/*x, */nameAttr, docMethodInfo.ParamsPart);

                            //记录接口信息，用于搜索
                            var isAsync = docMethodInfo.MethodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase) ||
                                            docMethodInfo.MethodName.Contains("Async``", StringComparison.OrdinalIgnoreCase);//是否是异步方法
                            _findWeixinApiService.Value.RecordApiItem(category, docMethodInfo.MethodName, docMethodInfo.ParamsPart,
                                x.Element("summary")?.Value, isAsync);
                        }
                    }
                }
            }

            //WriteLog($"docMembersCollection init cost:{SystemTime.DiffTotalMS(dtDoc)}ms");

            //WriteLog("Document Root Name:" + root.Name);
            //WriteLog($"find docMembers:{docMembers.Count()}");

            #region 动态创建程序集

            var dynamicAssembly = BuildDynamicAssembly(assembleName, apiBindGroup);

            #endregion

            var apiBindFilterList = apiBindGroup.Where(z => !z.Value.ApiBindAttribute.Name.EndsWith("Async")
                                     && z.Value.MethodInfo.ReturnType != typeof(Task<>)
                                     && z.Value.MethodInfo.ReturnType != typeof(void)
                                     && !z.Value.MethodInfo.IsGenericMethod //SemanticApi.SemanticSend 是泛型方法

                                     //临时过滤 IEnumerable 对象   —— Jeffrey Su 2021.06.17
                                     && !z.Value.MethodInfo.GetParameters().Any(z =>
                                                        z.IsOut ||
                                                        z.ParameterType.Name.Contains("IEnumerable") ||
                                                        z.ParameterType.Name.Contains("IList`1")))
                                .OrderBy(z => z.Value.ApiBindAttribute.Name)
                                .ToArray();

            //把 CommonApi 提前到头部
            //Func<KeyValuePair<string, ApiBindInfo>, bool> funcCommonApi = z => z.Value.ApiBindAttribute.Name.StartsWith("CommonApi.");
            //var commonApiList = filterList.Where(z => funcCommonApi(z)).ToList();
            //filterList.RemoveAll(z => funcCommonApi(z));
            //filterList.InsertRange(0, commonApiList);

            int apiIndex = 0;
            ConcurrentDictionary<string, string> apiMethodName = new ConcurrentDictionary<string, string>();

            List<Task> taskList = new List<Task>();

            //预分配每个线程需要领取的任务（索引范围）
            var apiFilterMaxIndex = apiBindFilterList.Length - 1;//最大索引
            var avgBlockCount = (int)((apiBindFilterList.Length - 1) / _taskCount);//每个线程（块）领取的平均数量
            var lastEndIndex = -1;//上一个块的结束索引

            for (int taskIndex = 0; taskIndex < _taskCount; taskIndex++)
            {
                if (lastEndIndex >= apiFilterMaxIndex)
                {
                    break;//已经排满，终止
                }

                var blockStart = Math.Min(lastEndIndex + 1, apiFilterMaxIndex);//当前块起始索引
                var blockEnd = 0;//当前快结束索引
                if (taskIndex == _taskCount - 1 || /*最后一个快，一直分配到最后（解决余数问题）*/
                    avgBlockCount == 0                  /*如果API总数比线程数还要少，则只够一个模块*/)
                {
                    blockEnd = apiFilterMaxIndex;//
                }
                else
                {
                    blockEnd = Math.Min(blockStart + avgBlockCount, apiFilterMaxIndex);//非最后一个快，取平均数量
                }
                lastEndIndex = blockEnd;//记录当前快位置

                var apiTask = Task.Factory.StartNew(() =>
                {
                    Range blockRange = blockStart..(blockEnd + 1);
                    var apiBindInfoBlock = apiBindFilterList[blockRange];//截取一段，分配给当前任务

                    apiIndex++;

                    #region 创建 API 方法

                    BuildApiForOneThread(apiBindGroup, apiBindInfoBlock, apiMethodName, dynamicAssembly.ControllerKeyName, dynamicAssembly.Tb,
                    dynamicAssembly.FbServiceProvider, docMembersCollection, apiIndex);

                    #endregion
                });
                taskList.Add(apiTask);
            }

            await Task.WhenAll(taskList);

            WriteLog("Api Task Count:" + taskList.Count, true);

            TypeInfo objectTypeInfo = dynamicAssembly.Tb.CreateTypeInfo();
            var myType = objectTypeInfo.AsType();

            WriteLog($"\t create type:  {myType.Namespace} - {myType.FullName}");

            //WeixinApiAssembly = myType.Assembly;//注意：此处会重复赋值相同的对象，不布偶股影响效率

            #region 保存新的 Xml 文件
            if (useXml)
            {
                var newDocFileName = $"App_Data/ApiDocXml/{assembleName}.xml";
                try
                {
                    //using (XmlWriter xw = XmlWriter.Create(newDocFile, new XmlWriterSettings() { Async = true }))
                    //{
                    //    await document.SaveAsync(xw, new CancellationToken()).ConfigureAwait(false);//保存
                    //}
                    document.Save(newDocFileName);

                    WriteLog($"new document file saved: {newDocFileName}, assembly cost:{SystemTime.NowDiff(groupStartTime)}");
                }
                catch (Exception ex)
                {
                    WriteLog($"save document xml faild: {ex.Message}\r\n{ex.ToString()}");
                }

            }


            #endregion

            ApiAssemblyCollection[category] = dynamicAssembly.Mb.Assembly;//储存程序集

            var timeCost = SystemTime.NowDiff(dt1);

            WriteLog($"==== Finish BuildWebApi for {category} / Total Time: {timeCost.TotalMilliseconds:###,###} ms ====");
            WriteLog("");

            return apiMethodName.Count;
        }

        private void BuildXmlDoc(string methodName, MethodInfo methodInfo, TypeBuilder tb, ConcurrentDictionary<string, DocMembersCollectionValue> docMembersCollection)
        {
            //生成文档
            var docMethodName = $"{methodInfo.DeclaringType.FullName}.{methodInfo.Name}";//以(结尾确定匹配到完整的方法名

            WriteLog($"\t search for docName:  {docMethodName}");//\t\tSDK Method：{apiMethodInfo.ToString()}

            if (docMembersCollection.ContainsKey(docMethodName))
            {
                var docMethodInfo = docMembersCollection[docMethodName];
                // like: "M:Senparc.Weixin.MP.AdvancedAPIs.AnalysisApi.GetArticleSummary(System.String,System.String,System.String,System.Int32)"
                var newAttrName = $"M:{tb.FullName}.{methodName}{docMethodInfo.ParamsPart}";

                docMethodInfo.NameAttr.SetValue(newAttrName);
                //WriteLog($"\t change document name:  {attr.Value} -> {newAttrName}");
            }
        }


        /// <summary>
        /// 获取 WeixinApiAssembly 程序集对象
        /// </summary>
        /// <returns></returns>
        public Assembly GetApiAssembly(string category)
        {
            return ApiAssemblyCollection[category];
        }

        /// <summary>
        /// 通过 ApiRequestMethod 枚举获取对应的 Http 请求特性
        /// </summary>
        /// <param name="apiRequestMethod"></param>
        /// <returns></returns>
        public Type GetRequestMethodAttribute(ApiRequestMethod apiRequestMethod)
        {
            return apiRequestMethod switch
            {
                ApiRequestMethod.GlobalDefault => throw new CO2NET.Exceptions.HttpException($"{nameof(ApiRequestMethod.GlobalDefault)} 不是有效的请求类型"),
                ApiRequestMethod.Get => typeof(HttpGetAttribute),
                ApiRequestMethod.Head => typeof(HttpHeadAttribute),
                ApiRequestMethod.Post => typeof(HttpPostAttribute),
                ApiRequestMethod.Put => typeof(HttpPutAttribute),
                ApiRequestMethod.Delete => typeof(HttpDeleteAttribute),
                ApiRequestMethod.Options => typeof(HttpOptionsAttribute),
                ApiRequestMethod.Patch => typeof(HttpPatchAttribute),
                _ => typeof(HttpPostAttribute),//默认都使用 Post
            };
        }

    }
}
