using Dapper;
using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public  class CourseSyllabusService : ICourseSyllabusService
    {
        public CrudService<CourseSyllabus> CourseSyllabusCrudService { get; set; } = new CrudService<CourseSyllabus>();

        public async Task<IEnumerable<CourseSyllabusViewModel>> CourseSyllabuslist(int pq_curpage, int pq_rpp)
        {

            try
            {
                string sql = @"  select (select Count(1) from TrainingCourseSyllabus) as RowTotal ,C.Name as CourseName,cs.*   from TrainingCourseSyllabus cs
                                        left join  TrainingCourse C on C.Id = cs.CourseId
                                        order by cs.CourseId
                                        offset ((@PageNumber-1)*@RowPerPage) rows fetch next @RowPerPage rows only
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<CourseSyllabusViewModel>(sql, new { @PageNumber = pq_curpage, @RowPerPage = pq_rpp });
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
