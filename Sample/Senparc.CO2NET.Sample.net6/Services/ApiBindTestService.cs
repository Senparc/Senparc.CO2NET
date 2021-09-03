using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Sample.net6.Services
{
    /// <summary>
    /// 用于测试动态生成 API 的 Service
    /// </summary>
    public class ApiBindTestService
    {
        public ApiBindTestService()
        {
        }

        /// <summary>
        /// 测试方法转接口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiBind("CO2NET", "ApiBindTest.TestApi", WebApi.ApiRequestMethod.Get)]
        [MyTest("TestCopyAttrFromTestApi")]
        //[AuthorizeAttribute()]
        public string TestApi(string name = "Senparc", int value = 678)
        {
            return $"[from ApiBindTestService.TestApi]{name}:{value}";
        }

        /// <summary>
        /// 测试异步方法转接口
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiBind("CO2NET", "ApiBindTest.TestApiAsync", WebApi.ApiRequestMethod.Post)]
        public async Task<string> TestApiAsync(string name, int value)
        {
            var msg = $"[{SystemTime.Now:HH:mm:ss.ffff}] [from ApiBindTestService.TestApiAsync] Method";
            await Task.Delay(1000);
            msg += $"[{SystemTime.Now:HH:mm:ss.ffff}] {name}:{value}";
            return msg;
        }

        /// <summary>
        /// 模拟加入 WeChat_OfficialAccount 的接口
        /// </summary>
        /// <param name="name">这里填写名称</param>
        /// <param name="value">这里填写值</param>
        /// <returns></returns>
        [ApiBind("WeChat_OfficialAccount", "A-WexinApi", WebApi.ApiRequestMethod.Post)]
        public static async Task<string> WexinApi(string name, int value)
        {
            var msg = $"[{SystemTime.Now:HH:mm:ss.ffff}] [from WeChat_OfficialAccount.WexinApi] Method";
            await Task.Delay(1000);
            msg += $"[{SystemTime.Now:HH:mm:ss.ffff}] {name}:{value}";
            return msg;
        }

        /// <summary>
        /// 动态构建API代码，部分核心代码测试
        /// </summary>
        public void DynamicBuild(IServiceCollection services, IMvcCoreBuilder builder)
        {

            var invokeClassType = typeof(EntityApiBindTestService);
            //var invokeClassType = typeof(StaticApiBindTestService);
            var invokeMethodName = "TestApiAsync";//TestApiAsync  or TestApi
            var invokeMethodInfo = invokeClassType.GetMethod(invokeMethodName);
            Console.WriteLine("======== DynamicBuild Start ========");
            Console.WriteLine($"invokeClassType: {invokeClassType.Name}");
            Console.WriteLine($"invokeMethodName: {invokeMethodName}");
            Console.WriteLine($"invokeMethod ReturnType: {invokeMethodInfo.ReturnType.Name}");

            #region 构造程序集


            AssemblyName dynamicApiAssembly = new AssemblyName("DynamicTests");
            //AppDomain currentDomain = Thread.GetDomain();
            AssemblyBuilder dynamicAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicApiAssembly, AssemblyBuilderAccess.RunAndCollect);

            //动态创建模块
            ModuleBuilder mb = dynamicAssemblyBuilder.DefineDynamicModule(dynamicApiAssembly.Name);

            //动态创建类 XXController
            var controllerClassName = $"DynamicTestController";
            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, typeof(ControllerBase) /*typeof(Controller)*/);

            //var t1 = typeof(ApiController);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t1.GetConstructor(new Type[] { typeof(string) }), new object[]  }));

            var t2 = typeof(RouteAttribute);
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"myapi/[controller]" }));

            //私有变量
            var fbServiceProvider = tb.DefineField("_serviceProvider", typeof(IServiceProvider), FieldAttributes.Private | FieldAttributes.InitOnly);

            #endregion

            #region 设置构造函数

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

            #endregion

            #region 设置方法

            //设置方法
            MethodBuilder setPropMthdBldr =
                      tb.DefineMethod("Tests", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                      invokeMethodInfo.ReturnType, //返回类型
                      new[] { typeof(string), typeof(int) }//输入参数
                      );

            //添加标签
            var t2_3 = typeof(SwaggerOperationAttribute);
            var tagName = new[] { $"DynamicTest:Test" };
            var tagAttrBuilder = new CustomAttributeBuilder(t2_3.GetConstructor(new Type[] { typeof(string), typeof(string) }),
                new object[] { (string)null, (string)null },
                new[] { t2_3.GetProperty("Tags") }, new[] { tagName });
            setPropMthdBldr.SetCustomAttribute(tagAttrBuilder);

            var t2_4 = typeof(RouteAttribute);
            //var routeName = apiBindInfo.Value.ApiBindAttribute.Name.Split('.')[0];
            var apiPath = $"/mywxapi/dynamic";
            var routeAttrBuilder = new CustomAttributeBuilder(t2_4.GetConstructor(new Type[] { typeof(string) }),
                new object[] { apiPath }/*, new[] { t2_2.GetProperty("Name") }, new[] { routeName }*/);
            setPropMthdBldr.SetCustomAttribute(routeAttrBuilder);

            //[HttpGet]
            var t3 = typeof(HttpGetAttribute);
            setPropMthdBldr.SetCustomAttribute(new CustomAttributeBuilder(t3.GetConstructor(new Type[0]), new object[0]));
            //var tFromQuery = typeof(FromQueryAttribute);
            //pb2.SetCustomAttribute(new CustomAttributeBuilder(tFromQuery.GetConstructor(new Type[0]), new object[0]));


            ParameterBuilder pb1 = setPropMthdBldr.DefineParameter(1, ParameterAttributes.None, "name");
            ParameterBuilder pb2 = setPropMthdBldr.DefineParameter(2, ParameterAttributes.None, "val");



            //复制特性
            var customAttrs = CustomAttributeData.GetCustomAttributes(invokeMethodInfo);

            foreach (var item in customAttrs)
            {
                if (item.AttributeType == typeof(ApiBindAttribute))
                {
                    continue;
                }

                var attrBuilder = new CustomAttributeBuilder(item.Constructor, item.ConstructorArguments.Select(z => z.Value).ToArray());
                setPropMthdBldr.SetCustomAttribute(attrBuilder);
            }

            #endregion

            #region 设置方法体（Body）

            //执行具体方法
            var il = setPropMthdBldr.GetILGenerator();
            LocalBuilder local = il.DeclareLocal(invokeMethodInfo.ReturnType); // create a local variable

            if (invokeClassType == typeof(EntityApiBindTestService) || !invokeMethodInfo.IsStatic)
            {
                //Label lblEnd = il.DefineLabel();

                /* 最简洁方法（独立使用）
                il.Emit(OpCodes.Nop);
                //il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldarg, 1);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ret);
                */

                //实例方法
                il.Emit(OpCodes.Nop);
                il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldfld, fbServiceProvider);
                il.Emit(OpCodes.Ldtoken, invokeClassType);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));
                il.Emit(OpCodes.Callvirt, typeof(IServiceProvider).GetMethod("GetService"));
                il.Emit(OpCodes.Isinst, invokeClassType);
                il.Emit(OpCodes.Stloc, 0);
                il.Emit(OpCodes.Ldloc, 0);
                il.Emit(OpCodes.Ldarg, 1);
                il.Emit(OpCodes.Ldarg, 2);
                il.Emit(OpCodes.Callvirt, invokeMethodInfo);
                il.Emit(OpCodes.Stloc, local);
                //il.Emit(OpCodes.Br_S, lblEnd);
                //il.MarkLabel(lblEnd);     
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                //静态方法调用

                il.Emit(OpCodes.Nop); // the first one in arguments list
                il.Emit(OpCodes.Ldarg, 1); // the first one in arguments list
                il.Emit(OpCodes.Ldarg, 2);
                il.Emit(OpCodes.Call, invokeMethodInfo);
                il.Emit(OpCodes.Stloc, local); // set local variable
                il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                //il.Emit(OpCodes.Stloc, 1);
                //Label lbl = il.DefineLabel();
                //il.Emit(OpCodes.Br_S, lbl);
                //il.MarkLabel(lbl);
                //il.Emit(OpCodes.Ldloc, 1);
                il.Emit(OpCodes.Ret);
            }


            #endregion

            var t = tb.CreateType();
            TypeInfo objectTypeInfo = tb.CreateTypeInfo();
            var myType = objectTypeInfo.AsType();
            services.AddScoped(myType);
            builder.AddApplicationPart(mb.Assembly);


            Console.WriteLine($"\t create type:  {myType.Namespace} - {myType.FullName}");
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var ctrl = scope.ServiceProvider.GetService(myType);
                Console.WriteLine(ctrl.GetType());
                var testMethod = ctrl.GetType().GetMethod("Tests");
                Console.WriteLine("testMethod.GetParameters().Count(): " + testMethod.GetParameters().Count());
                var result = testMethod.Invoke(ctrl, new object[] { "来自 ApiBindTestService.DynamicBuild() 方法，看到此信息表明自动生成 API 已成功", 1 });
                Console.WriteLine("result:" + result);
                Console.WriteLine("Attrs Name: " + string.Join('|', ctrl.GetType().GetMethod("Tests").GetCustomAttributes().Select(z => z.GetType().Name)));
            }
        }
    }

    /// <summary>
    /// 用于测试自动生成的 WebApi 方法内调用非静态方法，并且包含 IServiceProvider，使用 DI 自动注入
    /// </summary>
    public class EntityApiBindTestService
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityApiBindTestService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 用于测试自动生成的 WebApi 方法内调用非静态方法（同步方法），并且包含 IServiceProvider，使用 DI 自动注入。
        /// <para>同时测试自定义 Attribute</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        //[ApiBind("CO2NETEntity", "EntityApiBindTest.TestApi")]
        [MyTest("TestCopyAttrFromTestApi in EntityApiBindTestService class")]
        public string TestApi(string name, int value = 666)
        {
            var addMsg = "";

            var testService = _serviceProvider.GetService(typeof(ApiBindTestService)) as ApiBindTestService;
            addMsg = testService.TestApi(name, value);

            return $"[from EntityApiBindTestService.TestApi]{name}:{value} - {addMsg}";
        }

        /// <summary>
        /// 用于测试自动生成的 WebApi 方法内调用非静态方法（异步方法），并且包含 IServiceProvider，使用 DI 自动注入。
        /// <para>同时测试自定义 Attribute</para>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MyTest("TestCopyAttrFromTestApiAsync in EntityApiBindTestService class")]
        public async Task<string> TestApiAsync(string name = "Senparc", int value = 999)
        {
            var msg = $"[{SystemTime.Now:HH:mm:ss.ffff}] [from EntityApiBindTestService.TestApiAsync] Method";
            await Task.Delay(1000);
            msg += $"[{SystemTime.Now:HH:mm:ss.ffff}] {name}:{value}";
            return msg;
        }
    }

    /// <summary>
    /// 类上进行 ApiBind 绑定的测试
    /// </summary>
    [ApiBind("ClassCover")]
    public class ApiBindCoverService2
    {
        /// <summary>
        /// 从 class 继承 ApiBind
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public string TestApiWithoutAttr(string name = "Senparc", int value = 900)
        {
            return $"[from ApiBindCoverService2.TestApiWithoutAttr]{name}:{value}";
        }

        /// <summary>
        /// 忽略，不会出现在 API 列表中
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [IgnoreApiBind]
        public string TestApiWithoutAttr_Ignore(string name = "Senparc", int value = 900)
        {
            return $"[from ApiBindCoverService2.TestApiWithoutAttr_Ignore]{name}:{value}";
        }

        /// <summary>
        /// 忽略，不会出现在 API 列表中
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiBind(Ignore = true)]
        public string TestApiWithoutAttr_Ignore2(string name = "Senparc", int value = 900)
        {
            return $"[from ApiBindCoverService2.TestApiWithoutAttr_Ignore2]{name}:{value}";
        }

        /// <summary>
        /// 重写 ApiBind，使用 GET 方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiBind("Mine", "ApiBindCoverService2.TestApi", ApiRequestMethod = WebApi.ApiRequestMethod.Get)]
        public string TestApi(string name = "Senparc", int value = 910)
        {
            return $"[from ApiBindCoverService2.TestApi_Get]{name}:{value}";
        }

        /// <summary>
        /// 重写 ApiBind，自定义 Catetory 参数，使其和其他定义同名，将被自动改名，使用 GET 方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [ApiBind("Mine", "ApiBindCoverService2.TestApi", ApiRequestMethod = WebApi.ApiRequestMethod.Get)]
        public string TestApiSameName(string name = "Senparc", int value = 920)
        {
            return $"[from ApiBindCoverService2.TestApi_Get2]{name}:{value}";
        }


        /// <summary>
        /// 重写 ApiBind，name 设置后，可以融入和同类自动生成的 API，看上去无差别
        /// </summary>
        /// <returns></returns>
        [ApiBind("ClassCover", "ApiBindCoverService2.RewriteApiBind", ApiRequestMethod = WebApi.ApiRequestMethod.Post)]
        public string RewriteApiBind(string name = "Senparc", int value = 920)
        {
            return $"[from ApiBindCoverService2.RewriteApiBind]{name}:{value}";
        }

        /// <summary>
        /// 静态类，自动继承 class 配置
        /// </summary>
        /// <returns></returns>
        public static string StaticMethod() {
            return "ClassCover.StaticMethod";
        }
    }

    /// <summary>
    /// 用于测试自动生成的 WebApi 方法内调用静态方法
    /// </summary>
    public static class StaticApiBindTestService
    {
        //[ApiBind("CO2NETStatic", "StaticApiBindTest.TestApi")]
        /// <summary>
        /// 用于测试自动生成的 WebApi 方法内调用静态方法
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi(string name = "Senparc", int value = 999)
        {
            return $"[from StaticApiBindTestService.TestApi]{name}:{value}";
        }

        /// <summary>
        /// 用于测试自动生成的 WebApi 方法内调用静态方法 + 自定义 Attribute
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [MyTest("TestCopyAttrFromTestApiAsync in StaticApiBindTestService class")]
        public static async Task<string> TestApiAsync(string name = "Senparc", int value = 999)
        {
            var msg = $"[{SystemTime.Now:HH:mm:ss.ffff}] [from StaticApiBindTestService.TestApiAsync] Method";
            await Task.Delay(1000);
            msg += $"[{SystemTime.Now:HH:mm:ss.ffff}] {name}:{value}";
            return msg;
        }
    }

    /// <summary>
    /// 通过代码额外增加的类
    /// </summary>
    public class AdditionalType
    {
        /// <summary>
        /// 这个方法将通过“额外类”被注入
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi(string name = "Senparc", int value = 666)
        {
            return $"[from AdditionalType.TestApi]{name}:{value}";
        }

        /// <summary>
        /// 这个方法也将通过“额外类”被注入
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi2(string name = "Senparc", int value = 666)
        {
            return $"[from AdditionalType.TestApi2]{name}:{value}";
        }
    }


    /// <summary>
    /// 通过代码额外增加的方法
    /// </summary>
    public class AdditionalMethod
    {
        /// <summary>
        /// 这个方法将通过“额外方法”被注入
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi(string name = "Senparc", int value = 666)
        {
            return $"[from AdditionalMethod.TestApi]{name}:{value}";
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class MyTestAttribute : Attribute
    {
        public string Name { get; set; }
        public MyTestAttribute(string name)
        {
            Name = name;
            StackTrace st = new StackTrace(true);
            var fromName = st.GetFrame(1);
            //输出 TypeId，用于确认当前特性是非被复制到动态 API，并被调用
            //Console.WriteLine("-------------");
            Console.WriteLine($"MyTestAttribute [{Name}] TypeId Hash:{this.TypeId.GetHashCode()}  Caller ：{fromName?.GetMethod()?.Name}");
            //foreach (var item in st.GetFrames())
            //{
            //    Console.WriteLine($"\t{item.GetMethod()?.Name}");
            //}
            //Console.WriteLine("-------------");

        }
    }

}
