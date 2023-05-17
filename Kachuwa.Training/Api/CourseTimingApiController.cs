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
    [Route("api/v1/coursetiming")]
    public class CourseTimingApiController : BaseApiController
    {
        private readonly ICourseTimingService _courseTimingService;
        private ILogger _logger;

        public CourseTimingApiController(ICourseTimingService courseTimingService,ILogger logger)
        {
            _courseTimingService = courseTimingService;
            _logger = logger;
        }

        [Route("gridList")]
        public async Task<object> CourseTimingList([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {

            try
            {
                var data = await _courseTimingService.CourseTimingList(pq_curpage, pq_rpp);
                return HttpResponse(200, "Success", data, pq_curpage);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }

        [Route("getCourseTimingbycourseTimingId/{courseTimingId}")]
        public async Task<object> getCourseTimingbycourseTimingId([FromRoute]int courseTimingId)
        {

            try
            {
                var data = await _courseTimingService.CourseTimingCrudService.GetAsync(courseTimingId);
                return HttpResponse(200, "Success", data);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }

        [Route("getCourseTimingListForStreamByTutorIdentityUserId/{identityuserId}")]
        public async Task<object> GetCourseTimingListForStreamByTutorId([FromRoute]int identityuserId)
        {

            try
            {
                var data = await _courseTimingService.CourseTimingListforStreamByTutorIdentityUserId(identityuserId);
                return HttpResponse(200, "Success", data);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }
    }
}
