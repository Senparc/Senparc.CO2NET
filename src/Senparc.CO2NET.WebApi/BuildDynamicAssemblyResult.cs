using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Text;

namespace Senparc.CO2NET.WebApi
{
    public class BuildDynamicAssemblyResult
    {
        public BuildDynamicAssemblyResult(AssemblyBuilder assemblyBuilder, ModuleBuilder mb, TypeBuilder tb, string controllerKeyName)
        {
            AssemblyBuilder = assemblyBuilder;
            Mb = mb;
            Tb = tb;
            ControllerKeyName = controllerKeyName;
        }

        public AssemblyBuilder AssemblyBuilder { get; }
        public ModuleBuilder Mb { get; }
        public TypeBuilder Tb { get; }
        public string ControllerKeyName { get; }
    }
}
