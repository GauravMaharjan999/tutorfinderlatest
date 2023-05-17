﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Data;
using Kachuwa.Identity.Models;
using Kachuwa.Identity.ViewModels;

namespace Kachuwa.Identity.Service
{
    public interface IAppUserService
    {
        CrudService<AppUser> AppUserCrudService { get; set; }
        Task<UserStatus> SaveNewUserAsync(UserViewModel model);
        Task<UserStatus> UpdateUserAsync(UserEditViewModel model);
        Task<bool> DeleteUserAsync(int appUserId);
        Task<bool> AssignRolesAsync(UserRolesViewModel roles);

        Task<UserEditViewModel> GetAsync(int appuserId);
        Task<bool> UpdateProfilePicture(long appUserId, string imagePath);
        Task<bool> UpdateUserDeviceId(long appUserId, string deviceId);

        Task<int> GetNewUserStatusAsync(int showRecords);
        Task<bool> UpdateReferalCode(string code,int userId);
    }
}