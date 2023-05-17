using Kachuwa.Dash.Services;
using Kachuwa.Identity.Extensions;
using Kachuwa.Training.Service;
using Kachuwa.Web.Notification;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using System.Linq;
using Kachuwa.Dash.Live;
using Kachuwa.Data.Extension;

namespace Kachuwa.KLiveApp.Controllers
{
    public class WatchController : Controller
    {
        private readonly IStreamingService _streamingService;
        private readonly IEnrollService _enrollService;
        private readonly ICourseTimingStreamMapping _courseTimingStreamMapping;
        private readonly INotificationService _notificationService;
        private readonly IEventService _eventService;

        public WatchController(IStreamingService streamingService,IEnrollService enrollService , ICourseTimingStreamMapping courseTimingStreamMapping,INotificationService notificationService,IEventService eventService)
        {
            _streamingService = streamingService;
            _enrollService = enrollService;
            _courseTimingStreamMapping = courseTimingStreamMapping;
            _notificationService = notificationService;
            _eventService = eventService;
        }
             
        [Route("watch")]
        public async Task<IActionResult> Index([FromQuery]string v)
         {

             //TODO check onair flag later now testing
            var StreamDetail = await _streamingService.LiveStreamService.GetAsync("Where VideoId= @VideoId", new { VideoId = v });
            if (StreamDetail == null)
            {
                ViewBag.Message = "Stream does not Exist ";
                return RedirectToAction("WatchError");
            }
            var userId = Convert.ToInt32(User.Identity.GetIdentityUserId());
            var role=  User.Identity.GetRoles().ToArray();
            if (role.Contains("SuperAdmin"))
            {
                ViewBag.VideoUrl = $"{v}/hls";
                ViewBag.streamId = StreamDetail.StreamId;
                return View();
            }
            else
            {
                ViewBag.VideoUrl = $"{v}/hls";
                ViewBag.streamId = StreamDetail.StreamId;
                //COMENTED FOR TESTING
                var CourseTimingStreammappingDetail = await _courseTimingStreamMapping.CourseTimingStreamMappingCrudService.GetAsync("Where StreamId = @streamId", new { @streamId = StreamDetail.StreamId });
                if (CourseTimingStreammappingDetail.Type == "C") //C = Course // means Stream is of Course
                {
                    if (userId != 0)
                    {
                        var Count = await _enrollService.EnrollCrudService.RecordCountAsync("Where UserId=@userId and CourseTimingId=@courseTimingId and IsPaid=@isPaid and IsPaidVerified = @isPaidVerified ", new { @userId = userId, @courseTimingId = CourseTimingStreammappingDetail.ReferenceId, @isPaid = true, @isPaidVerified = true });
                        if (Count != 0)
                        {

                            return View();
                        }
                        else
                        {
                            ViewBag.Message = "You don't Have Access or Pay Respective Course To Watch";
                            return View("WatchError");
                        }
                    }
                    else
                    {
                        ViewBag.Message = "Please Login First to watch Live Courses";
                        return View("WatchError");
                    }

                }
                else if (CourseTimingStreammappingDetail.Type == "E") //E = Event // means Stream is of Event
                {
                    var EventDetail = await _eventService.EventCrudService.GetAsync(CourseTimingStreammappingDetail.ReferenceId);
                    if (EventDetail.IsLoginRequired ==  false)
                    {
                        return View();
                    }
                    else 
                    {
                        if (userId != 0)
                        {
                            if (EventDetail.IsFree == true)
                            {
                                var count = await _eventService.EventRegisterCrudService.RecordCountAsync("Where UserId = @userId and EventId=@eventId", new { @userId = userId, @eventId = CourseTimingStreammappingDetail.ReferenceId });
                                if (count != 0)
                                {
                                    return View();
                                }
                                else
                                {
                                    //user hasnot registered for the events
                                    ViewBag.Message = "You don't Have Access to watch this Live Event";
                                    return View("WatchError");
                                }
                            }
                            else
                            {
                                //redirect to payment
                                ViewBag.Message = "No Access.It is Only For Paid Events.Pay Respective Events To Watch";
                                return View("WatchError");
                            }
                        }
                        else
                        {
                            ViewBag.Message = "Please Login First to watch Live Events";
                            return View("WatchError");
                        }

                    }
                    
                }

                else
                {
                    ViewBag.Message = "Invalid";
                    return View("WatchError");
                }

            }

        }

        [Route("watcherror")]
        public async Task<IActionResult> WatchError()
        {
            return View();
        }
        [Route("watch/UpdateQuestionsForTutor")]
        public async Task<JsonResult> UpdateQuestionsForTutor([FromQuery]long streamId, [FromQuery] string Message)
        {
            try
            {
                if (streamId == 0)
                {
                    return Json(new { Code = 500, Message = "Invalid", Data = false });
                }
                LiveChat model = new LiveChat();
                model.AutoFill();
                model.Message = Message;
                model.StreamId = streamId;
                model.SenderId = (User.Identity.GetIdentityUserId()); 

                var result = await _streamingService.LiveChatService.InsertAsync(model);
                if (result > 0)
                {
                    return Json(new { Code = 200, Message = "success", Data = "OK" });
                }
                else
                {
                    return Json(new { Code = 500, Message = "Error", Data = false });
                }
              
            

            }
            catch (Exception ex)
            {

                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }

        }
        [Route("watch/getMessagesofUserbyStreamId/{streamId}")]
        public async Task<JsonResult> getMessagesofUserbyStreamId([FromRoute]long streamId)
        {
            try
            {
                if (streamId == 0)
                {
                    return Json(new { Code = 500, Message = "Invalid", Data = false });
                }
               
               var SenderId = (User.Identity.GetIdentityUserId());

                var result = await _streamingService.LiveChatService.GetListAsync("Where StreamId = @streamId and SenderId =@senderId",new { @streamId= streamId , @senderId = SenderId });
                if (result.Count()!=0)
                {
                    return Json(new { Code = 200, Message = "success", Data = result });
                }
                else
                {
                    return Json(new { Code = 500, Message = "NoData", Data = false });
                }



            }
            catch (Exception ex)
            {

                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }

        }

    }
}
