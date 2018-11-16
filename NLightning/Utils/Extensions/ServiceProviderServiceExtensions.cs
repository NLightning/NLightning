using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Utils.Extensions
{
    public static class ServiceProviderServiceExtensions
    {
        public static T GetService<I, T>(this IServiceProvider provider) where T : I
        {
            var services = provider.GetServices<I>();

            return (T)services.First(s => s is T);
        }
    }
}