using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Host.Logging;
using IdentityModel;
using Kachuwa.Core.Extensions;
using Kachuwa.Data;
using Kachuwa.Data.Crud;
using Kachuwa.Identity.IdSrv;
using Kachuwa.Identity.Models;
using Kachuwa.Identity.Service;
using Kachuwa.Log.Insight;
using Kachuwa.Web;
using Kachuwa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Kachuwa.KLiveApp
{
    public class Startup
    {
        private IHostingEnvironment hostingEnvironment;
        public Startup(IHostingEnvironment env, IConfiguration configuration)
        {
            hostingEnvironment = env;
            Configuration = configuration;

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Configuration["KachuwaAppConfig:DataProtectionKeyPath"]))
            //    .SetApplicationName(Configuration["KachuwaAppConfig:AppName"]);
            services.AddSingleton(Configuration);
            var serviceProvider = services.BuildServiceProvider();
            IDatabaseFactory dbFactory = DatabaseFactories.SetFactory(Dialect.SQLServer, serviceProvider);
            services.AddSingleton(dbFactory);

            services.ConfigureKachuwa(setup =>
            {


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

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseSuccessEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.IssuerUri = hostingEnvironment.IsDevelopment() ? "http://localhost:3798" : "https://learnersnepal.com";

                options.MutualTls.Enabled = true;
                options.MutualTls.ClientCertificateAuthenticationScheme = "x509";
                options.Cors.CorsPolicyName = "corsGlobalPolicy";
            })
               //.AddInMemoryClients(Clients.Get())
               //.AddInMemoryClients(_config.GetSection("Clients"))
               // .AddInMemoryIdentityResources(Resources.GetIdentityResources())
               //.AddInMemoryApiResources(Resources.GetApiResources())
               .AddDeveloperSigningCredential()
               // .AddExtensionGrantValidator<ExtensionGrantValidator>()
               // .AddExtensionGrantValidator<NoSubjectExtensionGrantValidator>()
               //.AddJwtBearerClientAuthentication()
               .AddAppAuthRedirectUriValidator()
               //.AddTestUsers(TestUsers.Users)
               .AddMutualTlsSecretValidators()
               .AddInMemoryIdentityResources(Kachuwa.Identity.IdentityConfig.Resources.GetIdentityResources())
               .AddInMemoryApiResources(Kachuwa.Identity.IdentityConfig.Resources.GetApiResources())
               .AddInMemoryClients(Kachuwa.Identity.IdentityConfig.Clients.Get())

               .AddAspNetIdentity<IdentityUser>()
               .AddProfileService<CustomProfileService>()
               .AddResourceOwnerValidator<CustomResourceOwnerPasswordValidator>();


            services.AddAuthentication()
                .AddCookie("Cookies");
            //services.AddExternalIdentityProviders(); 
            //.AddJwtBearer(opts =>
            //{
            //    opts.Authority = Configuration["KachuwaAppConfig:TokenAuthority"];
            //    opts.Audience = "OnlineKachhyaApi";
            //    opts.RequireHttpsMetadata = false;
            //    opts.IncludeErrorDetails = true;
            //});


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

            services.AddLocalization();
            // File Sile
            services.Configure<FormOptions>(options =>
            {
                options.MultipartBodyLengthLimit = Int64.MaxValue;
                options.MultipartBoundaryLengthLimit = Int32.MaxValue;
                options.ValueLengthLimit = Int32.MaxValue;
                options.MultipartHeadersLengthLimit = Int32.MaxValue;
                options.ValueCountLimit = Int32.MaxValue;
            });

            //enable directory browsing
            //services.AddDirectoryBrowser();
            services.AddSignalR().AddJsonProtocol(options =>
            {
                options.PayloadSerializerSettings.ContractResolver = new DefaultContractResolver();
                options.PayloadSerializerSettings.Formatting = Formatting.Indented;
            });
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


            provider.Mappings[".mpd"] = "application/dash+xml";
            provider.Mappings[".m4a"] = "audio/mp4";
            provider.Mappings[".aac"] = "audio/aac";
            provider.Mappings[".m4a"] = "audio/mp4";
            provider.Mappings[".m3u"] = "audio/x-mpegurl";
            provider.Mappings[".m4s"] = "video/iso.segment";
            provider.Mappings[".m3u8"] = "vnd.apple.mpegURL";
            provider.Mappings[".ts"] = "video/MP2T";
            provider.Mappings[".vtt"] = "text/vtt";
            provider.Mappings[".srt"] = "text/srt";

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
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(hostingEnvironment.ContentRootPath, @"wwwroot"))
                ,
                ContentTypeProvider = provider
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(hostingEnvironment.ContentRootPath, @"wwwroot", "lib")),
                RequestPath = new PathString("/lib")
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(hostingEnvironment.ContentRootPath, @"Themes")),
                RequestPath = new PathString("/themes")
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(hostingEnvironment.ContentRootPath, @"Locale")),
                RequestPath = new PathString("/locale")
            });
            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(hostingEnvironment.ContentRootPath, @"wwwroot", "test")),
                RequestPath = new PathString("/test")
            });
            app.UseAuthentication();
            app.UseStaticHttpContext();

            app.UseWebSockets();
            var supportedCultures = new[]
            {
                new CultureInfo("en-US"),
                new CultureInfo("hi-IN"),
                new CultureInfo("en-GB"),
                //new CultureInfo("en"),
                new CultureInfo("es-ES"),
                new CultureInfo("zh-CN"),
                new CultureInfo("es"),
                new CultureInfo("fr-FR"),
                new CultureInfo("fr"),
                new CultureInfo("ne-NP"),
            };

            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture("en-US"),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures

            });
            app.UseKachuwaApps(serviceProvider, hostingEnvironment);
            //core
            app.UseKachuwaCore(serviceProvider);
            //web
            app.UseKachuwaWeb(false);

            app.UseSignalR(routes =>  // <-- SignalR
            {

                // routes.MapHub<KachuwaUserHub>("/hubs/user");
                //routes.MapHub<DashboardHub>("/hubs/dashboard");

            });
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");


                routes.MapRoute(name: "areaRoute",
                    template: "{area:exists}/{controller}/{action}/{id?}",
                    defaults: new { area = "Admin", controller = "Dashboard", action = "Index" });

            });

        }
    }
}
