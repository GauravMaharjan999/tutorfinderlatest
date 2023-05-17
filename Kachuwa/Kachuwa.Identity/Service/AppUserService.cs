﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Kachuwa.Data;
using Kachuwa.Data.Extension;
using Kachuwa.Extensions;
using Kachuwa.Identity.Models;
using Kachuwa.Identity.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace Kachuwa.Identity.Service
{
    public class AppUserService : IAppUserService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IIdentityUserService _identityUserService;
        private readonly IIdentityRoleService _identityRoleService;

        public AppUserService(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager, IIdentityUserService identityUserService, IIdentityRoleService identityRoleService)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _identityUserService = identityUserService;
            _identityRoleService = identityRoleService;
        }
        public CrudService<AppUser> AppUserCrudService { get; set; } = new CrudService<AppUser>();

        public async Task<UserStatus> SaveNewUserAsync(UserViewModel model)
        {
            var status = new UserStatus();
            try
            {
                if (model.AppUserId == 0)
                {
                    var user = new IdentityUser { UserName =model.UserName??model.Email, Email = model.Email };
                    var existingUser = await _userManager.FindByEmailAsync(model.Email);
                    if (existingUser != null)
                    {
                        status.HasError = true;
                        status.Message = "Email is already registered";
                        return status;
                    }
                    var dbFactory = DbFactoryProvider.GetFactory();
                    using (var db = (DbConnection)dbFactory.GetConnection())
                    {
                        await db.OpenAsync();
                        using (var dbTran = db.BeginTransaction())
                        {
                            try
                            {
                                var stat = await _userManager.CreateAsync(user, model.Password);
                                if (stat.Succeeded)
                                {
                                    var identityUser = await _userManager.FindByEmailAsync(model.Email);
                                    var appUser = model.To<AppUser>();
                                    appUser.AutoFill();
                                    appUser.IdentityUserId = identityUser.Id;
                                    appUser.IsActive = true;
                                    var id = await AppUserCrudService.InsertAsync<long>(db, appUser, dbTran, 10);

                                    var roleIds = model.UserRoles.Where(z => z.IsSelected == true).Select(x => (int)x.RoleId).ToArray();
                                    if (roleIds != null && roleIds.Any())
                                        await _identityUserService.AddUserRoles(roleIds, identityUser.Id);
                                    else
                                        await _identityUserService.AddUserRoles(new int[] { 3 }, identityUser.Id);//customer role

                                    dbTran.Commit();
                                    status.HasError = false;
                                    status.Message = id.ToString();
                                    return status;
                                }
                                status.HasError = true;
                                status.Message = string.Join(",", stat.Errors);
                                return status;
                            }
                            catch (Exception e)
                            {

                                dbTran.Rollback();
                                var identityUser = await _userManager.FindByEmailAsync(model.Email);
                                await _userManager.DeleteAsync(identityUser);
                                status.HasError = true;
                                status.Message = e.Message.ToString();
                                return status;
                            }
                        }





                    }
                    //status.HasError = true;
                    //status.Message = string.Join(",", result.Errors);
                    //return status;
                }
                else
                {
                    var appUser = model.To<AppUser>();
                    appUser.AutoFill();
                    var id = await AppUserCrudService.UpdateAsync(appUser);
                    //await _identityUserService.DeleteUserRoles(appUser.IdentityUserId );
                    var roleIds = model.UserRoles.Where(z => z.IsSelected == true).Select(x => (int)x.RoleId).ToArray();
                    await _identityUserService.AddUserRoles(roleIds, appUser.IdentityUserId);
                    status.HasError = false;
                    status.Message = "Updated Succesfully";
                    return status;
                }
            }
            catch (Exception e)
            {
                status.HasError = true;
                status.Message = e.Message;
                return status;
            }
        }

        public async Task<UserStatus> UpdateUserAsync(UserEditViewModel model)
        {
            var status = new UserStatus();
            try
            {
                if (model.AppUserId != 0)
                {
                    var appUser = model.To<AppUser>();
                    appUser.AutoFill();
                    var id = await AppUserCrudService.UpdateAsync(appUser);
                    // await _identityUserService.DeleteUserRoles(appUser.IdentityUserId);
                    var roleIds = model.UserRoles.Where(z => z.IsSelected == true).Select(x => (int)x.RoleId).ToArray();
                    await _identityUserService.AddUserRoles(roleIds, appUser.IdentityUserId);

                    status.HasError = false;
                    status.Message = "Updated Succesfully";
                    return status;
                }
                status.HasError = true;
                status.Message = "Invalid AppUserId";
                return status;
            }
            catch (Exception e)
            {
                status.HasError = true;
                status.Message = e.Message;
                return status;
            }
        }

        public async Task<bool> DeleteUserAsync(int appUserId)
        {
            var user = await AppUserCrudService.GetAsync(appUserId);
            var identityUser = await _userManager.FindByEmailAsync(user.Email);
            if (identityUser != null)
                await _userManager.DeleteAsync(identityUser);

            user.IsDeleted = true;
            user.IsActive = false;
            await AppUserCrudService.UpdateAsync(user);
            return true;
        }

        public async Task<bool> AssignRolesAsync(UserRolesViewModel userRoles)
        {
            await _identityUserService.AddUserRoles(userRoles.Roles.Select(e => (int)e.Id).ToArray(), userRoles.IdentityUserId);
            return true;
        }

        public async Task<UserEditViewModel> GetAsync(int appuserId)
        {
            var user = await AppUserCrudService.GetAsync(appuserId);
            var userViewModel = user.To<UserEditViewModel>();
            userViewModel.UserRoleIds = await _identityUserService.GetUserRoles(user.IdentityUserId);
            return userViewModel;
        }

        public async Task<bool> UpdateProfilePicture(long appUserId, string imagePath)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                await db.ExecuteAsync("Update dbo.AppUser Set ProfilePicture=@Image where AppUserId=@appUserId",
                    new { appUserId, Image = imagePath });
                return true;
            }
        }

        public async Task<bool> UpdateUserDeviceId(long appUserId, string deviceId)
        {
            //TODO:: update table fields
            throw new NotImplementedException();
        }

        public async Task<int> GetNewUserStatusAsync(int showRecords)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                return await db.ExecuteScalarAsync<int>(@"if(@showRecords=1)
                     select count(1) from appuser where cast(addedon as date)=cast(GETUTCDATE() as date)
                    else if(@showRecords=2)   
                    select count(1) from appuser where cast(addedon as date)= dateAdd(day, -1,cast(GETUTCDATE() as date))
                    else if(@showRecords=3 )
                    select count(1) from appuser where cast(addedon as date) between  DATEADD(DAY, 1 - DATEPART(WEEKDAY, GETUTCDATE()), CAST(GETUTCDATE() AS DATE)) and  DATEADD(DAY, 7 - DATEPART(WEEKDAY, GETUTCDATE()), CAST(GETUTCDATE() AS DATE)) 

", new { showRecords });
            }
        }
        public async Task<bool> UpdateReferalCode(string code, int userId)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                await db.ExecuteAsync("update AppUser set ReferredUserCode = @Code where IdentityUserId =@UserId ", new { @Code = code, @UserId = userId });
                return true;
            }
        }
    }
}
