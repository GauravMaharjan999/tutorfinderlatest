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
    [Route("api/v1/event")]
    public  class EventApiController : BaseApiController
    {
        private readonly IEventService _eventService;
        private ILogger _logger;

        public EventApiController(IEventService eventService,ILogger logger)
        {
            _eventService = eventService;
            _logger = logger;
        }

        [Route("gridList")]
        public async Task<object> EventList([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {
            try
            {
                var data = await _eventService.EventCrudService.GetListPagedAsync(pq_curpage, pq_rpp, 1, "Where IsDeleted=@isDel", "Id asc", new { @isDel = false });
                return HttpResponse(200, "Success", data, pq_curpage);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }

        [Route("getEventListforStreaming/{identityuserId}")]
        public async Task<object> GetCourseTimingListForStreamByTutorId([FromRoute]int identityuserId)
        {

            try
            {
                var data = await _eventService.EventCrudService.GetListAsync("Where AddedBy=@addedby and (@date between FromDate and ToDate)", new { @date = DateTime.Now.Date ,@addedby = identityuserId });
                //var data = await _courseTimingService.CourseTimingListforStreamByTutorIdentityUserId(identityuserId);
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
