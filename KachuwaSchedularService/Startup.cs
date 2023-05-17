using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Core.Extensions;
using Kachuwa.Data;
using Kachuwa.Data.Crud;
using Kachuwa.Identity;
using Kachuwa.Job;
using Kachuwa.Log.Insight;
using Kachuwa.Web;
using Kachuwa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace KachuwaSchedularService
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;
        public IConfiguration Configuration { get; }
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            hostingEnvironment = env;
            Configuration = configuration;

        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Configuration["KachuwaAppConfig:DataProtectionKeyPath"]))
                .SetApplicationName(Configuration["KachuwaAppConfig:AppName"]);
            services.AddSingleton(Configuration);
            var serviceProvider = services.BuildServiceProvider();
            //TODO can be used in Action Config in KachuwaSetup
            //registering default database factory service
            IDatabaseFactory dbFactory = DatabaseFactories.SetFactory(Dialect.SQLServer, serviceProvider);
            services.AddSingleton(dbFactory);
            services.ConfigureKachuwa(setup =>
            {
                //setup.UseDefaultMemory(services)
                //    .UseKachuwaLocalization(services, config =>
                //    {
                //        config.UseDbResources = true;
                //        config.UseJsonResources = true;
                //    });

            });
            

            services.AddAuthorization(options =>
            {
                //options.AddPolicy("Test", policy => policy.Requirements.Add(new HasScopeRequirement("read:messages", "xyz.com")));
                options.AddPolicy(PolicyConstants.PagePermission, policy => policy.Requirements.Add(new PagePermissionRequirement()));
            });

            services.RegisterKachuwaCoreServices(serviceProvider);
            services.AddSingleton<IAuthorizationPolicyProvider, AuthorizationPolicyProvider>();
            services.AddSingleton<IAuthorizationHandler, PagePermissionHandler>();
            // Register the Swagger generator, defining one or more Swagger documents

            services.AddAntiforgery(options =>
            {

                //options.FormFieldName = "AF_Tix";
                //options.HeaderName = "X-CSRF-TOKEN-Tixalaya";
                //options.SuppressXFrameOptionsHeader = false;
                //options.Cookie.Domain = "tixalaya.com";
                //options.Cookie.Name = "X-CSRF-TOKEN-Tixalaya";
                //options.Cookie.Path = "Path";
                //options.Cookie.Expiration = TimeSpan.FromMinutes(10);
                //options.Cookie.SecurePolicy = CookieSecurePolicy.None;
            });
           
            services.Configure<GzipCompressionProviderOptions>
                (options => options.Level = CompressionLevel.Fastest);
            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });
            // Add framework services.
            services.AddMvc(options =>
            {
                options.Filters.Add(new AuditAttribute());

            }).AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                options.SerializerSettings.Formatting = Formatting.Indented;
            }).AddViewComponentsAsServices();
            services.AddKachuwaIdentitySever(hostingEnvironment, Configuration);


            //dual authorization support
            services.AddAuthentication(o =>
            {// setting default authorization scheme ,authorize attribute apply default schem
             //o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;


            })
                .AddJwtBearer(opts =>
                {
                    opts.Authority = Configuration["KachuwaAppConfig:SiteUrl"].ToString();
                    opts.Audience = Configuration["KachuwaAppConfig:AppName"].ToString();
                    opts.RequireHttpsMetadata = false;
                    opts.IncludeErrorDetails = true;
                });
           
            services.AddSession(options =>
            {
                options.Cookie.Name = ".Kachuwa.Session";
                options.IdleTimeout = TimeSpan.FromMinutes(10);
            });

            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];
                options.SchemaName = "dbo";
                options.TableName = "Sessions";
            });

            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Int64.MaxValue;
                options.MultipartBoundaryLengthLimit = Int32.MaxValue;
                options.ValueLengthLimit = Int32.MaxValue;
                options.MultipartHeadersLengthLimit = Int32.MaxValue;
                options.ValueCountLimit = Int32.MaxValue;
            });
          
            services.AddSignalR().AddJsonProtocol(options => {
                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                options.PayloadSerializerSettings.Formatting = Formatting.Indented;
            }); ;
           
            services.RegisterSchedulingServer(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider,
            IHostingEnvironment env, ILoggerFactory loggerFactory,
            IOptions<ApplicationInsightsSettings> applicationInsightsSettings)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseSession();

            app.UseResponseCompression();
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".log"] = "text/plain";
            provider.Mappings[".txt"] = "text/plain";

            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(hostingEnvironment.ContentRootPath, @"Logs")),
                RequestPath = new PathString("/dev/logs"),
                EnableDirectoryBrowsing = true,
                StaticFileOptions =
                {
                    DefaultContentType = "text/plain",
                    ContentTypeProvider = provider

                }

            });

            app.UseAuthentication();
            app.UseIdentityServer();
            app.UseStaticHttpContext();
            app.UseStaticFiles();
            app.UseWebSockets();
         
            app.UseKachuwaApps(serviceProvider, hostingEnvironment);
            //core
            app.UseKachuwaCore(serviceProvider);
            //web
           // app.UseKachuwaWeb(false);

            app.UseSignalR(routes =>  // <-- SignalR
            {

                // routes.MapHub<KachuwaUserHub>("/hubs/user");
                //routes.MapHub<DashboardHub>("/hubs/dashboard");

            });
            app.UseMvc(routes =>
            {
                //routes.MapRoute(
                //    name: "default",
                //    template: "{pageUrl?}",
                //    defaults: new { controller = "KachuwPage", action = "Index" }
                //    // , constraints: new { pageUrl = @"\w+" }
                //);
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");


                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}/{id?}",
                    defaults: new { area = "Admin", controller = "Dashboard", action = "Index" });

            });
            app.UseSchedulingServer();

        }
    }
}
