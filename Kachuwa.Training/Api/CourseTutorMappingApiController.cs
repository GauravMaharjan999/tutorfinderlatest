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
    [Route("api/v1/coursetutormapping")]
    public class CourseTutorMappingApiController : BaseApiController
    {
        private readonly ICourseTutorMappingService _courseTutorMappingService;
        private ILogger _logger;

        public CourseTutorMappingApiController(ICourseTutorMappingService courseTutorMappingService,ILogger logger)
        {
            _courseTutorMappingService = courseTutorMappingService;
            _logger = logger;
        }

        [Route("gridList")]
        public async Task<object> CourseTutorMappingList([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {

            try
            {
                var data = await _courseTutorMappingService.CourseTutorMappingAllList(pq_curpage, pq_rpp);
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
