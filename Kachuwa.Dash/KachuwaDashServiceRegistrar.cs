using System.Reflection;
using Kachuwa.Core.DI;
using Kachuwa.Dash.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.FileProviders;

namespace Kachuwa.Dash
{
    public class KachuwaDashServiceRegistrar : IServiceRegistrar
    {
        private bool _isInstalled = false;
        public void Register(IServiceCollection services, IConfiguration configuration)
        {
            if (configuration != null)
            {
                var str_isInstalled = configuration["KachuwaAppConfig:IsInstalled"].ToLower();
                _isInstalled = str_isInstalled != "false";

            }
         

            services.TryAddSingleton<IEncodingService, EncodingService>();
            services.TryAddSingleton<IServerService, ServerService>();
            services.TryAddSingleton<IStreamingService, StreamingService>();
            var embeddedAssembly = new EmbeddedFileProvider(typeof(KachuwaDashServiceRegistrar).GetTypeInfo().Assembly);
            services.Configure<RazorViewEngineOptions>(opts =>
            {
                opts.FileProviders.Add(embeddedAssembly);
            });



        }

        public void Update(IServiceCollection serviceCollection)
        {
            if (_isInstalled)
            {

                // var builder = serviceCollection.BuildServiceProvider();
                //  var settingService = builder.GetService<ISettingService>();
                // serviceCollection.AddSingleton(settingService.CrudService.Get(1));
            }

        }
    }
}