using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Components.EventSliderHomePage
{
    [ViewComponent(Name = "EventSliderHomePage")]
    public class EventSliderHomePageViewComponent : KachuwaViewComponent
    {
        private readonly ILogger _logger;
        private readonly IEventService _eventService;

        public EventSliderHomePageViewComponent(ILogger logger,IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var data = await _eventService.EventCrudService.GetListAsync("Where IsActive = @isActive and FromDate > @fromDate", new { @isActive = true ,@fromDate =DateTime.Now.Date });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Event loading error.", e);
                throw e;
            }

        }
        public override string DisplayName { get; } = "Event";
        public override bool IsVisibleOnUI { get; } = true;
    }
}
