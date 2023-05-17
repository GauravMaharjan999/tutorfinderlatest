using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public interface ICourseSyllabusService
    {
        CrudService<CourseSyllabus> CourseSyllabusCrudService { get; set; }
        Task<IEnumerable<CourseSyllabusViewModel>> CourseSyllabuslist(int pq_curpage, int pq_rpp);

    }
}
