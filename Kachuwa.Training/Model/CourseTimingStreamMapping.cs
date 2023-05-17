using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("CourseTimingStreamMapping")]
   public  class CourseTimingStreamMapping
    {
        [Key]
        public int Id { get; set; }
        public int ReferenceId { get; set; }
        public int StreamId { get; set; }
        public string Type { get; set; }
    }
}
