using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Senparc.CO2NET.Sample.net8.Services
{
    /// <summary>
    /// Service for testing dynamically generated APIs
    /// </summary>
    public class ApiBindTestService
    {
        public ApiBindTestService()
        {
        }

        /// <summary>
        /// Test converting method to API
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
        /// Test converting async method to API
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
        /// Simulates adding WeChat_OfficialAccount API
        /// </summary>
        /// <param name="name">Enter name here</param>
        /// <param name="value">Enter value here</param>
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
        /// Dynamically build API code; partial core code test
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

            #region Build assembly


            AssemblyName dynamicApiAssembly = new AssemblyName("DynamicTests");
            //AppDomain currentDomain = Thread.GetDomain();
            AssemblyBuilder dynamicAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(dynamicApiAssembly, AssemblyBuilderAccess.RunAndCollect);

            // Dynamically create module
            ModuleBuilder mb = dynamicAssemblyBuilder.DefineDynamicModule(dynamicApiAssembly.Name);

            // Dynamically create XXController class
            var controllerClassName = $"DynamicTestController";
            TypeBuilder tb = mb.DefineType(controllerClassName, TypeAttributes.Public, typeof(ControllerBase) /*typeof(Controller)*/);

            //var t1 = typeof(ApiController);
            //tb.SetCustomAttribute(new CustomAttributeBuilder(t1.GetConstructor(new Type[] { typeof(string) }), new object[]  }));

            var t2 = typeof(RouteAttribute);
            tb.SetCustomAttribute(new CustomAttributeBuilder(t2.GetConstructor(new Type[] { typeof(string) }), new object[] { $"myapi/[controller]" }));

            // Private field
            var fbServiceProvider = tb.DefineField("_serviceProvider", typeof(IServiceProvider), FieldAttributes.Private | FieldAttributes.InitOnly);

            #endregion

            #region Configure constructor

            // Configure constructor
            var ctorBuilder = tb.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new[] { typeof(IServiceProvider) });
            var ctorIl = ctorBuilder.GetILGenerator();
            ctorIl.Emit(OpCodes.Ldarg, 0);
            //Define the reflection ConstructorInfor for System.Object
            ConstructorInfo conObj = typeof(object).GetConstructor(new Type[0]);
            ctorIl.Emit(OpCodes.Call, conObj);// Call base default ctor
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Nop);
            ctorIl.Emit(OpCodes.Ldarg, 0);
            ctorIl.Emit(OpCodes.Ldarg, 1);
            ctorIl.Emit(OpCodes.Stfld, fbServiceProvider);
            ctorIl.Emit(OpCodes.Ret);

            #endregion

            #region Configure method

            // Configure method
            MethodBuilder setPropMthdBldr =
                      tb.DefineMethod("Tests", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.HideBySig,
                      invokeMethodInfo.ReturnType, // Return type
                      new[] { typeof(string), typeof(int) }// Input parameters
                      );

            // Add tags
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



            // Copy attributes
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

            #region Configure method body

            // Invoke target method
            var il = setPropMthdBldr.GetILGenerator();
            LocalBuilder local = il.DeclareLocal(invokeMethodInfo.ReturnType); // create a local variable

            if (invokeClassType == typeof(EntityApiBindTestService) || !invokeMethodInfo.IsStatic)
            {
                //Label lblEnd = il.DefineLabel();

                /* Simplest approach (standalone)
                il.Emit(OpCodes.Nop);
                //il.Emit(OpCodes.Ldarg, 0);
                il.Emit(OpCodes.Ldarg, 1);
                il.Emit(OpCodes.Stloc, local);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ret);
                */

                // Instance method
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
                // Static method call

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
    /// For testing non-static method calls in auto-generated WebApi methods with IServiceProvider and DI auto-injection
    /// </summary>
    public class EntityApiBindTestService
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityApiBindTestService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        /// <summary>
        /// For testing non-static method calls (sync) in auto-generated WebApi methods with IServiceProvider and DI auto-injection.
        /// <para>Also tests custom Attribute</para>
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
        /// For testing non-static method calls (async) in auto-generated WebApi methods with IServiceProvider and DI auto-injection.
        /// <para>Also tests custom Attribute</para>
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
    /// Test ApiBind binding at class level
    /// </summary>
    [ApiBind("ClassCover")]
    public class ApiBindCoverService2
    {
        /// <summary>
        /// Inherit ApiBind from class
        /// </summary>
        /// <param name="name">Default: Senparc</param>
        /// <param name="value">Default: 900</param>
        /// <returns></returns>
        public string TestApiWithoutAttr(string name = "Senparc", int value = 900)
        {
            return $"[from ApiBindCoverService2.TestApiWithoutAttr]{name}:{value}";
        }

        /// <summary>
        /// Ignored; will not appear in API list
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
        /// Ignored; will not appear in API list
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
        /// Override ApiBind using GET method
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
        /// Override ApiBind with custom Category parameter so it shares a name with other definitions and gets auto-renamed; uses GET method
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
        /// Override ApiBind; after name is set it blends with auto-generated APIs of the same class and appears identical
        /// </summary>
        /// <returns></returns>
        [ApiBind("ClassCover", "ApiBindCoverService2.RewriteApiBind", ApiRequestMethod = WebApi.ApiRequestMethod.Post)]
        public string RewriteApiBind(string name = "Senparc", int value = 920)
        {
            return $"[from ApiBindCoverService2.RewriteApiBind]{name}:{value}";
        }

        /// <summary>
        /// Static method; automatically inherits class configuration
        /// </summary>
        /// <returns></returns>
        public static string StaticMethod()
        {
            return "ClassCover.StaticMethod";
        }
    }

    /// <summary>
    /// For testing static method calls in auto-generated WebApi methods
    /// </summary>
    public static class StaticApiBindTestService
    {
        //[ApiBind("CO2NETStatic", "StaticApiBindTest.TestApi")]
        /// <summary>
        /// For testing static method calls in auto-generated WebApi methods
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi(string name = "Senparc", int value = 999)
        {
            return $"[from StaticApiBindTestService.TestApi]{name}:{value}";
        }

        /// <summary>
        /// For testing static method calls in auto-generated WebApi methods with custom Attribute
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
    /// Class added extra via code
    /// </summary>
    public class AdditionalType
    {
        /// <summary>
        /// This method will be injected via "additional class"
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string TestApi(string name = "Senparc", int value = 666)
        {
            return $"[from AdditionalType.TestApi]{name}:{value}";
        }

        /// <summary>
        /// This method will also be injected via "additional class"
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
    /// Methods added extra via code
    /// </summary>
    public class AdditionalMethod
    {
        /// <summary>
        /// This method will be injected via "additional method"
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
            // Output TypeId to verify the attribute is copied to the dynamic API and invoked
            //Console.WriteLine("-------------");
            Console.WriteLine($"MyTestAttribute [{Name}] TypeId Hash:{this.TypeId.GetHashCode()}  Caller ：{fromName?.GetMethod()?.Name}");
            //foreach (var item in st.GetFrames())
            //{
            //    Console.WriteLine($"\t{item.GetMethod()?.Name}");
            //}
            //Console.WriteLine("-------------");

        }
    }


    /// <summary>
    /// Parameters with Attribute
    /// </summary>
    [ApiController]
    public class ParameterAttribute
    {
        /// <summary>
        /// Parameter with Attribute test
        /// </summary>
        /// <param name="requestData"></param>
        /// <returns></returns>
        [ApiBind(ApiRequestMethod = WebApi.ApiRequestMethod.Post)]
        public static string ParameterAttributeTest([FromBody]RequestData requestData1)
        {
            /* 
             * Test with PostMan or similar tools:
             * curl --location --request POST 'https://localhost:44351/api/Senparc.CO2NET.Sample/ParameterAttribute/CO2NET.Sample_ParameterAttribute.ParameterAttributeTest' \
               --header 'Content-Type: application/json' \
               --data-raw '{
                   "UserName": "SenparcCoreAdmin96",
                   "Password": "743e815e2"
                }'
             *
             *  Result: SenparcCoreAdmin96:743e815e2 -- 2021/11/22 21:11:14 +08:00
             */

            return $"{requestData1.UserName}:{requestData1.Password} -- {SystemTime.Now}";
        }


    }
    public class RequestData
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    [ApiController]
    [Route("[controller]")]
    public class NormalApi : ControllerBase
    {

        [HttpPost("login")]
        public IActionResult Login([FromBody]RequestData request)
        {
            return Content($"{request.UserName}:{request.Password} -- {SystemTime.Now}");
        }

        [HttpGet("login2")]
        public IActionResult Login2(RequestData request)
        {
            return Content($"{request.UserName}:{request.Password}");
        }
    }

}
