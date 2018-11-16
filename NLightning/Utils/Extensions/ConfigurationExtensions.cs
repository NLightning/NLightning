using System;
using Microsoft.Extensions.Configuration;

namespace NLightning.Utils.Extensions
{
    public static class ConfigurationExtensions
    {
        public static TSection GetConfiguration<TSection>(this IConfiguration configuration)
        {
            IConfigurationSection section = configuration.GetSection(typeof(TSection).Name);
            TSection instance = section.Get<TSection>();
            if (instance == null)
            {
                instance = Activator.CreateInstance<TSection>();
            }
            
            return instance;
        }
    }
}