using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Kachuwa.KLiveApp.Controllers
{
    public class CourseController : Controller
    {
        private readonly ICourseService _courseService;

        public CourseController(ICourseService courseService)
        {
            _courseService = courseService;
        }


        [Route("courses/page/{pageNo?}")]
        [Route("courses")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
           
            return View();
        }
        [Route("course/{id}")]
        public async Task<IActionResult> Detail([FromRoute]int id)
        {
             var data = await _courseService.GetCourseandTutorDetailsByCourseId(id);
            return View(data);
        }

    }
}