using Kachuwa.Training.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Controllers
{
    public class TutorController : Controller
    {
        private readonly ITrainingTutorService _trainingTutorService;
        public TutorController(ITrainingTutorService trainingTutorService)
        {
            _trainingTutorService = trainingTutorService;
        }
        [Route("tutors/page/{pageNo?}")]
        [Route("tutors")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {

            return View();
        }
        [Route("tutor/{id}")]
        public async Task<IActionResult> Detail([FromRoute]int id)
        {
            var data = await _trainingTutorService.TrainingTutorCrudService.GetAsync(id);
            return View(data);
        }

    }
}
