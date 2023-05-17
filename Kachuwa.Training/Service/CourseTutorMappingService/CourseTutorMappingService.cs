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
    public class CourseTutorMappingService : ICourseTutorMappingService
    {
        public CrudService<CourseTutorMapping> CourseTutorMappingCrudService { get; set; } = new CrudService<CourseTutorMapping>();

        public async Task<IEnumerable<CourseTutorMappingViewModel>> CourseTutorMappingAllList(int pq_curpage, int pq_rpp)
        {

            try
            {
                string sql = @"  select (select Count(1) from TrainingCourseTutor) as RowTotal ,TC.Name as CourseName,TT.Name as TutorName,TCT.* from TrainingCourseTutor TCT
                                                                left join TrainingCourse TC on TC.Id = TCT.CourseId
                                                                left join TrainingTutor TT on TT.Id = TCT.TutorId
                                                                order by TCT.Id
                                                                offset ((@PageNumber-1)*@RowPerPage) rows fetch next @RowPerPage rows only";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<CourseTutorMappingViewModel>(sql, new { @PageNumber = pq_curpage, @RowPerPage = pq_rpp });
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
