using System.Reflection;
using Kachuwa.Core.DI;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Kachuwa.FileUploader
{
    public class KachuwaFileUploaderModuleServiceRegistrar : IServiceRegistrar
    {
        public void Update(IServiceCollection serviceCollection)
        {

        }


        public void Register(IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var assp = new EmbeddedFileProvider(typeof(KachuwaFileUploaderModuleServiceRegistrar).GetTypeInfo().Assembly);
            serviceCollection.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(assp);
            });
        }
    }
}