using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingTutor")]
   public  class TrainingTutor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Email { get; set; }
        public string ProfileImagePath { get; set; }
        public string MobileNo { get; set; }
        public string ShortBio { get; set; }
        public string Bio { get; set; }
        public string Address { get; set; }
        public string Experience { get; set; }
        public string Expertise { get; set; }
        public string LinkedInLink { get; set; }
        public bool IsShowOnHomePage { get; set; }

        [IgnoreUpdate]
        public int AddedBy { get; set; }

        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [IgnoreInsert]
        public int ModifiedBy { get; set; }

        [IgnoreInsert]
        public DateTime? ModifiedOn { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [IgnoreInsert]
        public int DeletedBy { get; set; }
        [IgnoreInsert]
        public DateTime? DeletedOn { get; set; }

        [IgnoreAll]
        public IFormFile ProfileImageAttachment { get; set; }

        [IgnoreAll]
        public IFormFile EditProfileImageAttachment { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }

        public int IdentityUserId { get; set; }
    }
}
