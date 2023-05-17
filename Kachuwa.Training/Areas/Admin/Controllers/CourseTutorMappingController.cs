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
    public class CourseTutorMappingController : BaseController
    {
        private readonly ITrainingTutorService _trainingTutorService;
        private readonly ICourseService _courseService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly ICourseTutorMappingService _courseTutorMappingService;

        public CourseTutorMappingController(ITrainingTutorService trainingTutorService,ICourseService courseService,INotificationService notificationService,
                                            ILocaleResourceProvider localeResourceProvider,ILogger logger,ICourseTutorMappingService courseTutorMappingService)
        {
            _trainingTutorService = trainingTutorService;
            _courseService = courseService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _courseTutorMappingService = courseTutorMappingService;

        }

        [Route("admin/coursetutormapping")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/coursetutormapping/new")]
        public async Task<IActionResult> New()
        {
            ViewData["FormDataSources"] = await LoadDataSource(0);
            return View();
        }

        [HttpPost]
        [Route("admin/coursetutormapping/new")]
        public async Task<IActionResult> New(CourseTutorMapping model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.CourseId != 0 && model.TutorId != 0)
                    {
                        int existing = await _courseTutorMappingService.CourseTutorMappingCrudService.RecordCountAsync("Where CourseId = @courseId and TutorId = @tutorId", new { @courseId = model.CourseId, @tutorId = model.TutorId });
                        if (existing == 0)
                        {
                            var status = await _courseTutorMappingService.CourseTutorMappingCrudService.InsertAsync<int>(model);
                            _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Already Mapped Selected Course in Selected Tutor", NotificationType.Error);
                            ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                            return View(model);
                        }
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Please Select Both Course and Tutor", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                        return View(model);
                    }       
                    

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                return View(model);
            }
        }

        [HttpGet]
        [Route("admin/coursetutormapping/edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await _courseTutorMappingService.CourseTutorMappingCrudService.GetAsync(id);
            ViewData["FormDataSources"] = await LoadDataSource(data.CourseId,data.TutorId);
            return View(data);
        }

        [HttpPost]
        [Route("admin/coursetutormapping/edit")]
        public async Task<IActionResult> Edit(CourseTutorMapping model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.CourseId != 0 && model.TutorId != 0)
                    {
                        int existing = await _courseTutorMappingService.CourseTutorMappingCrudService.RecordCountAsync("Where CourseId = @courseId and TutorId = @tutorId and Id != @Id", new { @courseId = model.CourseId, @tutorId = model.TutorId ,@Id=model.Id});
                        if (existing == 0)
                        {
                            var status = await _courseTutorMappingService.CourseTutorMappingCrudService.UpdateAsync(model);
                            _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                            return RedirectToAction("Index");
                        }
                        else
                        {
                            _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Already Mapped Selected Course in Selected Tutor", NotificationType.Error);
                            ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                            return View(model);
                        }
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Please Select Both Course and Tutor", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                        return View(model);
                    }


                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.CourseId, model.TutorId);
                return View(model);
            }
        }

        [HttpPost]
        [Route("admin/coursetutormapping/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {

            try
            {
                var status = await _courseService.CourseCrudService.DeleteAsync(id);
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

        private async Task<FormDatasource> LoadDataSource(int CourseId = 0, int TutorId = 0)
        {

            FormDatasource formDataSources = new FormDatasource();

            var courseList = await _courseService.CourseCrudService.GetListAsync("Where IsActive = @isActive", new { @isActive = true });
            //var courseList = await _courseService.CourseNotMappedOnCourseTutorList();
            var TrainingCourseList = courseList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Id.ToString(),
                Label = e.Name,
                IsSelected = e.Id == CourseId,
            });
            formDataSources.SetSource("CourseListSource", TrainingCourseList);

            var tutorList = await _trainingTutorService.TrainingTutorCrudService.GetListAsync("Where IsActive = @isActive", new { @isActive = true });
            var TrainingTutorList = tutorList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Id.ToString(),
                Label = e.Name + '('+e.Expertise +')',
                IsSelected = e.Id == TutorId,
            });
            formDataSources.SetSource("TutorListSource", TrainingTutorList);


            return formDataSources;
        }
    }
}
