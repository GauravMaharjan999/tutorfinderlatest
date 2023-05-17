using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Kachuwa.Data;
using Kachuwa.Identity.Models;

namespace Kachuwa.Identity.Service
{
    public interface IIdentityUserService
    {
        CrudService<IdentityUser> UserService { get; set; }
        CrudService<IdentityLogin> LoginService { get; set; }
        CrudService<IdentityUserClaim> ClaimService { get; set; }
        CrudService<IdentityUserRole> UserRoleService { get; set; }

        Task<bool> AddUserRoles(int[] roleIds,long userId);
        Task<bool> AddUserRoles(int[] roleIds, long userId,IDbConnection db,IDbTransaction tran);
        Task<List<int>> GetUserRoles(long userIdentityUserId);
        Task DeleteUserRoles(long identityUserId);
    }
}