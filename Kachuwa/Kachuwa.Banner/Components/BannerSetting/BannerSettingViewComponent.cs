using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Banner;
using Kachuwa.Log;
using Kachuwa.Web;
using Kachuwa.Web.Module;
using Kachuwa.Web.Notification;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.Banner.Components
{
   
    [ViewComponent(Name = "BannerSetting")]
    public class BannerSettingViewComponent : KachuwaModuleViewComponent<BannerModule>
    {
        private readonly ILogger _logger;
        private readonly IBannerService _bannerService;

        public BannerSettingViewComponent( IModuleManager moduleManager, ILogger logger,
            IBannerService bannerService) : base(moduleManager)
        {
            _logger = logger;
            _bannerService = bannerService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var banners = await _bannerService.KeyCrudService.GetListAsync(
                    "Where IsDeleted=@IsDeleted and IsActive=@IsActive"
                    , new {IsActive = true, IsDeleted = false});
                return View(banners);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Banner loading error.", e);
                throw e;
            }

        }

        public override string DisplayName { get; } = "Banner Setting";
        public override bool IsVisibleOnUI { get; } = true;
    }
}
