using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
    public interface ICourseTimingService
    {
        CrudService<CourseTiming> CourseTimingCrudService { get; set; }
        Task<IEnumerable<CourseTimingViewModel>> CourseTimingList(int pq_curpage, int pq_rpp);
        Task<IEnumerable<CourseTimingViewModel>> CourseTimingListforStreamByTutorIdentityUserId(int IdentityUserId);

    }
}
