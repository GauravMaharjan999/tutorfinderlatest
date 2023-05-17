using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Identity.Extensions;
using Kachuwa.Localization;
using Kachuwa.Log;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.LiveTrainingApp.Controllers
{
  
    public class EnrollController : Controller
    {
        private readonly ICourseService _courseService;
        private readonly ICourseTimingService _courseTimingService;
        private readonly IEnrollService _enrollService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly IPaymentLogService _paymentLogService;
        private readonly IFileService _ifileService;
       
        public EnrollController(ICourseService courseService, ICourseTimingService courseTimingService, IEnrollService enrollService, INotificationService notificationService, 
                                ILocaleResourceProvider localeResourceProvider, ILogger logger,IPaymentLogService paymentLogService,IFileService ifileService)
        {
            _courseService = courseService;
            _courseTimingService = courseTimingService;
            _enrollService = enrollService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _ifileService = ifileService;
            _paymentLogService = paymentLogService;
        }
        public IActionResult Index()
        {
            return View();
        }

        [Route("Checkenroll")]
        public async Task<IActionResult> CheckEnroll([FromQuery]int UserId ,[FromQuery] int CourseId)
        {
           var enrolldetail = (await _enrollService.EnrollCrudService.GetListAsync("Where UserId=@userId and CourseId = @courseId",new { @userId = UserId, @courseId = CourseId })).LastOrDefault();
            if (enrolldetail == null)
            {
                //no enrollment in course by user
                var path = "/enroll/" + CourseId;
                return base.Redirect(path);
            }
            else if (enrolldetail.CourseEndDate < DateTime.Now.Date)
            {
                //enrollment has been finished courseenddate
                var path = "/enroll/" + CourseId;
                return base.Redirect(path);
            }
            else if (enrolldetail.CourseEndDate > DateTime.Now.Date)
            {
                if (enrolldetail.IsPaid==false)
                {
                    var path = "/enroll/payment/" + enrolldetail.Id;
                    return base.Redirect(path);
                }
                return View();
            }
            return View();
        }


        [Route("enroll/{courseId}")]
        public async Task<IActionResult> Detail([FromRoute]int courseId)
        {
           
            EnrollViewModel enrollviewModelobj = new EnrollViewModel();
            enrollviewModelobj.courseDetailViewModel = await _courseService.GetCourseandTutorDetailsByCourseId(courseId);
            enrollviewModelobj.courseTimimg = await _courseTimingService.CourseTimingCrudService.GetListAsync("Where CourseId =@courseId and FromDate <= @fromDate",new { @courseId= courseId,@fromDate =DateTime.Now.Date });
            return View(enrollviewModelobj);
        }

        [HttpPost]
        [Route("enroll/placeenroll")]
        public async Task<JsonResult> PlaceEnroll([FromBody]Enroll model)
        {

            try
            {
                if (ModelState.IsValid)
                {

                    model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                    model.AddedOn = DateTime.Now;
                    model.IsPaid = false;
                    model.IsPaidVerified = false;
                    model.PaymentMethod = "NotNow";
                    int enrollId = await _enrollService.EnrollCrudService.InsertAsync<int>(model);
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Enrollment Successful!"), NotificationType.Success);
                    return Json(new { Code = 200, Message = "success", Data = enrollId });

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    return Json(new { Code = 500, Message = string.Join(',', d), Data = false });
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }

        }

        [Route("enroll/payment/{enrollId}")]
        public async Task<IActionResult> EnrollPayment([FromRoute]int enrollId)
        {

            Enroll enrollDetail = await _enrollService.EnrollCrudService.GetAsync(enrollId);
            return View(enrollDetail);
        }

        [HttpPost]
        [Route("enroll/payment")]
        public async Task<IActionResult> EnrollPaymentPost(Enroll model)
        {
            if (model.PaymentMethod =="" || model.PaymentMethod == null )
            {
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Please Select Payment Method", NotificationType.Error);
                return View(model);
            }
            else
            {
                var enrollDetail = await _enrollService.EnrollCrudService.GetAsync(model.Id);
                enrollDetail.PaymentMethod = model.PaymentMethod;
                var status = await _enrollService.EnrollCrudService.UpdateAsync(enrollDetail);

                if (enrollDetail.PaymentMethod == "BankTransfer")
                {
                    return RedirectToAction("PaymentByDirectBankTransfer", enrollDetail);
                }
                else if (enrollDetail.PaymentMethod == "Esewa")
                {
                    return RedirectToAction("PaymentByEsewa", enrollDetail);
                }
                else
                {
                    return View();
                }
            }
            
            
        }


        [Route("enroll/payment/banktransfer")]
        public async Task<IActionResult> PaymentByDirectBankTransfer(Enroll enrollDetail)
        {

            ViewBag.EnrollDetail = enrollDetail;
            return View();
        }

        [HttpPost]
        [Route("enroll/payment/banktransfer")]
        public async Task<IActionResult> PaymentByDirectBankTransfer(PaymentLog model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.VoucherAttachment != null)
                    {
                        model.VoucherAttachmentPath = await SaveVoucherAttachment(model.VoucherAttachment);
                    }
                    else
                    {
                        model.VoucherAttachmentPath = "/";
                    }

                    var enrollDetail = await _enrollService.EnrollCrudService.GetAsync(model.EnrollId);
                    enrollDetail.IsPaid = true;
                    var enrollUpdate = await _enrollService.EnrollCrudService.UpdateAsync(enrollDetail);

                    var status = await _paymentLogService.PaymentLogCrudService.InsertAsync<int>(model);
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                    return RedirectToAction("PaymentSuccess");

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                return View(model);
            }
            
        }
        [Route("enroll/payment/paymentsuccess")]
        public async Task<IActionResult> PaymentSuccess()
        {
            return View();
        }



        [Route("enroll/payment/esewa")]
        public async Task<IActionResult> PaymentByEsewa(Enroll enrollDetail)
        {

            ViewBag.EnrollDetail = enrollDetail;
            return View();
        }

        [HttpPost]
        [Route("enroll/payment/esewa/success")]
        public async Task<IActionResult> PaymentByEsewaSuccess(PaymentLog model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    //savepayment from esewa paramerter to log
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                    return RedirectToAction("PaymentSuccess");

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                return View(model);
            }

        }
        [HttpPost]
        [Route("enroll/payment/esewa/failed")]
        public async Task<IActionResult> PaymentByEsewaFailed(PaymentLog model)
        {
            return View();
            

        }
        public async Task<string> SaveVoucherAttachment(IFormFile imgFile)
        {
            string path = "/";
            try
            {
                if (imgFile != null)
                {
                    path = _ifileService.Save("PaymentLog/BankTransferVoucher", imgFile);

                }
            }
            catch (Exception e)
            {
                path = "/";
            }
            return path;
        }


    }

}