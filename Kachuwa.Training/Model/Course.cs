using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingCourse")]
   public  class Course
    {
        [Key]
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; }
        public string ProfileImagePath { get; set; }
        public string CoverImagePath { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }
        public string Tags { get; set; }
        public int Duration { get; set; }
        public string PreRequisites { get; set; }
        [Required]
        public string CourseLevel { get; set; }
        public bool IsShowOnHomePage { get; set; }

        public decimal CourseFee { get; set; }
        public string IntroVideoPath { get; set; }

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
        public IFormFile CoverImageAttachment { get; set; }

        [IgnoreAll]
        public IFormFile EditCoverImageAttachment { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }
    }

    public class CourseDetailViewModel 
    {
        public Course Course { get; set; }

        public TrainingTutor TrainingTutor { get; set; }
        public IEnumerable<CourseSyllabus> CourseSyllabus { get; set; }
        public CourseUserReview CourseUserReview { get; set; }


    }
}
