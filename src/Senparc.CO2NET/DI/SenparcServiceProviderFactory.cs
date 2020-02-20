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
            var builder = new ContainerBuilder();
            builder.Populate(services);
            return builder;
            return services.ToServiceContainer();
        }

        public IServiceProvider CreateServiceProvider(IServiceContainer containerBuilder)
        {
            return containerBuilder;
        }
    }
}
#endif