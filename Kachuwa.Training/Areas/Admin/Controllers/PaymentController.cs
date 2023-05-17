using Kachuwa.Data;
using Kachuwa.Localization;
using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(PolicyConstants.PagePermission)]
    public class PaymentController : BaseController
    {
        private readonly IPaymentLogService _paymentLogService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly IEnrollService _enrollService;

        public PaymentController(IPaymentLogService paymentLogService ,INotificationService notificationService,ILocaleResourceProvider localeResourceProvider,ILogger logger,IEnrollService enrollService)
        {
            _paymentLogService = paymentLogService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _enrollService = enrollService;
        }

        [Route("admin/payment")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [Route("admin/payment/detail/{id}")]
        public async Task<IActionResult> Detail([FromRoute] int id)
        {
            var paymentdetail = await _paymentLogService.PaymentDetailByPaymentId(id);
            return View(paymentdetail);

        }

        [Route("admin/payment/approve/{id}")]
        public async Task<IActionResult> Verify([FromRoute] int id)
        {
            var paymentdetail = await _paymentLogService.PaymentLogCrudService.GetAsync(id);
            var enrolldetail = await _enrollService.EnrollCrudService.GetAsync(paymentdetail.EnrollId);

            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                using (var tran = db.BeginTransaction())
                {
                    try
                    {
                        paymentdetail.IsVerified = true;
                        var status = await _paymentLogService.PaymentLogCrudService.UpdateAsync(db,paymentdetail,tran,null);
                        enrolldetail.IsPaidVerified = true;
                        var enrollstatus = await _enrollService.EnrollCrudService.UpdateAsync(db,enrolldetail,tran,null);

                        tran.Commit();
                    }
                    catch (Exception ex)
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Error"), _localeResourceProvider.Get("Couldnot Verify"), NotificationType.Error);
                        _logger.Log(LogType.Error, () => ex.Message, ex);
                        tran.Rollback();
                        return RedirectToAction("Index");
                    }
                }
            }
            _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Approve Successfully successfully!"), NotificationType.Success);
            return RedirectToAction("Index");

        }
    }
}
