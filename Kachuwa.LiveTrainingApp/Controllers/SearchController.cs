using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.KLiveApp.Controllers
{
    public class SearchController : Controller
    {
        public IActionResult Index([FromQuery]string query="")
        {
            return View();
        }
      
    }
}