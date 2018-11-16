using Microsoft.Extensions.DependencyInjection;

namespace NLightning.Utils.Extensions
{
    public static class IServiceScopeFactoryExtensions
    {
        public static T CreateScopedService<T>(this IServiceScopeFactory factory)
        {
            return factory.CreateScope().ServiceProvider.GetService<T>();
        }
    }
}