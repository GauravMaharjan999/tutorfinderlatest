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
   public class PaymentLogService : IPaymentLogService
    {
        public CrudService<PaymentLog> PaymentLogCrudService { get; set; } = new CrudService<PaymentLog>();

        public async Task<IEnumerable<PaymentLogViewModel>> AllPaymentList(int pq_curpage, int pq_rpp)
        {

            try
            {
                string sql = @" select  (select Count(1) from PaymentLog) as RowTotal ,IU.Email as UserEmail,TC.Name as CourseName,E.CourseFee,PL.* from  PaymentLog Pl
                                            left join 
                                            Enroll E
                                            on E.id = Pl.EnrollId
                                            left Join TrainingCourse TC
                                            on TC.Id = Pl.CourseId 
                                            left join IdentityUser IU 
                                            on IU.Id = PL.UserId
                                            order by Pl.Id
                                            offset ((@PageNumber-1)*@RowPerPage) rows fetch next @RowPerPage rows only
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<PaymentLogViewModel>(sql, new { @PageNumber = pq_curpage, @RowPerPage = pq_rpp });
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<PaymentLogViewModel> PaymentDetailByPaymentId(int paymentLogId)
        {

            try
            {
                string sql = @" select IU.Email as UserEmail,TC.Name as CourseName,E.CourseFee,PL.* from  PaymentLog Pl
                                            left join 
                                            Enroll E
                                            on E.id = Pl.EnrollId
                                            left Join TrainingCourse TC
                                            on TC.Id = Pl.CourseId 
                                            left join IdentityUser IU 
                                            on IU.Id = PL.UserId
                                           where Pl.Id = @paymentId
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryFirstAsync<PaymentLogViewModel>(sql, new { @paymentId = paymentLogId });
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<IEnumerable<PaymentLogViewModel>> PaymentListbyUserId(int userId,int pq_curpage, int pq_rpp)
        {

            try
            {
                string sql = @" select  (select Count(1) from PaymentLog where UserId = @userId) as RowTotal ,IU.Email as UserEmail,TC.Name as CourseName,E.CourseFee,PL.* from  PaymentLog Pl
                                            left join 
                                            Enroll E
                                            on E.id = Pl.EnrollId
                                            left Join TrainingCourse TC
                                            on TC.Id = Pl.CourseId 
                                            left join IdentityUser IU 
                                            on IU.Id = PL.UserId
                                            where Pl.UserId= @userId
                                            order by Pl.Id
                                            offset ((@PageNumber-1)*@RowPerPage) rows fetch next @RowPerPage rows only
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<PaymentLogViewModel>(sql, new { @userId= userId, @PageNumber = pq_curpage, @RowPerPage = pq_rpp });
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
