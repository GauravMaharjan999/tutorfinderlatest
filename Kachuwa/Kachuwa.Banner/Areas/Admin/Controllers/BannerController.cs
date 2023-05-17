using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Kachuwa.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kachuwa.Extensions;
using Kachuwa.Web.Form;
using Kachuwa.Web.Notification;
using Kachuwa.Data.Extension;

namespace Kachuwa.Banner.Controllers
{
    [Area("Admin")]

    public class BannerController : BaseController
    {
        public IHostingEnvironment HostingEnvironment { get; private set; }
        private readonly IBannerService _bannerService;
        private readonly INotificationService _notificationService;

        public BannerController(IBannerService bannerService,
            IHostingEnvironment hostingEnvironment, INotificationService notificationService)
        {
            HostingEnvironment = hostingEnvironment;
            _bannerService = bannerService;
            _notificationService = notificationService;
        }


        [Route("admin/banner/page/{pageNo?}")]
        [Route("admin/banner")]//default make it at last
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1


            , [FromQuery]string query = "")
        {
            ViewData["Page"] = pageNo;
            int rowsPerPage = 10;
            var model = await _bannerService.GetBannersList(pageNo, rowsPerPage, query);
            return View(model);
        }


        [Route("admin/banner/new")]
        public async Task<IActionResult> New()
        {
            try
            {
                var bannerKeys = await _bannerService.KeyCrudService.GetListAsync("Where IsDeleted=@IsDeleted", new { IsDeleted = false });
                ViewBag.BannerKeys = bannerKeys;
                return View();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        [HttpPost]
        [Route("admin/banner/new")]
        public async Task<IActionResult> New(BannerInfo model)
        {
            if (ModelState.IsValid)
            {
                model.AutoFill();
                if (model.BannerId == 0)
                {
                   
                    await _bannerService.BannerCrudService.InsertAsync<int>(model);
                    _notificationService.Notify("Success", "Data has been saved succesfully.",
                        NotificationType.Success);
                    return RedirectToAction("Index");
                }

                _notificationService.Notify("Alert", "Invalid inputs or missing inputs on submited form.",
                    NotificationType.Warning);

                return View(model);

            }
            else
            {
                _notificationService.Notify("Alert", "Invalid inputs or missing inputs on submited form.",
                    NotificationType.Warning);


                return View(model);
            }
        }

        [Route("admin/banner/edit/{bannerId}")]
        public async Task<IActionResult> Edit([FromRoute]int bannerId)
        {
            var banner = await _bannerService.BannerCrudService.GetAsync(bannerId);
            ViewData["FormDataSource"] = await GetFormDataSources();
            var model = banner.To<BannerViewModel>();
            var setting = await _bannerService.SettingCrudService.GetAsync("Where KeyId=@KeyId",new{ KeyId =banner.KeyId});
            model.ImageHeight = setting.ImageHeight;
            model.ImageWidth = setting.ImageWidth;
            return View(model);
        }

        [HttpPost]
        [Route("admin/banner/edit")]
        public async Task<IActionResult> Edit(BannerInfo model)
        {
            if (ModelState.IsValid)
            {
                model.AutoFill();
                if (model.BannerId != 0)
                {

                    await _bannerService.BannerCrudService.UpdateAsync(model);
                    _notificationService.Notify("Success", "Data has been saved succesfully.",
                        NotificationType.Success);

                    return RedirectToAction("Index");
                }
                _notificationService.Notify("Alert", "Invalid inputs or missing inputs on submited form.",
                    NotificationType.Warning);

                ViewData["FormDataSource"] = await GetFormDataSources();
                return View(model);
            }
            else
            {
                ViewData["FormDataSource"] = await GetFormDataSources();
                return View(model);
            }
        }

        private async Task<FormDatasource> GetFormDataSources(int theaterId = 0)
        {

            var formDataSource = new FormDatasource();

            return formDataSource;
        }
        [HttpPost]
        [Route("admin/banner/delete")]
        public async Task<JsonResult> Delete(int id)
        {
             await _bannerService.BannerCrudService.DeleteAsync("Where KeyId=@KeyId",new{ KeyId =id});
            var result = await _bannerService.KeyCrudService.DeleteAsync(id);
            _notificationService.Notify("Success", "Data deleted succesfully.",
                NotificationType.Success);
            return Json(result);
        }


        //public ActionResult BannerKey()
        //{
        //    return View();
        //}

        //public async Task<ActionResult> New(int? id)
        //{
        //    if (id == null)
        //    {
        //        var condition = "where IsDeleted = @deleted";
        //        var bannerKeys = await _bannerService.BannerKey.GetListAsync(condition, new { deleted = 0 });
        //        ViewBag.BannerKeys = bannerKeys;
        //        return View();
        //    }
        //    else
        //    {
        //        var bannerDetail = await _bannerService.Banner.GetAsync(id);
        //        var bannerKeys = _bannerService.BannerKey.GetList();
        //        ViewBag.BannerKeys = bannerKeys;

        //        return View(bannerDetail);
        //    }

        //}

        //public async Task<ActionResult> Detail(int id)
        //{
        //    var conditions = "where BannerKeyId = @BannerKeyId and IsActive = 1 and IsDeleted = 0";

        //    var bannerList = await _bannerService.Banner.GetListAsync(conditions, new { BannerKeyId = id });

        //    return View(bannerList);
        //}

        //public async Task<ActionResult> Edit(int id)
        //{
        //    var bannerDetail = await _bannerService.BannerCrudService.GetAsync(id);
        //    var bannerKeys = _bannerService.KeyCrudService.GetListAsync("Where IsDeleted=@IsDeleted",new{IsDeleted=false});
        //    ViewBag.BannerKeys = bannerKeys;
        //    return View("AddNewBanner", bannerDetail);
        //}


    }
}