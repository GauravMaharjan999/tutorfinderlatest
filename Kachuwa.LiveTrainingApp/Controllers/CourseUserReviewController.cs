using Dapper;
using Kachuwa.Training.Model;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System;
using System.Data.SqlClient;
using MXTires.Microdata.Core.Intangible;
using Kachuwa.Data;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.LiveTrainingApp.ViewModels.CourseUserReview;
using Newtonsoft.Json;


namespace Kachuwa.LiveTrainingApp.Controllers
{
    public class CourseUserReviewController : Controller
    {
       [HttpPost]
        public IActionResult Insert(CourseUserReview courseUserReview)
        {
            using (IDbConnection db = new SqlConnection(@"Server=LAPTOP-3P9NJVAL\\SQLEXPRESS02;Database=Training;Persist Security Info=False;Integrated Security=SSPI;MultipleActiveResultSets=true;Connection Timeout=30;"))
            {
                string insertQuery = @"INSERT INTO [dbo].[CourseUserReview]([CourseId], [UserId], [Rating], [ReviewContent]) VALUES (@CourseId, @UserId, @Rating, @ReviewContent)";

                var result = db.Execute(insertQuery,courseUserReview); 
            }

            return Redirect("/course/"+courseUserReview.CourseId);
        }



        [HttpGet]
        public async Task<IActionResult> GetRating(int courseId)
        {
            
            using (IDbConnection db = new SqlConnection(@"Server=LAPTOP-3P9NJVAL\\SQLEXPRESS02;Database=Training;Persist Security Info=False;Integrated Security=SSPI;MultipleActiveResultSets=true;Connection Timeout=30;"))
            {
                
                try
                {

                    string getQuery = @"SELECT [Rating],[ReviewContent] FROM [dbo].[CourseUserReview] where CourseId =@courseId ORDER BY Id desc";

                    var dbFactory = DbFactoryProvider.GetFactory();
                    using (var dbcon = (SqlConnection)dbFactory.GetConnection())
                    {
                        var list = await dbcon.QueryAsync<RatingViewModel>(getQuery, new { @courseId = courseId });
                        
                        return Json(list);

                    }
                }
                catch (Exception ex) {
                    throw ex;
                }
            }

        }


    }
}
