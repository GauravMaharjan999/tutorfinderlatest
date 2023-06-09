using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Kachuwa.Data.Crud.Attribute;
using Kachuwa.Identity.Models;
using Microsoft.AspNetCore.Http;

namespace Kachuwa.Identity.ViewModels
{
    public class UserViewModel : AppUser
    {
        [Required]
        public string Password { get; set; }
        public string UserName { get; set; }
        // public List<int> UserRoleIds { get; set; }
        public List<UserRolesSelected> UserRoles { get; set; }
        [IgnoreAll]
        public IFormFile ImageFile { get; set; }

    }
    public class UserRolesSelected
    {
        public long RoleId { get; set; }
        public string Name { get; set; }
        public bool IsSelected { get; set; }
    }
}