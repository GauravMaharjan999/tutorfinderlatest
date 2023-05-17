using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingCourseSyllabus")]
    public  class CourseSyllabus
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        public int Hours { get; set; }
        public bool IsFree { get; set; }
        public string VideoPath { get; set; }
        public bool IsActive { get; set; }


        [IgnoreUpdate]
        public int AddedBy { get; set; }

        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [IgnoreInsert]
        public int ModifiedBy { get; set; }

        [IgnoreInsert]
        public DateTime? ModifiedOn { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }

    }
    public class CourseSyllabusViewModel : CourseSyllabus
    {
        public string CourseName { get; set; }
    }
}
