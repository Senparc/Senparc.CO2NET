#if !NET45
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel.Design;

namespace Senparc.CO2NET
{
    public class SenparcServiceProviderFactory : IServiceProviderFactory<IServiceContainer>
    {
        public IServiceContainer CreateBuilder(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            return new ServiceContainer(serviceProvider);
        }

        public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        {
            //SenparcDI.GlobalServiceProvider = containerBuilder;
            Console.WriteLine(containerBuilder.GetHashCode());
            return containerBuilder;
        }
    }
}
#endif