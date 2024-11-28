/*----------------------------------------------------------------
    Copyright (C) 2023 Senparc

    FileName: WebApiEngine.cs
    File Function Description: WebApi auto-generation engine


    Creation Identifier: Senparc - 20210627

    Modification Identifier: Senparc - 20211122
    Modification Description: v1.1 provides the ability to synchronize parameter attributes to dynamic APIs

    Modification Identifier: Senparc - 20241108
    Modification Description: v2.0.0-beta2 1. Add UseLowerCaseApiName to WebApiEngineOptions
                              2. Add unique WebApi name to duplicate method name
   
    Modification Identifier：Senparc - 20241119
    Modification Description：v3.0.0-beta3 reconstruction

    Modification Identifier：Senparc - 20241128
    Modification Description：v2.0.2-beta3 Add UseLowerCaseApiName property for SenparcSetting

----------------------------------------------------------------*/

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
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
    /// WebApi auto-generation engine
    /// </summary>
    public partial class WebApiEngine
    {
        public static ConcurrentDictionary<string, Assembly> ApiAssemblyCollection { get; set; } = new ConcurrentDictionary<string, Assembly>();

        public static ConcurrentDictionary<string, string> ApiAssemblyNames { get; private set; } = new ConcurrentDictionary<string, string>();
        public static ConcurrentDictionary<string, string> ApiAssemblyVersions { get; private set; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// API method additional attributes
        /// </summary>
        public static Func<MethodInfo, IEnumerable<CustomAttributeBuilder>> AdditionalAttributeFunc { get; internal set; }

        public static string GetDynamicFilePath(string apiXmlPath) => Path.Combine(apiXmlPath, "DynamicFiles");

        internal string DocXmlPath { get; set; }
        internal int TaskCount { get; set; }

        private bool _showDetailApiLog = false;
        private readonly Lazy<FindApiService> _findWeixinApiService;
        private readonly ApiRequestMethod _defaultRequestMethod;
        private readonly bool _copyCustomAttributes;
        private Type _typeOfApiBind = typeof(ApiBindAttribute);
        private Type _baseApiControllerType;
        private bool _addApiControllerAttribute = true;
        private bool _useLowerCaseApiName = false;

        public bool BuildXml => DocXmlPath != null;

        /// <summary>
        /// WebApiEngine
        /// </summary>
        /// <param name="options"> WebApiEngine configuration</param>
        public WebApiEngine(Action<WebApiEngineOptions> options = null)
        {
            WebApiEngineOptions opt = new();
            options?.Invoke(opt);

            _ = opt.DefaultRequestMethod == ApiRequestMethod.GlobalDefault ? throw new Exception($"{nameof(opt.DefaultRequestMethod)} 不能作为默认请求类型！") : true;

            DocXmlPath = opt.DocXmlPath;
            _findWeixinApiService = new Lazy<FindApiService>(new FindApiService());
            _defaultRequestMethod = opt.DefaultRequestMethod;
            _baseApiControllerType = opt.BaseApiControllerType ?? typeof(ControllerBase);
            _copyCustomAttributes = opt.CopyCustomAttributes;
            TaskCount = opt.TaskCount;
            _showDetailApiLog = opt.ShowDetailApiLog;
            _addApiControllerAttribute = opt.AddApiControllerAttribute;
            _useLowerCaseApiName = opt.UseLowerCaseApiName ?? Config.SenparcSetting.UseLowerCaseApiName ?? false;
            Register.ForbiddenExternalAccess = opt.ForbiddenExternalAccess;
            WebApiEngine.AdditionalAttributeFunc = opt.AdditionalAttributeFunc;
        }

        /// <summary>
        /// Console print log
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="hideLog"></param>
        internal void WriteLog(string msg, bool hideLog = false)
        {
            if (!hideLog || _showDetailApiLog)
            {
                Console.WriteLine($"[{Thread.CurrentThread.ManagedThreadId:00}] {SystemTime.Now:yyyy-MM-dd HH:mm:ss.ffff}\t{msg}");
            }
        }

        #region Create dynamic assembly related



        /// <summary>
        /// Create dynamic WebApi
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

            #region Dynamically create assembly

            var dynamicAssembly = BuildDynamicAssembly(assembleName, apiBindGroup);

            #endregion

            //TODO: Open all types

            var apiBindFilterList = apiBindGroup.Where(z => //!z.Value.GlobalName.EndsWith("Async")
                                                            //&& z.Value.MethodInfo.ReturnType != typeof(Task<>)
                                                            //&& 
                                     z.Value.MethodInfo.ReturnType != typeof(void)
                                     && !z.Value.MethodInfo.IsGenericMethod //SemanticApi.SemanticSend is a generic method

                                     //Temporary filter IEnumerable objects   —— Jeffrey Su 2021.06.17
                                     && !z.Value.MethodInfo.GetParameters().Any(z =>
                                                        z.IsOut ||
                                                        z.ParameterType.Name.Contains("IEnumerable") ||
                                                        z.ParameterType.Name.Contains("IList`1")))
                                .OrderBy(z => z.Value.GlobalName)
                                .ToArray();

            //Move CommonApi to the top
            //Func<KeyValuePair<string, ApiBindInfo>, bool> funcCommonApi = z => z.Value.ApiBindAttribute.Name.StartsWith("CommonApi.");
            //var commonApiList = filterList.Where(z => funcCommonApi(z)).ToList();
            //filterList.RemoveAll(z => funcCommonApi(z));
            //filterList.InsertRange(0, commonApiList);

            int apiIndex = 0;
            ConcurrentDictionary<string, string> apiMethodName = new ConcurrentDictionary<string, string>();

            List<Task> taskList = new List<Task>();

            //Pre-allocate the tasks (index range) for each thread
            var apiFilterMaxIndex = apiBindFilterList.Length - 1;//Maximum index
            var avgBlockCount = (int)((apiBindFilterList.Length - 1) / TaskCount);//Average number of tasks per thread (block)
            var lastEndIndex = -1;//End index of the previous block

            for (int taskIndex = 0; taskIndex < TaskCount; taskIndex++)
            {
                if (lastEndIndex >= apiFilterMaxIndex)
                {
                    break;//Fully allocated, terminate
                }

                var blockStart = Math.Min(lastEndIndex + 1, apiFilterMaxIndex);//Start index of the current block
                var blockEnd = 0;//End index of the current block
                if (taskIndex == TaskCount - 1 || /*Last block, allocate until the end (solve remainder issue)*/
                    avgBlockCount == 0                  /*If the total number of APIs is less than the number of threads, only one module is enough*/)
                {
                    blockEnd = apiFilterMaxIndex;//
                }
                else
                {
                    blockEnd = Math.Min(blockStart + avgBlockCount, apiFilterMaxIndex);//Not the last block, take the average number
                }
                lastEndIndex = blockEnd;//Record the current block position

                var apiTask = Task.Factory.StartNew(async () =>
                {
                    Range blockRange = blockStart..(blockEnd + 1);
                    var apiBindInfoBlock = apiBindFilterList[blockRange];//Take a segment and assign it to the current task

                    apiIndex++;

                    #region Create API methods

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

            //WeixinApiAssembly = myType.Assembly;//Note: This will repeatedly assign the same object, which does not affect efficiency

            ApiAssemblyCollection[category] = dynamicAssembly.Mb.Assembly;//Store assembly

            var timeCost = SystemTime.NowDiff(dt1);

            WriteLog($"==== Finish BuildWebApi for {category} / Total Time: {timeCost.TotalMilliseconds:###,###} ms ====");
            WriteLog("");

            return apiMethodName.Count;
        }

        /// <summary>
        /// Generate API methods (Method) for a single thread (or Task), smallest granularity
        /// </summary>
        private async Task BuildApiMethodForOneThread(IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup,
            KeyValuePair<string, ApiBindInfo>[] apiBindInfoBlock, ConcurrentDictionary<string, string> apiMethodName, string controllerKeyName,
            TypeBuilder tb, FieldBuilder fbServiceProvider, int apiIndex)
        {
            foreach (var apiBindInfoKv in apiBindInfoBlock)
            {
                #region ApiBuild

                try
                {
                    //if (apiIndex > 9999)//200-250
                    //{
                    //    return;//For small range analysis
                    //}

                    var category = apiBindGroup.Key;
                    var apiBindInfo = apiBindInfoKv.Value;

                    //Define version number
                    if (!ApiAssemblyVersions.ContainsKey(category))
                    {
                        ApiAssemblyVersions[category] = apiBindInfo.MethodInfo.DeclaringType.Assembly.GetName().Version.ToString(3);
                    }

                    //Current method name
                    var globalName = apiBindInfo.GlobalName;

                    var methodName = apiBindInfo.MethodName;
                    //var apiBindGlobalName = globalName.Split('.')[0];
                    var apiBindName = apiBindInfo.ApiBindName;
                    var apiName = apiBindInfo.ApiName;

                    //Current API's MethodInfo
                    MethodInfo apiMethodInfo = apiBindInfo.MethodInfo;
                    //All parameter information of the current API
                    var parameters = apiMethodInfo.GetParameters();

                    var apiLog = $"> Search DynamicApi[{apiIndex}]: {controllerKeyName} > ";
                    var prefixIndex = apiLog.Length;//For alignment indentation
                    apiLog += $"{category}";

                    WriteLog(apiLog, true);
                    WriteLog($"-> {methodName} - Parameters: {parameters.Count()}".PadLeft(prefixIndex), true);

                    Func<string> getMethodUniqueNo = () => parameters.Sum(z => z.Name.Length + z.ParameterType.Name.Length).ToString();

                    //Ensure the name is not duplicated
                    while (apiMethodName.ContainsKey(methodName))
                    {
                        //开发过程中可能会因为接口增加，导致重复名称的后缀改变，因此使用相对差异更大的方式增加后缀（将所有参数名、类型的字符串长度相加）
                        //TODO：这种做法仍然无法解决第一个名称的命名问题（需要转回去修改）
                        methodName += "_" + getMethodUniqueNo;
                        apiName += "_" + getMethodUniqueNo;
                    }
                    apiMethodName[methodName] = apiName;

                    //Add static method marker
                    string showStaticApiState = null;//$"{(apiMethodInfo.IsStatic ? "_StaticApi" : "_NonStaticApi")}";

                    MethodBuilder setPropMthdBldr =
                        tb.DefineMethod(methodName/* + showStaticApiState*/, MethodAttributes.Public | MethodAttributes.Virtual,
                        apiMethodInfo.ReturnType, //Return type
                        parameters.Select(z => z.ParameterType).ToArray()//Input parameters
                        );

                    //Controller has already used SwaggerOperationAttribute once
                    var t2_3 = typeof(SwaggerOperationAttribute);
                    var tagName = new[] { $"{controllerKeyName}:{apiBindName}" };
                    var tagAttrBuilder = new CustomAttributeBuilder(t2_3.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                        new object[] { (string)null, (string)null },
                        new[] { t2_3.GetProperty("Tags") }, new[] { tagName });
                    setPropMthdBldr.SetCustomAttribute(tagAttrBuilder);
                    //Other method sorting methods refer to: https://stackoverflow.com/questions/34175018/grouping-of-api-methods-in-documentation-is-there-some-custom-attribute

                    //TODO:

                    //[Route("/api/...", Name="xxx")]
                    var t2_4 = typeof(RouteAttribute);
                    //var routeName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
                    string apiPath = GetApiPath(apiBindInfo, showStaticApiState);

                    //强制所有名称小写
                    if (_useLowerCaseApiName)
                    {
                        apiPath = apiPath.ToLower();
                    }

                    var routeAttrBuilder = new CustomAttributeBuilder(t2_4.GetConstructor(new Type[] { typeof(string) }),
                        new object[] { apiPath }/*, new[] { t2_2.GetProperty("Name") }, new[] { routeName }*/);
                    setPropMthdBldr.SetCustomAttribute(routeAttrBuilder);

                    //TODO: Customize from ApiBind

                    WriteLog($"Added DynamicApi Path: {apiPath}{System.Environment.NewLine}", true);

                    //[HttpPost]
                    var specialMethod = apiBindInfo.ApiBindAttribute.ApiRequestMethod;
                    if (specialMethod == ApiRequestMethod.GlobalDefault)
                    {
                        specialMethod = _defaultRequestMethod;//Use global default
                    }
                    Type tActionMethod = GetRequestMethodAttribute(specialMethod);

                    setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(tActionMethod.GetConstructor(new Type[0]), new object[0]));

                    //Add default existing attributes
                    if (_copyCustomAttributes)
                    {
                        //Custom attributes on the class     TODO: Cache to increase efficiency
                        var classAttrs = CustomAttributeData.GetCustomAttributes(apiMethodInfo.DeclaringType).ToList();
                        //Reverse array
                        classAttrs.Reverse();

                        //Custom attributes of the current method
                        var customAttrs = CustomAttributeData.GetCustomAttributes(apiMethodInfo).ToList();
                        foreach (var classAttr in classAttrs)
                        {
                            if (customAttrs.FirstOrDefault(z => z.AttributeType == classAttr.AttributeType) == null)
                            {
                                customAttrs.Insert(0, classAttr);
                            }
                        }

                        //Overlay class and method attributes
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

                    //Add user-defined attributes
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


                    //User restriction  ——  Jeffrey Su 2021.06.18
                    //var t4 = typeof(UserAuthorizeAttribute);//[UserAuthorize("UserOnly")]
                    //setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(t4.GetConstructor(new Type[] { typeof(string) }), new[] { "UserOnly" }));


                    //Set return type
                    //setPropMthdBldr.SetReturnType(apiMethodInfo.ReturnType);

                    //Set parameters
                    var boundSourceMetadata = false;//Parameters use attributes like [FromBody]
                    var boundClassType = false;//Parameters already bind class complex types
                    //Define other parameters
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        var p = parameters[i];
                        ParameterBuilder pb = setPropMthdBldr.DefineParameter(i + 1/*Start from 1, 0 is the return value*/, p.Attributes, p.Name);
                        //Handle parameters, otherwise throw an exception for complex type parameters: InvalidOperationException: Action 'WeChat_OfficialAccountController.CardApi_GetOrderList (WeixinApiAssembly)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:


                        boundSourceMetadata = boundSourceMetadata || typeof(IBindingSourceMetadata).IsAssignableFrom(p.ParameterType);

                        //Copy and add all attributes on a single parameter
                        try
                        {
                            var paramAttrs = p.CustomAttributes;// CustomAttributeData.GetCustomAttributes(p.ParameterType).ToList();
                            foreach (var item in paramAttrs)
                            {
                                var attrBuilder = new CustomAttributeBuilder(item.Constructor, item.ConstructorArguments.Select(z => z.Value).ToArray());
                                pb.SetCustomAttribute(attrBuilder);
                            }
                        }
                        catch (Exception)
                        {
                            //TODO: Collect error information
                            //throw;
                        }

                        try
                        {
                            if (p.ParameterType.IsClass && !boundClassType)
                            {
                                boundClassType = true;//First binding, can be ignored
                            }
                            else if (boundClassType && !boundSourceMetadata)
                            {
                                //Start using tags from the second one     TODO: More types can be customized
                                var tFromQuery = typeof(FromQueryAttribute);
                                pb.SetCustomAttribute(new CustomAttributeBuilder(tFromQuery.GetConstructor(new Type[0]), new object[0]));
                            }

                            //if (!boundSourceMetadata && p.ParameterType.IsClass)
                            //{
                            //    if (boundClassType == false)
                            //    {

                            //    }
                            //    else
                            //    {

                            //    }
                            //}
                        }
                        catch (Exception)
                        {
                            //throw;
                        }

                        try
                        {
                            //Set default value
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

                    //Execute specific method (body)
                    BuildMethodBody(apiMethodInfo, setPropMthdBldr, parameters, fbServiceProvider);

                    //var dt1 = SystemTime.Now;
                    //Modify XML document
                    await BuildXmlDoc(category, methodName, apiMethodInfo, tb);
                    //WriteLog($"methodName document modification time: {SystemTime.DiffTotalMS(dt1)}ms");
                }
                catch (Exception ex)
                {
                    //Encounter an error
                    WriteLog($"==== Error ====\r\n \t{ex}");
                }
                #endregion

            }
        }

        public static string GetApiPath(ApiBindInfo apiBindInfo, string showStaticApiState)
        {
            return GetApiPath(apiBindInfo.ControllerKeyName, apiBindInfo.ApiBindName, apiBindInfo.ApiName, showStaticApiState);
        }

        public static string GetApiPath(string keyName, string apiBindName, string apiName, string showStaticApiState)
        {
            var apiBindGroupNamePath = apiBindName.Replace(":", "_");
            var apiNamePath = apiName.Replace(":", "_");
            var apiPath = $"/api/{keyName}/{apiBindGroupNamePath}/{apiNamePath}{showStaticApiState}";
            return apiPath;
        }

        /// <summary>
        /// Create internal call of API method
        /// </summary>
        /// <param name="apiMethodInfo"></param>
        /// <param name="setPropMthdBldr"></param>
        /// <param name="parameters"></param>
        /// <param name="fbServiceProvider"></param>
        private void BuildMethodBody(MethodInfo apiMethodInfo, MethodBuilder setPropMthdBldr, ParameterInfo[] parameters, FieldBuilder fbServiceProvider)
        {
            //Execute specific method (body)
            var il = setPropMthdBldr.GetILGenerator();

            //FieldBuilder fb = tb.DefineField("id", typeof(System.String), FieldAttributes.Private);

            //var methodInfo = apiMethodInfo;
            LocalBuilder local = null;
            if (apiMethodInfo.ReturnType != typeof(void))
            {
                //il.Emit(OpCodes.Ldstr, "The I.M implementation of C");
                local = il.DeclareLocal(apiMethodInfo.ReturnType); // create a local variable
                                                                   //il.Emit(OpCodes.Ldarg_0);
                                                                   //Dynamically create fields   
            }

            if (apiMethodInfo.IsStatic)
            {
                //Static method


                //il.Emit(OpCodes.Ldarg_0); // this  //Static methods do not need to use this
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

                ////if (apiMethodInfo.GetType() == apiMethodInfo.DeclaringType)//Note: Using different methods here will result in different exceptions
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
                //Non-static method
                var invokeClassType = apiMethodInfo.DeclaringType;


                il.Emit(OpCodes.Nop); // the first one in arguments list
                il.Emit(OpCodes.Ldarg_0); // this  //Static methods do not need to use this
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
        /// Create dynamic assembly
        /// </summary>
        /// <param name="assembleName"></param>
        /// <param name="apiBindGroup"></param>
        /// <returns></returns>
        private BuildDynamicAssemblyResult BuildDynamicAssembly(string assembleName, IGrouping<string, KeyValuePair<string, ApiBindInfo>> apiBindGroup)
        {
            var category = apiBindGroup.Key;

            //Dynamically create assembly
            AssemblyName dynamicApiAssembly = new AssemblyName(assembleName); //Assembly.GetExecutingAssembly().GetName();// new AssemblyName("DynamicAssembly");
                                                                              //AppDomain currentDomain = Thread.GetDomain();
            AssemblyBuilder dynamicAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicApiAssembly, AssemblyBuilderAccess.RunAndCollect);

            //Dynamically create module
            ModuleBuilder mb = dynamicAssemblyBuilder.DefineDynamicModule(dynamicApiAssembly.Name);

            //Store API
            //_apiCollection[category] = new Dictionary<string, ApiBindInfo>(apiBindGroup);
            var controllerKeyName = ApiBindInfo.GetControllerKeyName(category);//Do not change the rules arbitrarily, global consistency is required

            WriteLog($"search key: {category} -> {controllerKeyName}", true);

            //Dynamically create class XXController
            var controllerClassName = $"{controllerKeyName}Controller";
            Type baseApiControllerType = apiBindGroup
                                            .Where(z => z.Value.BaseApiControllerType != null)
                                            .OrderByDescending(z => z.Value.BaseApiControllerOrder)
                                            .Take(1)
                                            .Select(z => z.Value.BaseApiControllerType)
                                            .FirstOrDefault()
                                         ?? this._baseApiControllerType;

            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, baseApiControllerType /*typeof(ControllerBase)*/ /*typeof(Controller)*/);

            //Private variable
            var fbServiceProvider = tb.DefineField("_serviceProvider", typeof(IServiceProvider), FieldAttributes.Private | FieldAttributes.InitOnly);

            //Set constructor
            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(IServiceProvider) });
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg, 0);
            //Define the reflection ConstructorInfor for System.Object
            ConstructorInfo conObj = typeof(object).GetConstructor(new Type[0]);
            ctorIl.Emit(OpCodes.Call, conObj);//Call the default ctor of base
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Ldarg, 0);
            ctorIl.Emit(OpCodes.Ldarg, 1);
            ctorIl.Emit(OpCodes.Stfld, fbServiceProvider);
            ctorIl.Emit(OpCodes.Ret);

            //ConstructorBuilder constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
            //Multiple Get parameters in the same Controller may cause issues: NotSupportedException: HTTP method "GET" & path "api/WeChat_OfficialAccount/CommonApi_CreateMenu" overloaded by actions - WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly),WeChat_OfficialAccountController.CommonApi_CreateMenu (WeixinApiAssembly). Actions require unique method/path combination for OpenAPI 3.0. Use ConflictingActionsResolver as a workaround

            /*The following method will throw an exception if it encounters parameters containing IEnumerable<T>:
             * InvalidOperationException: Action 'WeChat_MiniProgramController.TcbApi_UpdateIndex (NeuCharDocApi.WeChat_MiniProgram)' has more than one parameter that was specified or inferred as bound from request body. Only one parameter per action may be bound from body. Inspect the following parameters, and use 'FromQueryAttribute' to specify bound from query, 'FromRouteAttribute' to specify bound from route, and 'FromBodyAttribute' for parameters to be bound from body:
            IEnumerable<CreateIndex> create_indexes
            IEnumerable<DropIndex> drop_indexes
             */
            if (_addApiControllerAttribute)
            {
                var t = typeof(ApiControllerAttribute);
                tb.SetCustomAttribute(new CustomAttributeBuilder(t.GetConstructor(new Type[0]), new object[0]));
            }

            //Temporarily cancel login verification  —— Jeffrey Su 2021.06.18
            //var t_0 = typeof(AuthorizeAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t_0.GetConstructor(new Type[0]), new object[0]));


            var t2 = typeof(RouteAttribute);
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"/api/{controllerKeyName}" }));

            //TODO:Unit Test
            //[ForbiddenExternalAccess]
            if (Register.ForbiddenExternalAccess)
            {
                var forbiddenExternalAsyncAttr = typeof(ForbiddenExternalAccessAsyncFilter);
                tb.SetCustomAttribute(new CustomAttributeBuilder(forbiddenExternalAsyncAttr.GetConstructor(new Type[0]), new object[0] { }));//Only one is needed, interchangeable with ForbiddenExternalAccessFilter
                //var forbiddenExternalAttr = typeof(ForbiddenExternalAccessFilter);
                //tb.SetCustomAttribute(new CustomAttributeBuilder(forbiddenExternalAttr.GetConstructor(new Type[0]), new object[0] { }));
            }

            //Add Controller-level classification (temporarily ineffective)

            //TODO: External injection

            //var t2_0 = typeof(SwaggerOperationAttribute);
            //var t2_0_tagName = new[] { controllerKeyName };
            //var t2_0_tagAttrBuilder = new CustomAttributeBuilder(t2_0.GetConstructor(new Type[] { typeof(string), typeof(string) }),
            //    new object[] { (string)null, (string)null },
            //    new[] { t2_0.GetProperty("Tags") }, new[] { t2_0_tagName });
            //tb.SetCustomAttribute(t2_0_tagAttrBuilder);

            ////Add return type tag https://docs.microsoft.com/zh-cn/aspnet/core/tutorials/getting-started-with-swashbuckle?view=aspnetcore-2.2&tabs=visual-studio
            //var t2_1 = typeof(ProducesAttribute);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t2_1.GetConstructor(new Type[] { typeof(string), typeof(string[]) }), new object[] { "application/json", (string[])null }));

            ////Add GroupName for Controller
            //var t2_2 = typeof(ApiExplorerSettingsAttribute);
            //var t2_2_groupName = category.ToString();
            //var t2_2_tagAttrBuilder = new CustomAttributeBuilder(t2_2.GetConstructor(new Type[0]), new object[0],
            //    new[] { t2_0.GetProperty("GroupName") }, new[] { t2_2_groupName });
            //tb.SetCustomAttribute(t2_2_tagAttrBuilder);

            return new BuildDynamicAssemblyResult(dynamicAssemblyBuilder, mb, tb, fbServiceProvider, controllerKeyName);
        }


        /// <summary>
        /// Get WeixinApiAssembly assembly object
        /// </summary>
        /// <returns></returns>
        public Assembly GetApiAssembly(string category)
        {
            return ApiAssemblyCollection[category];
        }

        /// <summary>
        /// Get corresponding Http request attribute through ApiRequestMethod enum
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
                _ => typeof(HttpPostAttribute),//Default to use Post
            };
        }

        #endregion

    }
}
