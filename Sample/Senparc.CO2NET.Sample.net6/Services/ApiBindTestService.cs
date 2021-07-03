using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Senparc.CO2NET.Exceptions;

namespace Senparc.CO2NET.Sample.net6.Services
{
    public class ApiBindTestService
    {
        private readonly IServiceProvider _serviceProvider;

        public ApiBindTestService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }
        //[ApiBind("CO2NET", "ApiBindTest.TestApi")]
        public string TestApi(string name, int value)
        {
            return $"[from ApiBindTestService.TestApi]{name}:{value}";
        }

        /// <summary>
        /// 动态构建API代码
        /// </summary>
        public void DynamicBuild(IServiceCollection services)
        {
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


            //设置方法
            MethodBuilder setPropMthdBldr =
                      tb.DefineMethod("Tests", MethodAttributes.Public | MethodAttributes.Virtual,
                      typeof(string), //返回类型
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

            //执行具体方法
            var il = setPropMthdBldr.GetILGenerator();

            if (true)
            {
                var invokeClassType = typeof(EntityApiBindTestService);
                //Label lblEnd = il.DefineLabel();

                LocalBuilder local = il.DeclareLocal(typeof(string)); // create a local variable

                /* 最简洁方法
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

                //il.Emit(OpCodes.Ldarg, 1);
                //il.Emit(OpCodes.Stloc, 0);
                //il.Emit(OpCodes.Ldloc, 0);


                il.Emit(OpCodes.Ldfld, fbServiceProvider);

                il.Emit(OpCodes.Ldtoken, invokeClassType);
                il.Emit(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"));

                il.Emit(OpCodes.Callvirt, typeof(IServiceProvider).GetMethod("GetService"));
                il.Emit(OpCodes.Isinst, invokeClassType);
                il.Emit(OpCodes.Stloc, 0);
                il.Emit(OpCodes.Ldloc, 0);
                il.Emit(OpCodes.Ldarg, 1);
                il.Emit(OpCodes.Ldarg, 2);
                il.Emit(OpCodes.Callvirt, invokeClassType.GetMethod("TestApi"));
                il.Emit(OpCodes.Stloc, local);
                //il.Emit(OpCodes.Br_S, lblEnd);
                //il.MarkLabel(lblEnd);
                il.Emit(OpCodes.Ldloc, local);
                il.Emit(OpCodes.Ret);
            }
            else
            {
                //静态方法调用
                var methodInfo = typeof(StaticApiBindTestService).GetMethod("TestApi");
                LocalBuilder local = il.DeclareLocal(methodInfo.ReturnType); // create a local variable

                il.Emit(OpCodes.Nop); // the first one in arguments list
                il.Emit(OpCodes.Ldarg, 1); // the first one in arguments list
                il.Emit(OpCodes.Ldarg, 2);
                il.Emit(OpCodes.Call, methodInfo);
                il.Emit(OpCodes.Stloc, local); // set local variable
                il.Emit(OpCodes.Ldloc, local); // load local variable to stack 
                //il.Emit(OpCodes.Stloc, 1);
                //Label lbl = il.DefineLabel();
                //il.Emit(OpCodes.Br_S, lbl);
                //il.MarkLabel(lbl);
                //il.Emit(OpCodes.Ldloc, 1);
                il.Emit(OpCodes.Ret);
            }

            var t = tb.CreateType();
            TypeInfo objectTypeInfo = tb.CreateTypeInfo();
            var myType = objectTypeInfo.AsType();
            services.AddScoped(myType);

            Console.WriteLine($"\t create type:  {myType.Namespace} - {myType.FullName}");
            using (var scope = services.BuildServiceProvider().CreateScope())
            {
                var ctrl = scope.ServiceProvider.GetService(myType);
                Console.WriteLine(ctrl.GetType());
                var testMethod = ctrl.GetType().GetMethod("Tests");
                Console.WriteLine(testMethod.GetParameters().Count());
                var result = testMethod.Invoke(ctrl, new object?[] { "from dynamic 2", 1 });
                Console.WriteLine("result:" + result);
                Console.WriteLine("MethodName: " + string.Join('|', ctrl.GetType().GetMethod("Tests").GetCustomAttributes().Select(z => z.GetType().Name)));
            }
        }
    }

    public class EntityApiBindTestService
    {
        private readonly IServiceProvider _serviceProvider;

        public EntityApiBindTestService(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        //[ApiBind("CO2NETEntity", "EntityApiBindTest.TestApi")]
        public string TestApi(string name, int value)
        {
            var addMsg = "";

            var testService = _serviceProvider.GetService(typeof(ApiBindTestService)) as ApiBindTestService;
            addMsg = testService.TestApi(name, value);

            return $"[from EntityApiBindTestService.TestApi]{name}:{value} - {addMsg}";
        }
    }

    public static class StaticApiBindTestService
    {
        //[ApiBind("CO2NETStatic", "StaticApiBindTest.TestApi")]
        public static string TestApi(string name, int value)
        {
            return $"[from StaticApiBindTestService.TestApi]{name}:{value}";
        }
    }
}
