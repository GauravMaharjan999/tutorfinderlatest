using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public interface ICourseTutorMappingService
    {
        CrudService<CourseTutorMapping> CourseTutorMappingCrudService { get; set; }
        Task<IEnumerable<CourseTutorMappingViewModel>> CourseTutorMappingAllList(int pq_curpage, int pq_rpp);
    }
}
