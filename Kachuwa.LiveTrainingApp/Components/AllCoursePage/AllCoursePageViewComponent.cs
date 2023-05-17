using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kachuwa.LiveTrainingApp.Components.AllCoursePage
{
    [ViewComponent(Name = "AllCoursePage")]
    public class AllCoursePageViewComponent : KachuwaViewComponent
    {
        private readonly ILogger _logger;
        private readonly ICourseService _courseService;
        public AllCoursePageViewComponent(ILogger logger,ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var data = await _courseService.CourseCrudService.GetListAsync("Where IsActive = @isActive ", new { @isActive = true });
                return View(data);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => "Training Course loading error.", e);
                throw e;
            }

        }
        public override string DisplayName { get; } = "AllCourse";
        public override bool IsVisibleOnUI { get; } = true;


    }
}
