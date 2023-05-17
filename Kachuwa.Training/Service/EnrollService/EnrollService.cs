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
    public class EnrollService : IEnrollService
    {
        public CrudService<Enroll> EnrollCrudService { get; set; } = new CrudService<Enroll>();

        public async Task<IEnumerable<EnrollViewModelForUser>> EnrollListByUserId(int userId)
        {

            try
            {
                string sql = @"  Select (Select Count(1) from Enroll where UserId =@userId ) as TotalEnrollCourseNumberByUser,E.*,TC.Name as CourseName,TC.ProfileImagePath as CourseProfileImagePath 
                                    from Enroll E
                                    left join TrainingCourse TC on TC.Id = E.CourseId
                                    where E.UserId = @userId
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<EnrollViewModelForUser>(sql, new { @userId = userId });
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
