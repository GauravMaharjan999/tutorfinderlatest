using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
    public interface ICourseService
    {
        CrudService<Course> CourseCrudService { get; set; }
        Task<CourseDetailViewModel> GetCourseandTutorDetailsByCourseId(int CourseId);
        Task<IEnumerable<Course>> CourseNotMappedOnCourseTutorList();
    }
}
