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
    public class TrainingCourseCategoryController : BaseController
    {
        private readonly ITrainingCourseCategoryService _trainingCourseCategoryService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;


        public TrainingCourseCategoryController(ITrainingCourseCategoryService trainingCourseCategoryService, INotificationService notificationService,
                                                ILocaleResourceProvider localeResourceProvider, ILogger logger)
        {
            _trainingCourseCategoryService = trainingCourseCategoryService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;

        }

        [Route("admin/coursecategory")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/coursecategory/new")]
        public async Task<IActionResult> New()
        {
            ViewData["FormDataSources"] = await LoadDataSource(0);
            return View();
        }

        [HttpPost]
        [Route("admin/coursecategory/new")]
        public async Task<IActionResult> New(TrainingCourseCategory model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Name))) = @name and IsDeleted = @isDeleted and ParentId=@parentId", new { @name = (model.Name.Trim()).ToLower(), @isDeleted = false , @parentId =model.ParentId});
                    if (existing == 0)
                    {
                        model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.AddedOn = DateTime.Now;
                        var status = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.InsertAsync<int>(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Category Name in Parent", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                        return View(model);
                    }

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                return View(model);
            }
        }

        [HttpGet]
        [Route("admin/coursecategory/edit/{Id}")]
        public async Task<IActionResult> Edit([FromRoute] int id)

        {
            var model = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.GetAsync(id);
            ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
            return View(model);
        }

        [HttpPost]
        [Route("admin/coursecategory/edit")]
        public async Task<IActionResult> Edit(TrainingCourseCategory model)

        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Name))) = @name and IsDeleted = @isDeleted and Id != @id and ParentId=@parentId", new { @name = (model.Name.Trim()).ToLower(), @isDeleted = false ,@id=model.Id , @parentId = model.ParentId });
                    if (existing == 0)
                    {
                        model.ModifiedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.ModifiedOn = DateTime.Now;
                        var status = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.UpdateAsync(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Category Name in Parent", NotificationType.Error);
                        ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                        return View(model);
                    }

                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.ParentId);
                return View(model);
            }
        }


        [HttpPost]
        [Route("admin/coursecategory/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {
            try
            {
                var checkChildName = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.RecordCountAsync("where ParentId=@ParentId", new { @ParentId = id });
                if (checkChildName == 0)
                {
                    var model = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.GetAsync(id);
                    model.IsDeleted = true;
                    model.IsActive = false;
                    model.DeletedOn = DateTime.Now;
                    model.DeletedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                    var status = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.UpdateAsync(model);
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data deleted successfully!"), NotificationType.Success);
                    return Json(new { Code = 200, Message = "", Data = "OK" });
                }
                else
                {
                    _notificationService.Notify(_localeResourceProvider.Get("Error"), _localeResourceProvider.Get("Child Category Exists Cannot Delete!"), NotificationType.Error);
                    return Json(new { Code = 500, Message = "", Data = false });
                }

            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(ex.Message, NotificationType.Error);
                return Json(new { Code = 500, Message = ex.Message, Data = false });
            }
        }

        private async Task<FormDatasource> LoadDataSource(int ParentId = 0)
        {

            FormDatasource formDataSources = new FormDatasource();

            var CategoryList = await _trainingCourseCategoryService.TrainingCourseCategoryCrudService.GetListAsync("Where IsDeleted = @isDeleted", new { @isDeleted = false });
            var TrainingCategoryList = CategoryList.Select(e => new FormInputItem()
            {
                Id = e.Id,
                Value = e.Id.ToString(),
                Label = e.Name,
                IsSelected = e.Id == ParentId,
            });
            formDataSources.SetSource("CategoryListSource", TrainingCategoryList);




            return formDataSources;
        }
    }
}
