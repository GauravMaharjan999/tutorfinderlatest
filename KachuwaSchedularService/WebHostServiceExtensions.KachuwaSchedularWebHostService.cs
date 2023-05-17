using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.WindowsServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public static partial class WebHostServiceExtensions
{
    internal class KachuwaSchedularWebHostService : WebHostService
    {
        private ILogger _logger;

        public KachuwaSchedularWebHostService(IWebHost host) : base(host)
        {
            _logger = host.Services
                .GetRequiredService<ILogger<KachuwaSchedularWebHostService>>();
        }

        protected override void OnStarting(string[] args)
        {
            _logger.LogInformation("OnStarting method called.");
            base.OnStarting(args);
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted method called.");
            base.OnStarted();
        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping method called.");
            base.OnStopping();
        }
    }
}