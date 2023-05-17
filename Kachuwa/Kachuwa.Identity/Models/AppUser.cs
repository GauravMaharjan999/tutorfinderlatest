using System;
using Kachuwa.Data.Crud.Attribute;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kachuwa.Identity.Models
{
    [Table("AppUser")]
    public class AppUser
    {
        [Key]
        public long AppUserId { get; set; }

        [IgnoreUpdate]
        public long IdentityUserId { get; set; }

        [Required(ErrorMessage = "User.FirstName.Required")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "User.LastName.Required")]
        public string LastName { get; set; }

        public string Bio { get; set; }

        [Required(ErrorMessage = "User.Email.Required")]
        [IgnoreUpdate]
        [EmailAddress(ErrorMessage = "User.InvalidEmail")]
        public string Email { get; set; }

        public string Address { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        public string DOB { get; set; }
        public string ProfilePicture { get; set; }
        public string Gender { get; set; }
        //public int CompanyId { get; set; }

        public bool IsActive { get; set; }
        [IgnoreInsert]
        [AutoFill(false)]
        public bool IsDeleted { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }

    }
}