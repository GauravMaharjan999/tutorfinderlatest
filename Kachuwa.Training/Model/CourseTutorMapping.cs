using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingCourseTutor")]
    public  class CourseTutorMapping
    {
        [Key]
        public int Id { get; set; }
        public int CourseId { get; set; }
        public int TutorId { get; set; }
    }
    public class CourseTutorMappingViewModel : CourseTutorMapping
    {
        public string CourseName { get; set; }
        public string TutorName { get; set; }
        [IgnoreAll]
        public string RowTotal { get; set; }
    }
}
