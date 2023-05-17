using System.Reflection;
using Kachuwa.Banner;
using Kachuwa.Core.DI;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Kachuwa.Banner
{
    public class KachuwaBannerServiceRegistrar : IServiceRegistrar
    {
        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            serviceCollection.AddTransient<IBannerService, BannerService>();
            var assp = new EmbeddedFileProvider(typeof(BannerModule).GetTypeInfo().Assembly);
            serviceCollection.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(assp);
            });
        }

        public void Update(IServiceCollection serviceCollection)
        {
           
        }
    }
}