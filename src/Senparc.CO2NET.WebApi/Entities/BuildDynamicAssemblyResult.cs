using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class BuildDynamicAssemblyResult
    {
        public BuildDynamicAssemblyResult(AssemblyBuilder assemblyBuilder, ModuleBuilder mb, TypeBuilder tb, FieldBuilder fbServiceProvider, string controllerKeyName)
        {
            AssemblyBuilder = assemblyBuilder;
            Mb = mb;
            Tb = tb;
            FbServiceProvider = fbServiceProvider;
            ControllerKeyName = controllerKeyName;
        }

        public AssemblyBuilder AssemblyBuilder { get; }
        public ModuleBuilder Mb { get; }
        public TypeBuilder Tb { get; }
        public FieldBuilder FbServiceProvider { get; set; }
        public string ControllerKeyName { get; }
    }
}
