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
   public class CourseTimingService : ICourseTimingService
    {
        public CrudService<CourseTiming> CourseTimingCrudService { get; set; } = new CrudService<CourseTiming>();

        public async Task<IEnumerable<CourseTimingViewModel>> CourseTimingList(int pq_curpage, int pq_rpp)
        {

            try
            {
                string sql = @"  select (select Count(1) from TrainingCourseTiming) as RowTotal ,C.Name as CourseName,ct.*   from TrainingCourseTiming ct
                                        left join  TrainingCourse C on C.Id = ct.CourseId
                                        order by ct.CourseId
                                        offset ((@PageNumber-1)*@RowPerPage) rows fetch next @RowPerPage rows only
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<CourseTimingViewModel>(sql, new { @PageNumber = pq_curpage, @RowPerPage = pq_rpp });
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<IEnumerable<CourseTimingViewModel>> CourseTimingListforStreamByTutorIdentityUserId(int IdentityUserId)
        {

            try
            {
                string sql = @"  select TCT.*,TC.Name as CourseName from TrainingCourseTiming TCT left join 
                                    TrainingCourse TC on TCT.CourseId = TC.Id
                                    left join TrainingCourseTutor Tcct on Tcct.CourseId = TCT.CourseId
                                    left join TrainingTutor TT on TT.Id = Tcct.TutorId
                                    where TT.IdentityUserId = @identityuserId and GETDATE() between TCT.FromDate and TCT.ToDate
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<CourseTimingViewModel>(sql, new { identityuserId = IdentityUserId });
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
