/*----------------------------------------------------------------
    Copyright (C) 2021 Senparc

    文件名：WebApiEngine.cs
    文件功能描述：WebApi 自动生成引擎


    创建标识：Senparc - 20210627

----------------------------------------------------------------*/

using Microsoft.AspNetCore.Mvc;
using Senparc.CO2NET.WebApi.ActionFilters;
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
    /// <summary>
    /// WebApi 自动生成引擎
    /// </summary>
    public partial class WebApiEngine
    {
        public static ConcurrentDictionary<string, Assembly> ApiAssemblyCollection { get; set; } = new ConcurrentDictionary<string, Assembly>();

        public static ConcurrentDictionary<string, string> ApiAssemblyNames { get; private set; } = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> ApiAssemblyVersions { get; private set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// API 方法附加属性
        /// </summary>
        public static Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> AdditionalAttributeFunc { get; internal set; }

        public static string GetDynamicFilePath(string apiXmlPath) => Path.Combine(apiXmlPath, "DynamicFiles");

        private string _docXmlPath;
        private bool _showDetailApiLog = false;
        private readonly Lazy<FindApiService> _findWeixinApiService;
        private readonly ApiRequestMethod _defaultRequestMethod;
        private readonly bool _copyCustomAttributes;
        private int _taskCount;
        private Type _typeOfApiBind = typeof(ApiBindAttribute);
        private Type _baseApiControllerType;

        public bool BuildXml => _docXmlPath != null;

        /// <summary>
        /// WebApiEngine
        /// </summary>
        /// <param name="defaultRequestMethod">默认请求方式</param>
        /// <param name="baseApiControllerType">全局 ApiController 的基类，默认为 ControllerBase</param>
        /// <param name="taskCount">同时执行线程数</param>
        /// <param name="showDetailApiLog"></param>
        /// <param name="copyCustomAttributes"></param>
        /// <param name="defaultAction">默认请求类型，如 Post，Get</param>
        /// <param name="forbiddenExternalAccess">是否允许外部访问，默认为 false，只允许本机访问相关 API</param>
        public WebApiEngine(string docXmlPath, ApiRequestMethod defaultRequestMethod = ApiRequestMethod.Post, Type baseApiControllerType = null, bool copyCustomAttributes = true, int taskCount = 4, bool showDetailApiLog = false, bool forbiddenExternalAccess = true)
        {
            _docXmlPath = docXmlPath;
            _findWeixinApiService = new Lazy<FindApiService>(new FindApiService());
            _defaultRequestMethod = defaultRequestMethod;
            _baseApiControllerType = baseApiControllerType ?? typeof(ControllerBase);
            _copyCustomAttributes = copyCustomAttributes;
            _taskCount = taskCount;
            _showDetailApiLog = showDetailApiLog;
            Register.ForbiddenExternalAccess = forbiddenExternalAccess;
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
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] {SystemTime.Now:yyyy-MM-dd HH:mm:ss.ffff}\t\t{msg}");
            }
        }

        #region 创建动态程序集相关


        /// <summary>
        /// 从 apiBindInfo.Value.GlobalName 中匹配需要替换的关键字福
        /// </summary>
        private static Regex regexForMethodName = new Regex(@"[\.\-/:]", RegexOptions.Compiled);


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

            var assembleName = ApiAssemblyNames[category];

            #region 动态创建程序集

            var dynamicAssembly = BuildDynamicAssembly(assembleName, apiBindGroup);

            #endregion

            //TODO：开放所有类型

            var apiBindFilterList = apiBindGroup.Where(z => //!z.Value.GlobalName.EndsWith("Async")
                                                            //&& z.Value.MethodInfo.ReturnType != typeof(Task<>)
                                                            //&& 
                                     z.Value.MethodInfo.ReturnType != typeof(void)
                                     && !z.Value.MethodInfo.IsGenericMethod //SemanticApi.SemanticSend 是泛型方法

                                     //临时过滤 IEnumerable 对象   —— Jeffrey Su 2021.06.17
                                     && !z.Value.MethodInfo.GetParameters().Any(z =>
                                                        z.IsOut ||
                                                        z.ParameterType.Name.Contains("IEnumerable") ||
                                                        z.ParameterType.Name.Contains("IList`1")))
                                .OrderBy(z => z.Value.GlobalName)
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

                var apiTask = Task.Factory.StartNew(async () =>
                {
                    Range blockRange = blockStart..(blockEnd + 1);
                    var apiBindInfoBlock = apiBindFilterList[blockRange];//截取一段，分配给当前任务

                    apiIndex++;

                    #region 创建 API 方法

                    await BuildApiMethodForOneThread(apiBindGroup, apiBindInfoBlock, apiMethodName, dynamicAssembly.ControllerKeyName, dynamicAssembly.Tb,
                    dynamicAssembly.FbServiceProvider, apiIndex);

                    #endregion
                });
                taskList.Add(apiTask.Unwrap());
            }

            await Task.WhenAll(taskList);

            WriteLog("Api Task Count:" + taskList.Count, true);

            TypeInfo objectTypeInfo = dynamicAssembly.Tb.CreateTypeInfo();
            var myType = objectTypeInfo.AsType();

            WriteLog($"\t create type:  {myType.Namespace} - {myType.FullName}");

            //WeixinApiAssembly = myType.Assembly;//注意：此处会重复赋值相同的对象，不布偶股影响效率

            ApiAssemblyCollection[category] = dynamicAssembly.Mb.Assembly;//储存程序集

            var timeCost = SystemTime.NowDiff(dt1);

            WriteLog($"==== Finish BuildWebApi for {category} / Total Time: {timeCost.TotalMilliseconds:###,###} ms ====");
            WriteLog("");

            return apiMethodName.Count;
        }

        /// <summary>
        /// 生成单个线程（或 Task任务）的 API 方法（Method），最小粒度
        /// </summary>
        private async Task BuildApiMethodForOneThread(IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup,
            KeyValuePair<string, ApiBindInfo>[] apiBindInfoBlock, ConcurrentDictionary<string, string> apiMethodName, string keyName,
            TypeBuilder tb, FieldBuilder fbServiceProvider, int apiIndex)
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
                    var methodName = regexForMethodName.Replace(apiBindInfo.Value.GlobalName, "_");
                    var apiBindGlobalName = apiBindInfo.Value.GlobalName.Split('.')[0];
                    var apiBindName = apiBindInfo.Value.Name.Split('.')[0];
                    var indexOfApiGroupDot = apiBindInfo.Value.GlobalName.IndexOf(".");
                    var apiName = apiBindInfo.Value.GlobalName.Substring(indexOfApiGroupDot + 1, apiBindInfo.Value.GlobalName.Length - indexOfApiGroupDot - 1);

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

                    //添加静态方法的标记
                    string showStaticApiState = null;//$"{(apiMethodInfo.IsStatic ? "_StaticApi" : "_NonStaticApi")}";

                    MethodBuilder setPropMthdBldr =
                        tb.DefineMethod(methodName/* + showStaticApiState*/, MethodAttributes.Public | MethodAttributes.Virtual,
                        apiMethodInfo.ReturnType, //返回类型
                        parameters.Select(z => z.ParameterType).ToArray()//输入参数
                        );

                    //Controller已经使用过一次SwaggerOperationAttribute
                    var t2_3 = typeof(SwaggerOperationAttribute);
                    var tagName = new[] { $"{keyName}:{apiBindName}" };
                    var tagAttrBuilder = new CustomAttributeBuilder(t2_3.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                        new object[] { (string)null, (string)null },
                        new[] { t2_3.GetProperty("Tags") }, new[] { tagName });
                    setPropMthdBldr.SetCustomAttribute(tagAttrBuilder);
                    //其他Method排序方法参考：https://stackoverflow.com/questions/34175018/grouping-of-api-methods-in-documentation-is-there-some-custom-attribute

                    //TODO:

                    //[Route("/api/...", Name="xxx")]
                    var t2_4 = typeof(RouteAttribute);
                    //var routeName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    var apiBindGroupNamePath = apiBindName.Replace(":", "_");
                    var apiNamePath = apiName.Replace(":", "_");
                    var apiPath = $"/api/{keyName}/{apiBindGroupNamePath}/{apiNamePath}{showStaticApiState}";
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

                    //添加默认已有特性
                    if (_copyCustomAttributes)
                    {
                        //类上的自定义特性     TODO：缓存以增加效率
                        var classAttrs = CustomAttributeData.GetCustomAttributes(apiMethodInfo.DeclaringType).ToList();
                        //反转数组
                        classAttrs.Reverse();

                        //当前方法的自定义特性
                        var customAttrs = CustomAttributeData.GetCustomAttributes(apiMethodInfo).ToList();
                        foreach (var classAttr in classAttrs)
                        {
                            if (customAttrs.FirstOrDefault(z => z.AttributeType == classAttr.AttributeType) == null)
                            {
                                customAttrs.Insert(0, classAttr);
                            }
                        }

                        //叠加类和特性的方法

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
                    BuildMethodBody(apiMethodInfo, setPropMthdBldr, parameters, fbServiceProvider);

                    //var dt1 = SystemTime.Now;
                    //修改XML文档
                    await BuildXmlDoc(category, methodName, apiMethodInfo, tb);
                    //WriteLog($"methodName 文档修改耗时：{SystemTime.DiffTotalMS(dt1)}ms");
                }
                catch (Exception ex)
                {
                    //遇到错误
                    WriteLog($"==== Error ====\r\n \t{ex}");
                }
                #endregion

            }
        }

        /// <summary>
        /// 创建 API 方法内部调用
        /// </summary>
        /// <param name="apiMethodInfo"></param>
        /// <param name="setPropMthdBldr"></param>
        /// <param name="parameters"></param>
        /// <param name="fbServiceProvider"></param>
        private void BuildMethodBody(MethodInfo apiMethodInfo, MethodBuilder setPropMthdBldr, ParameterInfo[] parameters, FieldBuilder fbServiceProvider)
        {
            //执行具体方法（body）
            var il = setPropMthdBldr.GetILGenerator();

            //FieldBuilder fb = tb.DefineField("id", typeof(System.String), FieldAttributes.Private);

            //var methodInfo = apiMethodInfo;
            LocalBuilder local = null;
            if (apiMethodInfo.ReturnType != typeof(void))
            {
                //il.Emit(OpCodes.Ldstr, "The I.M implementation of C");
                local = il.DeclareLocal(apiMethodInfo.ReturnType); // create a local variable
                                                                   //il.Emit(OpCodes.Ldarg_0);
                                                                   //动态创建字段   
            }

            if (apiMethodInfo.IsStatic)
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

                il.Emit(OpCodes.Call, apiMethodInfo);

                ////if (apiMethodInfo.GetType() == apiMethodInfo.DeclaringType)//注意：此处使用不同的方法，会出现不同的异常
                //if (typeof(Senparc.Weixin.MP.CommonAPIs.CommonApi) == methodInfo.DeclaringType)
                //    il.Emit(OpCodes.Call, methodInfo);
                //else
                //    il.Emit(OpCodes.Callvirt, methodInfo);

                if (apiMethodInfo.ReturnType != typeof(void))
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
                var invokeClassType = apiMethodInfo.DeclaringType;


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
                il.Emit(OpCodes.Callvirt, apiMethodInfo);
                //il.Emit(OpCodes.Stloc, local);
                //il.Emit(OpCodes.Ldloc, local);
                //il.Emit(OpCodes.Ret);

                if (apiMethodInfo.ReturnType != typeof(void))
                {
                    il.Emit(OpCodes.Stloc, local); // set local variable
                    il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                                                   //il.Emit(OpCodes.Pop);
                }

                il.Emit(OpCodes.Ret);
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
            var controllerKeyName = category.Replace(":", "_");//不要随意改规则，全局需要保持一致

            WriteLog($"search key: {category} -> {controllerKeyName}", true);

            //动态创建类 XXController
            var controllerClassName = $"{controllerKeyName}Controller";
            Type baseApiControllerType = apiBindGroup
                                            .Where(z => z.Value.BaseApiControllerType != null)
                                            .OrderByDescending(z => z.Value.BaseApiControllerOrder)
                                            .Take(1)
                                            .Select(z => z.Value.BaseApiControllerType)
                                            .FirstOrDefault()
                                         ?? this._baseApiControllerType;

            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, baseApiControllerType /*typeof(ControllerBase)*/ /*typeof(Controller)*/);

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
            //多个Get参数放在同一个Controller中可能发生问题：NotSupportedException: HTTP method "GET" & path "api/WeChat_OfficialAccount/CommonApi_CreateMenu" overloaded by actions - WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly),WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly). Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround

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
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"/api/{controllerKeyName}" }));

            //TODO:Unit Test
            //[ForbiddenExternalAccess]
            if (Register.ForbiddenExternalAccess)
            {
                var forbiddenExternalAsyncAttr = typeof(ForbiddenExternalAccessAsyncFilter);
                tb.SetCustomAttribute(new CustomAttributeBuilder(forbiddenExternalAsyncAttr.GetConstructor(new Type[0]), new object[0] { }));//只需要一个，和ForbiddenExternalAccessFilter两者可互换
                //var forbiddenExternalAttr = typeof(ForbiddenExternalAccessFilter);
                //tb.SetCustomAttribute(new CustomAttributeBuilder(forbiddenExternalAttr.GetConstructor(new Type[0]), new object[0] { }));
            }

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
        private Type GetRequestMethodAttribute(ApiRequestMethod apiRequestMethod)
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

        #endregion

    }
}
