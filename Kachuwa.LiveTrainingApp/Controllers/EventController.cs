using Kachuwa.Identity.Extensions;
using Kachuwa.Identity.Service;
using Kachuwa.Log;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.KLiveApp.Controllers
{
    public class EventController : Controller
    {
        private readonly IEventService _eventService;
        private readonly ILogger _logger;
        private readonly IIdentityUserService _identityUserService;
       
        public EventController(IEventService eventService ,ILogger logger, IIdentityUserService identityUserService)
        {
            _eventService = eventService;
            _logger = logger;
            _identityUserService = identityUserService;
        }

        [Route("events/page/{pageNo?}")]
        [Route("events")]
        public async Task<IActionResult> Index([FromRoute]int pageNo = 1, [FromQuery]string query = "")
        {
            return View();
        }

        [Route("event/{id}")]
        public async Task<IActionResult> Detail([FromRoute]int id)
        {
            var data = await _eventService.EventCrudService.GetAsync(id);
            return View(data);
        }
        [Route("CheckEventRegister")]
        public async Task<IActionResult> CheckEventRegister([FromQuery]int UserId, [FromQuery] int EventId)
        {
            var registerdetail = (await _eventService.EventRegisterCrudService.GetListAsync("Where UserId=@userId and EventId = @eventId", new { @userId = UserId, @eventId = EventId })).LastOrDefault();
            if (registerdetail == null)
            {
                //no register in event by user
                var path = "/event/register/" + EventId;
                return base.Redirect(path);
            }
            else
            {
                //already registered in event by the user
                return View();
            }
        }

        [HttpGet]
        [Route("event/register/{eventid}")]
        public async Task<IActionResult> EventRegister([FromRoute]int eventid)

        {
            EventRegister model = new EventRegister();
            var data = await _eventService.EventCrudService.GetAsync(eventid);
            ViewBag.EventDetail = data;
            var UserId = Convert.ToInt32(User.Identity.GetIdentityUserId());
            var UserDetails  = await _identityUserService.UserService.GetAsync("Where Id=@identityUserId", new { @identityUserId = UserId });
            model.UserId = UserId;
            model.EmailAddress = UserDetails.Email.ToLower();
            return View(model);
        }

        [HttpPost]
        [Route("event/register/{eventid}")]
        public async Task<IActionResult> EventRegister(EventRegister model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                        var status = await _eventService.EventRegisterCrudService.InsertAsync<int>(model);
                        return RedirectToAction("EventRegisterSuccess");
                }
                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    var data = await _eventService.EventCrudService.GetAsync(model.EventId);
                    ViewBag.EventDetail = data;
                    return View(model);
                }
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                var data = await _eventService.EventCrudService.GetAsync(model.EventId);
                ViewBag.EventDetail = data;
                return View(model);
            }
        }

        [Route("event/register/registersuccess")]
        public async Task<IActionResult> EventRegisterSuccess()
        {
            return View();
        }


    }
}