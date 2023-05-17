using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseRecommend.CourseRecommemdation;
using CourseRecommendation;
using Kachuwa.Identity.Extensions;
using CourseRecommend.CourseRecommemdation.DataStructures;

namespace Kachuwa.LiveTrainingApp.Components.Recommendation
{
    [ViewComponent(Name = "Recommendation")]
    public class RecommendationViewComponent : KachuwaViewComponent
    {
        private readonly ILogger _logger;
        private readonly ICourseService _courseService;
        public RecommendationViewComponent(ILogger logger,ICourseService courseService)
        {
            _logger = logger;
            _courseService = courseService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var CurrentUserId = Convert.ToInt32(User.Identity.GetIdentityUserId());
                Recom val = CourseRecom.ClassRec(CurrentUserId);
                var course0 = val.courseId[0];
                var course1 = val.courseId[1];
                var course2 = val.courseId[2];
                var data = await _courseService.CourseCrudService.GetListAsync("Where IsActive = @isActive AND Id= "+course0+" OR Id= "+course1+" OR Id= "+course2+"" , new { @isActive = true });
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
