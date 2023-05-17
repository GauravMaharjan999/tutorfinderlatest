using Dapper;
using Kachuwa.Data;
using Kachuwa.Identity.Models;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public  class TrainingTutorService :ITrainingTutorService
    {
        public CrudService<TrainingTutor> TrainingTutorCrudService { get; set; } = new CrudService<TrainingTutor>();

        public async Task<IEnumerable<IdentityUser>> TutorIdentityListNotMappedOnTutor()
        {

            try
            {
                string sql = @" select IU.* from IdentityUser IU
                                    left join IdentityUserRole IUR
                                    on IUR.UserId = IU.Id
                                    left join TrainingTutor TT on TT.IdentityUserId = IU.Id
                                    where IUR.RoleId = @roleId and TT.IdentityUserId is NUll
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<IdentityUser>(sql,new { @roleId = 5 });   //5 means tutor
                    return list;

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<IEnumerable<IdentityUser>> TutorIdentityList(int identityUserId)
        {

            try
            {
                string sql = @" select IU.* from IdentityUser IU
                                    left join IdentityUserRole IUR
                                    on IUR.UserId = IU.Id
                                    left join TrainingTutor TT on TT.IdentityUserId = IU.Id
                                    where IUR.RoleId = @roleId and TT.IdentityUserId is NUll or TT.IdentityUserId = @identityUserId
                                        ";


                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (SqlConnection)dbFactory.GetConnection())
                {
                    var list = await db.QueryAsync<IdentityUser>(sql, new { @roleId = 5 , @identityUserId = identityUserId });   //5 means tutor
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
