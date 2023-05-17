using Kachuwa.Data.Extension;
using Kachuwa.Identity.Extensions;
using Kachuwa.Identity.Models;
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
    public class TrainingTutorController : BaseController
    {
        private readonly ITrainingTutorService _trainingTutorService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly IFileService _iFileService;

        public TrainingTutorController(ITrainingTutorService trainingTutorService, INotificationService notificationService,
                                        ILocaleResourceProvider localeResourceProvider, ILogger logger ,IFileService iFileService)
        {
            _trainingTutorService = trainingTutorService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _iFileService = iFileService;
        }

        [Route("admin/trainingtutor")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/trainingtutor/new")]
        public async Task<IActionResult> New()
        {
            ViewData["FormDataSources"] = await LoadDataSource(0);
            return View();
        }
        [HttpPost]
        [Route("admin/trainingtutor/new")]
        public async Task<IActionResult> New(TrainingTutor model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                    model.AddedOn = DateTime.Now;
                    if (model.ProfileImageAttachment != null)
                    {
                        model.ProfileImagePath = await saveTutorProfileImage(model.ProfileImageAttachment);
                    }
                    else
                    {
                        model.ProfileImagePath = "/";
                    }
                    var status = await _trainingTutorService.TrainingTutorCrudService.InsertAsync<int>(model);
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                    return RedirectToAction("Index");
                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.IdentityUserId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.IdentityUserId);
                return View(model);
            }
        }
        [HttpGet]
        [Route("admin/trainingtutor/edit/{Id}")]
        public async Task<IActionResult> Edit([FromRoute] int id)

        {
            var model = await _trainingTutorService.TrainingTutorCrudService.GetAsync(id);
            ViewData["FormDataSources"] = await LoadDataSource(model.IdentityUserId);
            return View(model);
        }

        [HttpPost]
        [Route("admin/trainingtutor/edit")]
        public async Task<IActionResult> Edit(TrainingTutor model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    model.ModifiedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                    model.ModifiedOn = DateTime.Now;
                    if (model.EditProfileImageAttachment != null)
                    {
                        model.ProfileImagePath = await saveTutorProfileImage(model.EditProfileImageAttachment);
                    }
                    
                    var status = await _trainingTutorService.TrainingTutorCrudService.UpdateAsync(model);
                    _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                    return RedirectToAction("Index");
                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    _notificationService.Notify(_localeResourceProvider.Get("Validation"), string.Join(',', d), NotificationType.Error);
                    ViewData["FormDataSources"] = await LoadDataSource(model.IdentityUserId);
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Exception Occured", NotificationType.Error);
                ViewData["FormDataSources"] = await LoadDataSource(model.IdentityUserId);
                return View(model);
            }
        }

        [HttpPost]
        [Route("admin/trainingtutor/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {

            try
            {
                var model = await _trainingTutorService.TrainingTutorCrudService.GetAsync(id);
                model.DeletedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                model.DeletedOn = DateTime.Now;
                model.IsActive = false;
                model.IsDeleted = true;
                var status = await _trainingTutorService.TrainingTutorCrudService.UpdateAsync(model);
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
        public async Task<string> saveTutorProfileImage(IFormFile imgFile)
        {
            string path = "/";
            try
            {
                if (imgFile != null)
                {
                    path = _iFileService.Save("TrainingTutor/Images", imgFile);

                }
            }
            catch (Exception e)
            {
                path = "/";
            }
            return path;
        }

        [HttpGet]
        [Route("admin/trainingtutor/detail/{id}")]
        public async Task<IActionResult> Detail([FromRoute] int id)

        {
            var model = await _trainingTutorService.TrainingTutorCrudService.GetAsync(id);

            return View(model);
        }

        private async Task<FormDatasource> LoadDataSource(int IdentityUserId = 0)
        {

            FormDatasource formDataSources = new FormDatasource();

            
            if (IdentityUserId == 0)
            {
                var  identityTutorList = await _trainingTutorService.TutorIdentityListNotMappedOnTutor();

                var IdentityTutorList = identityTutorList.Select(e => new FormInputItem()
                {
                    Id = e.Id,
                    Value = e.Id.ToString(),
                    Label = e.Email,
                    IsSelected = e.Id == IdentityUserId,
                });
                formDataSources.SetSource("IdentityTutorListSource", IdentityTutorList);
            }
            else
            {
               var    identityTutorList = await _trainingTutorService.TutorIdentityList(IdentityUserId);

                var IdentityTutorList = identityTutorList.Select(e => new FormInputItem()
                {
                    Id = e.Id,
                    Value = e.Id.ToString(),
                    Label = e.Email,
                    IsSelected = e.Id == IdentityUserId,
                });
                formDataSources.SetSource("IdentityTutorListSource", IdentityTutorList);
            }
            
            


            return formDataSources;
        }
    }

    
}
