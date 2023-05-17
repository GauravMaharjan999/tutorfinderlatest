using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("Event")]
    public class Event
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public string Address { get; set; }


        [IgnoreUpdate]
        public int AddedBy { get; set; }

        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [IgnoreInsert]
        public int ModifiedBy { get; set; }

        [IgnoreInsert]
        public DateTime? ModifiedOn { get; set; }

        [IgnoreInsert]
        public int DeletedBy { get; set; }

        [IgnoreInsert]
        public DateTime? DeletedOn { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public string ProfileImagePath { get; set; }

        [IgnoreAll]
        public IFormFile ProfileImageAttachment { get; set; }

        [IgnoreAll]
        public IFormFile EditProfileImageAttachment { get; set; }

        public bool IsFree { get; set; }

        public decimal Price { get; set; }

        public string ContactNumber { get; set; }

        public bool IsLoginRequired { get; set; }
    }
}
