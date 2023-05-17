using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingCourseTiming")]
   public  class CourseTiming
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int CourseId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Time { get; set; }

        public string Description { get; set; }
        [IgnoreUpdate]
        public int AddedBy { get; set; }

        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        public bool IsActive { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }
    }
    public class CourseTimingViewModel : CourseTiming
    {
        public string CourseName { get; set; }
    }
}
