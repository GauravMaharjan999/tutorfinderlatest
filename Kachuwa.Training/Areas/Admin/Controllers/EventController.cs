using Kachuwa.Identity.Extensions;
using Kachuwa.Localization;
using Kachuwa.Log;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Kachuwa.Web;
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
    public class EventController : BaseController
    {
        private readonly IEventService _eventService;
        private readonly INotificationService _notificationService;
        private readonly ILocaleResourceProvider _localeResourceProvider;
        private readonly ILogger _logger;
        private readonly IFileService _iFileService;

        public EventController(IEventService eventService,INotificationService notificationService, ILocaleResourceProvider localeResourceProvider,
                                ILogger logger , IFileService ifileService )
        {
            _eventService = eventService;
            _notificationService = notificationService;
            _localeResourceProvider = localeResourceProvider;
            _logger = logger;
            _iFileService = ifileService;
        }

        [Route("admin/event")]
        public async Task<IActionResult> Index()
        {

            return View();

        }
        [HttpGet]
        [Route("admin/event/new")]
        public async Task<IActionResult> New()
        {
            return View();
        }

        [HttpPost]
        [Route("admin/event/new")]
        public async Task<IActionResult> New(Event model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _eventService.EventCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Title))) = @title and IsDeleted = @isDeleted", new { @title = (model.Title.Trim()).ToLower(), @isDeleted = false });
                    if (existing == 0)
                    {
                        model.AddedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.AddedOn = DateTime.Now;
                        if (model.ProfileImageAttachment != null)
                        {
                            model.ProfileImagePath = await SaveEventProfileImage(model.ProfileImageAttachment);
                        }
                        else
                        {
                            model.ProfileImagePath = "/";
                        }
                        if (model.IsFree == true)
                        {
                            model.Price = 0;
                        }
                        var status = await _eventService.EventCrudService.InsertAsync<int>(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index", "Event", new { area = "admin" });
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Title in Event", NotificationType.Error);
                        return View(model);
                    }

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
        [HttpGet]
        [Route("admin/event/edit/{id}")]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            var data = await _eventService.EventCrudService.GetAsync(id);
            return View(data);
        }

        [HttpPost]
        [Route("admin/event/edit")]
        public async Task<IActionResult> Edit(Event model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    int existing = await _eventService.EventCrudService.RecordCountAsync("Where Lower(ltrim(rtrim(Title))) = @title and IsDeleted = @isDeleted and Id != @Id", new { @title = (model.Title.Trim()).ToLower(), @isDeleted = false , @Id = model.Id });
                    if (existing == 0)
                    {
                        model.ModifiedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                        model.ModifiedOn = DateTime.Now;
                        if (model.EditProfileImageAttachment != null)
                        {
                            model.ProfileImagePath = await SaveEventProfileImage(model.EditProfileImageAttachment);
                        }
                        if (model.IsFree == true)
                        {
                            model.Price = 0;
                        }
                        var status = await _eventService.EventCrudService.UpdateAsync(model);
                        _notificationService.Notify(_localeResourceProvider.Get("Success"), _localeResourceProvider.Get("Data has been saved successfully!"), NotificationType.Success);
                        return RedirectToAction("Index", "Event", new { area = "admin" });
                    }
                    else
                    {
                        _notificationService.Notify(_localeResourceProvider.Get("Validation"), "Duplicate Title in Event", NotificationType.Error);

                        return View(model);
                    }

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
        [Route("admin/event/delete")]
        public async Task<JsonResult> Delete([FromBody]int id)
        {

            try
            {
                var model = await _eventService.EventCrudService.GetAsync(id);
                model.DeletedBy = Convert.ToInt32(User.Identity.GetIdentityUserId());
                model.DeletedOn = DateTime.Now;
                model.IsActive = false;
                model.IsDeleted = true;
                var status = await _eventService.EventCrudService.UpdateAsync(model);
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

        public async Task<string> SaveEventProfileImage(IFormFile imgFile)
        {
            string path = "/";
            try
            {
                if (imgFile != null)
                {
                    path = _iFileService.Save("Event/ProfileImages", imgFile);

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
