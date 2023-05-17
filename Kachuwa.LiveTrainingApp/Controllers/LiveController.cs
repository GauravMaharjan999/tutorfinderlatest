using Kachuwa.Dash.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kachuwa.KLiveApp.Controllers
{
    public class LiveController : Controller
    {
        private readonly IStreamingService streamingService;

        public LiveController(IStreamingService streamingService)
        {
            this.streamingService = streamingService;
        }

       
        [Route("live/page/{pageNo?}")]
        [Route("live")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
            ViewData["Page"] = pageNo;
            int rowsPerPage = 10;
            //customized viewmodel with join
            //var model = await streamingService.GetLatestLiveStreams(query, pageNo, rowsPerPage);
            //return View(model);

            var data = await streamingService.GetAllCoursesLiveStream();
            var eventdata = await streamingService.GetAllEventsLiveStream();
            ViewBag.EventData = eventdata;
            return View(data);
        }
        public IActionResult Detail()
        {
            return View();
        }

    }
}