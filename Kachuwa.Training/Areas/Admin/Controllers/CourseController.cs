using Kachuwa.Identity.Extensions;
using Kachuwa.Localization;
using Kachuwa.Log;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Kachuwa.Web.Form;
using Kachuwa.Web.Notification;
using Kachuwa.Web.Security;
using Kachuwa.Web.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
    public class CourseController : BaseController
    {
        private readonly ICourseService _courseService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly ITrainingCourseCategoryService _trainingCourseCategoryService;
        private readonly IFileService _iFileService;

        public CourseController(ICourseService courseService,INotificationService notificationService ,ILocaleResourceProvider localeResourceProvider,ILogger logger,
                                    ITrainingCourseCategoryService trainingCourseCategoryService ,IFileService ifileService)
        {
            _courseService = courseService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _trainingCourseCategoryService = trainingCourseCategoryService;
            _iFileService = ifileService;
        }

        [Route("admin/course")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/course/new")]
        public async Task<IActionResult> New()
        {
            ViewData["FormDataSources"] = await LoadDataSource(0);
            return View();
        }
        [HttpPost]
        [Route("admin/course/new")]
        public async Task<IActionResult> New(Course model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _courseService.CourseCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Name))) = @name and IsDeleted = @isDeleted and CategoryId=@categoryId", new { @name = (model.Name.Trim()).ToLower(), @isDeleted = false , @categoryId = model.CategoryId});
                    if (existing == 0)
                    {
                        model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.AddedOn = DateTime.Now;
                        if (model.ProfileImageAttachment != null)
                        {
                             model.ProfileImagePath = await SaveCourseProfileImage(model.ProfileImageAttachment);
                        }
                        else
                        {
                            model.ProfileImagePath = "/";
                        }
                        if (model.CoverImageAttachment != null)
                        {
                            model.CoverImagePath = await SaveCourseCoverImage(model.CoverImageAttachment);
                        }
                        else
                        {
                            model.CoverImagePath = "/";
                        }
                        var status = await _courseService.CourseCrudService.InsertAsync<int>(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index","Course",new { area = "admin"});
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Course Name in Category", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                        return View(model);
                    }
                 
                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                return View(model);
            }
        }

        [HttpGet]
        [Route("admin/course/edit/{Id}")]
        public async Task<IActionResult> Edit([FromRoute] int id)

        {
            var model = await _courseService.CourseCrudService.GetAsync(id);
            ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
            return View(model);
        }

        [HttpPost]
        [Route("admin/course/edit")]
        public async Task<IActionResult> Edit(Course model)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _courseService.CourseCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Name))) = @name and IsDeleted = @isDeleted and CategoryId=@categoryId and Id !=@id", new { @name = (model.Name.Trim()).ToLower(), @isDeleted = false, @categoryId = model.CategoryId ,@id = model.Id});
                    if (existing == 0)
                    {
                        model.ModifiedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.ModifiedOn = DateTime.Now;
                            if (model.EditProfileImageAttachment != null)
                        {
                            model.ProfileImagePath = await SaveCourseProfileImage(model.EditProfileImageAttachment);
                        }
                        
                        if (model.EditCoverImageAttachment != null)
                        {
                            model.CoverImagePath = await SaveCourseCoverImage(model.EditCoverImageAttachment);
                        }
                        
                        var status = await _courseService.CourseCrudService.UpdateAsync(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index", "Course", new { area = "admin" });
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Course Name in Category", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                        return View(model);
                    }

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.CategoryId,model.CourseLevel);
                return View(model);
            }
        }

        [HttpPost]
        [Route("admin/course/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {

            try
            {
                var model = await _courseService.CourseCrudService.GetAsync(id);
                model.DeletedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                model.DeletedOn = DateTime.Now;
                model.IsActive = false;
                model.IsDeleted = true;
                var status = await _courseService.CourseCrudService.UpdateAsync(model);
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


        private async Task<FormDatasource> LoadDataSource(int CategoryId = 0 , string courseLevel = "")
        {

            FormDatasource formDataSources = new FormDatasource();

            var CategoryList = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.GetListAsync("Where IsDeleted = @isDeleted", new { @isDeleted = false });
            var TrainingCategoryList = CategoryList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Id.ToString(),
                Label = e.Name,
                IsSelected = e.Id == CategoryId,
            });
            formDataSources.SetSource("CategoryListSource", TrainingCategoryList);

            var courseLevelList = new CourseLevel().List();
            var courseLevelFormList = courseLevelList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Name.ToString(),
                Label = e.Name,
                IsSelected = e.Name == courseLevel,
            });
            formDataSources.SetSource("CourseLevelListSource", courseLevelFormList);


            return formDataSources;
        }

        public async Task<string> SaveCourseProfileImage(IFormFile imgFile)
        {
            string path = "/";
            try
            {
                if (imgFile != null)
                {
                    path = _iFileService.Save("Course/ProfileImages", imgFile);

                }
            }
            catch (Exception e)
            {
                path = "/";
            }
            return path;
        }

        public async Task<string> SaveCourseCoverImage(IFormFile imgFile)
        {
            string path = "/";
            try
            {
                if (imgFile != null)
                {
                    path = _iFileService.Save("Course/CoverImages", imgFile);

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
