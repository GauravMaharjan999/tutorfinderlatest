using System;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Dash.Services;
using Kachuwa.Identity.Extensions;
using Kachuwa.Log;
using Kachuwa.Training.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.KLiveApp.Areas.Controllers
{
    [Area("User")]
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IStreamingService _streamingService;
        private readonly ILogger _logger;
        private readonly IEnrollService _enrollService;
        
        private readonly IPaymentLogService _paymentLogService;

        public DashboardController(IStreamingService streamingService,ILogger logger ,IEnrollService enrollService ,IPaymentLogService paymentLogService)
        {
            _streamingService = streamingService;
            _logger = logger;
            _enrollService = enrollService;
            _paymentLogService = paymentLogService;
        }
        public IActionResult Index()
        {
            return View();
        }
        [Route("user/video")]
        public IActionResult Video()
        {
            return View();
        }
       // [Route("user/live")]
        [Route("user/live/{streamId}")]
        public async Task<IActionResult> Live(long streamId)
        {
            if (streamId == 0)
                return Redirect("Index");
            var stream = await _streamingService.LiveStreamService.GetAsync(streamId);
            return View(stream);
        }
        [Route("user/live/UpdateIsOnAirStatus")]
        public async Task<JsonResult> UpdateIsOnAirStatus([FromQuery]long streamId,[FromQuery] bool IsOnAir)
        {
            try
            {
                if (streamId == 0)
                {
                    return Json(new { Code = 500, Message = "Invalid", Data = false });
                }
                var stream = await _streamingService.LiveStreamService.GetAsync(streamId);
                stream.IsOnAir = IsOnAir;
                stream.OnAiredAt = DateTime.Now;
                var result = await _streamingService.LiveStreamService.UpdateAsync(stream);
                if (result)
                {
                    return Json(new { Code = 200, Message = "success", Data = "OK" });
                }
                else
                {
                    return Json(new { Code = 500, Message = "UpdateFalse", Data = false });
                }
               
            }
            catch (Exception ex)
            {

                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }
            
        }
        [Route("user/live/UpdateStreamFinished")]
        public async Task<JsonResult> UpdateStreamFinished([FromQuery]long streamId)
        {
            try
            {
                if (streamId == 0)
                {
                    return Json(new { Code = 500, Message = "Invalid", Data = false });
                }
                var stream = await _streamingService.LiveStreamService.GetAsync(streamId);
                stream.IsFinished = true;
                stream.FinishedAt = DateTime.Now;
                var result = await _streamingService.LiveStreamService.UpdateAsync(stream);
                if (result)
                {
                    return Json(new { Code = 200, Message = "success", Data = "OK" });
                }
                else
                {
                    return Json(new { Code = 500, Message = "UpdateFalse", Data = false });
                }

            }
            catch (Exception ex)
            {

                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }

        }
        [Route("user/playlist")]
        public IActionResult Playlist()
        {
            return View();
        }
        [Route("user/setting")]
        public IActionResult Setting()
        {
            return View();
        }
        [Route("user/order")]
        public async Task<IActionResult> Order()
        {
            var CurrentUserId = Convert.ToInt32(User.Identity.GetIdentityUserId());
            var EnrollCourseList = await _enrollService.EnrollListByUserId(CurrentUserId);
            //if (EnrollCourseList == null)
            //{
            //    return View();
            //}
            return View(EnrollCourseList);
        }




        [Route("user/payment")]
        public async Task<IActionResult> Payment([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {
            var CurrentUserId = Convert.ToInt32(User.Identity.GetIdentityUserId());
            var paymentList = await _paymentLogService.PaymentListbyUserId(CurrentUserId, pq_curpage, pq_rpp);
            return View(paymentList);
        }

        [Route("user/payment/detail/{id}")]
        public async Task<IActionResult> PaymentDetail([FromRoute] int id)
        {
            var paymentdetail = await _paymentLogService.PaymentDetailByPaymentId(id);
            return View(paymentdetail);
        }

        [Route("user/live/getMessagesSentbyUser/{streamId}")]
        public async Task<JsonResult> getMessagesofUserbyStreamId([FromRoute]long streamId)
        {
            try
            {
                if (streamId == 0)
                {
                    return Json(new { Code = 500, Message = "Invalid", Data = false });
                }

                var result = await _streamingService.LiveChatService.GetListAsync("Where StreamId = @streamId", new { @streamId = streamId});
                if (result.Count() != 0)
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