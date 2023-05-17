using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("Enroll")]
    public class Enroll
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please Login")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Please Select Course")]
        public int CourseId { get; set; }
        public int? Rate { get; set; }
       
        public decimal CourseFee { get; set; }
     
        public int CourseTimingId { get; set; }
        [Required(ErrorMessage = "Please Select Course Timing")]
        public string CourseTime { get; set; }
        [Required(ErrorMessage = "Please Select Start Date")]
        public DateTime CourseStartDate { get; set; }
        [Required(ErrorMessage = "Please Select End Date")]

        public DateTime CourseEndDate { get; set; }
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        [IgnoreUpdate]
        public int AddedBy { get; set; }
        public string PaymentMethod { get; set; }
        public bool IsPaid { get; set; }
        public bool IsPaidVerified { get; set; }
    }
    public class EnrollViewModel 
    {
        public IEnumerable<CourseTiming> courseTimimg { get; set; }
        public  CourseDetailViewModel courseDetailViewModel { get; set; }
    }
    public class EnrollViewModelForUser : Enroll
    {
        public string  CourseName { get; set; }
        public string CourseProfileImagePath { get; set; }
        public int TotalEnrollCourseNumberByUser { get; set; }
    }
}
