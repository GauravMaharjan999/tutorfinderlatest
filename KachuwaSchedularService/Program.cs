using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Kachuwa.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace KachuwaSchedularService
{
    //public class Program
    //{
    //    public static void Main(string[] args)
    //    {
    //        CreateWebHostBuilder(args).Build().Run();
    //    }

    //    public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
    //        WebHost.CreateDefaultBuilder(args)
    //            .UseStartup<Startup>();
    //}
    public class Program
    {
        public static void Main(string[] args)
        {





#if DEBUG
            var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());

            var host = builder.Build();
            host.Run();
#else
            var isService = !(Debugger.IsAttached || args.Contains("--console"));

           
            if (isService)
            {
                // To run the app without the CustomWebHostService change the
                // next line to host.RunAsService();
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                var pathToContentRoot = Path.GetDirectoryName(pathToExe);
                Directory.SetCurrentDirectory(pathToContentRoot);
     var builder = CreateWebHostBuilder(
                args.Where(arg => arg != "--console").ToArray());
                var host = builder.Build();
                host.RunAsCustomService();
            }
            else
            {
                var builder = CreateWebHostBuilder(
  args.Where(arg => arg != "--console").ToArray());

                var host = builder.Build();
                host.Run();

            }
#endif

        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestBodySize = 52428800; //50MB
                })
                .ConfigureAppConfiguration((builderContext, config) =>
                {
                    IHostingEnvironment env = builderContext.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
                    config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);
                    config.AddJsonFile("config/kachuwaconfig.json", optional: false, reloadOnChange: true);


                }).ConfigureServices(e => { e.TryAddSingleton<ConfigChangeEvent, KachuwaConfigChangeEvent>(); })
                .UseUrls("http://*:9091")//valid in production ie windows service
                .UseStartup<Startup>();
    }
}