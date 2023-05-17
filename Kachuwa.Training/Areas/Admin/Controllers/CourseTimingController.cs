using Kachuwa.Identity.Extensions;
using Kachuwa.Localization;
using Kachuwa.Log;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Kachuwa.Web.Form;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(PolicyConstants.PagePermission)]
    public class CourseTimingController : BaseController
    {
        private readonly ICourseService _courseService;
        private readonly ICourseTimingService _courseTimingService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;

        public CourseTimingController(ICourseService courseService, ICourseTimingService courseTimingService,
                                      INotificationService notificationService,ILocaleResourceProvider localeResourceProvider,ILogger logger)
        {
            _courseService = courseService;
            _courseTimingService = courseTimingService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
        }
        [Route("admin/coursetiming")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/coursetiming/new")]
        public async Task<IActionResult> New()
        {
            ViewData["FormDataSources"] = await LoadDataSource(0);
            CourseTiming CT = new CourseTiming();
            CT.FromDate = DateTime.Now;
            CT.ToDate = DateTime.Now;
            return View(CT);
        }

        [HttpPost]
        [Route("admin/coursetiming/new")]
        public async Task<IActionResult> New(CourseTiming model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.CourseId != 0 )
                    {
                        model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.AddedOn = DateTime.Now;
                            var status = await _courseTimingService.CourseTimingCrudService.InsertAsync<int>(model);
                            _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                            return RedirectToAction("Index");
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Please Select Course", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.CourseId);
                        return View(model);
                    }


                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.CourseId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.CourseId);
                return View(model);
            }
        }

        [HttpPost]
        [Route("admin/coursetiming/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {

            try
            {
                var status = await _courseTimingService.CourseTimingCrudService.DeleteAsync(id);
                _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data deleted successfully!"), NotificationType.Success);
                return Json(new { Code = 200, Message = "", Data = "OK" });
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Error"), "Exception Occured", NotificationType.Error);
                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }

        }

        private async Task<FormDatasource> LoadDataSource(int CourseId = 0)
        {

            FormDatasource formDataSources = new FormDatasource();

            var courseList = await _courseService.CourseCrudService.GetListAsync("Where IsActive = @isActive", new { @isActive = true });
            var TrainingCourseList = courseList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Id.ToString(),
                Label = e.Name,
                IsSelected = e.Id == CourseId,
            });
            formDataSources.SetSource("CourseListSource", TrainingCourseList);


            return formDataSources;
        }


    }
}
