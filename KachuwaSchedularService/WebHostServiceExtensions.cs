using System.ServiceProcess;
using Microsoft.AspNetCore.Hosting;

public static partial class WebHostServiceExtensions
{
    public static void RunAsCustomService(this IWebHost host)
    {
        var webHostService = new KachuwaSchedularWebHostService(host);
        ServiceBase.Run(webHostService);
    }
}
