using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Api
{
    [Route("api/v1/coursesyllabus")]
    public class CourseSyllabusApiController : BaseApiController
    {
        private readonly ICourseSyllabusService _courseSyllabusService;
        private ILogger _logger;

        public CourseSyllabusApiController(ICourseSyllabusService courseSyllabusService,ILogger logger)
        {
            _courseSyllabusService = courseSyllabusService;
            _logger = logger;
        }

        [Route("gridList")]
        public async Task<object> CourseSyllabusList([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {

            try
            {
                var data = await _courseSyllabusService.CourseSyllabuslist(pq_curpage, pq_rpp);
                return HttpResponse(200, "Success", data, pq_curpage);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }
    }
}
