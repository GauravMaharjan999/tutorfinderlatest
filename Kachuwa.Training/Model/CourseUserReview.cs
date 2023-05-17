using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("CourseUserReview")]
    public class CourseUserReview
    {
        [Key]
        public int Id { get; set; }
        
        public int CourseId { get; set; }
        public int UserId { get; set; }

        public int Rating { get; set; }
        public string ReviewTitle { get; set; }
        public string ReviewContent { get; set; }
    }
}
