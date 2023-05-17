using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Components.AllEventsPage
{
    [ViewComponent(Name = "AllEventsPage")]
    public class AllEventsPageViewComponent : KachuwaViewComponent
    {
        private readonly ILogger _logger;
        private readonly IEventService _eventService;

        public AllEventsPageViewComponent(ILogger logger , IEventService eventService)
        {
            _logger = logger;
            _eventService = eventService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var data = await _eventService.EventCrudService.GetListAsync("Where IsActive = @isActive and IsDeleted=@isDeleted", new { @isActive = true,@isDeleted = false });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Event loading error.", e);
                throw e;
            }

        }
        public override string DisplayName { get; } = "AllEvents";
        public override bool IsVisibleOnUI { get; } = true;
    }
}
