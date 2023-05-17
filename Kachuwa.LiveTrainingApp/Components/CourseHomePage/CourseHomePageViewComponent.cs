using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Components.CourseHomePage
{
    [ViewComponent(Name = "CourseHomePage")]
    public class CourseHomePageViewComponent : KachuwaViewComponent
    {
        private readonly ILogger _logger;
        private readonly ICourseService _courseService ;

        public CourseHomePageViewComponent(ILogger logger,ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;

        }

       
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var data = await _courseService.CourseCrudService.GetListAsync("Where IsActive = @isActive and  IsShowOnHomePage =@isShow ", new { @isActive = true, @isShow = true });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Training Course loading error.", e);
                throw e;
            }

        }
        public override string DisplayName { get; } = "Course";
        public override bool IsVisibleOnUI { get; } = true;
    }
}
