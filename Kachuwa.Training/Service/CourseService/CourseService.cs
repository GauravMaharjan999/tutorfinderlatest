using Dapper;
using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public class CourseService : ICourseService
    {
       
        public CrudService<Course> CourseCrudService { get; set; } = new CrudService<Course>();

        public async Task<IEnumerable<Course>> CourseNotMappedOnCourseTutorList()
        {

            try
            {
                string sql = @"select C.* from TrainingCourse C left join TrainingCourseTutor CT on C.Id = CT.CourseId where CT.CourseId is NULL and C.IsActive = @isActive ";

                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<Course>(sql, new { @isActive = true });
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<CourseDetailViewModel> GetCourseandTutorDetailsByCourseId(int CourseId)
        {

            try
            {
                CourseDetailViewModel courseDetailViewModel = new CourseDetailViewModel();

                string sql = @"
                                            Select * from TrainingCourse Where Id = @courseId;

                                           select TT.* from TrainingTutor TT inner Join TrainingCourseTutor TCT on TCT.TutorId = TT.Id where TCT.CourseId = @courseId;
                                            select * from TrainingCourseSyllabus where CourseId = @courseId;

                                            ";
                var dbfactory = DbFactoryProvider.GetFactory();
                using (var db = (DbConnection)dbfactory.GetConnection())
                {
                    await db.OpenAsync();
                    var result = await db.QueryMultipleAsync(sql,new { @courseId = CourseId });
                    courseDetailViewModel.Course  = await result.ReadFirstOrDefaultAsync<Course>();
                    courseDetailViewModel.TrainingTutor =  await result.ReadFirstOrDefaultAsync<TrainingTutor>();
                    courseDetailViewModel.CourseSyllabus = await result.ReadAsync<CourseSyllabus>();
                    return courseDetailViewModel;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
    }
}
