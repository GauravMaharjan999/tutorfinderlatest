using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Log;
using Kachuwa.Web;
using Kachuwa.Web.Module;
using Kachuwa.Web.Notification;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.Banner.Components
{
   
    [ViewComponent(Name = "Banner")]
    public class BannerViewComponent : KachuwaModuleViewComponent<BannerModule>
    {
        private readonly ILogger _logger;
        private readonly IBannerService _bannerService;

        public BannerViewComponent(IModuleManager moduleManager, ILogger logger, IBannerService bannerService) : base(moduleManager)
        {
            _logger = logger;
            _bannerService = bannerService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string key="default")
        {
            try
            {
                var banners=await _bannerService.GetBannersByKey(key);
                return View(banners);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Banner loading error.", e);
                throw e;
            }

        }

        public override string DisplayName { get; } = "Banner";
        public override bool IsVisibleOnUI { get; } = true;
    }
}
