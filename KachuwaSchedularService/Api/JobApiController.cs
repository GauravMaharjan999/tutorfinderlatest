using System.Threading.Tasks;
using Kachuwa.Web;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Mvc;

namespace Api.V1.KachawaSchedular
{
    public class HomeController : BaseController
    {
        public async Task<string> Index()
        {
            return "Hellow";
        }
    }

    [Route("api/v1/job")]
    public class JobApiController : BaseApiController
    {

        public JobApiController()
        {

        }

        [Route("start")]
        public async Task<dynamic> StartAsync(string jobId)
        {
            return HttpResponse(200, "");
        }
        [Route("restart")]
        public async Task<dynamic> ReStartAsync(string jobId)
        {
            return HttpResponse(200, "");
        }
        [Route("stop")]
        public async Task<dynamic> StopAsync(string jobId)
        {
            return HttpResponse(200, "");
        }
        [Route("delete")]
        public async Task<dynamic> DeleteAsync(string jobId)
        {
            return HttpResponse(200, "");
        }
    }
}