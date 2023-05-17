using Kachuwa.Data.Crud.Attribute;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("TrainingCourseCategory")]
   public  class TrainingCourseCategory
    {
        [Key]
        public int Id { get; set; }
        public int ParentId { get; set; }

        [Required]
        public string Name { get; set; }

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
        public int RowTotal { get; set; }
    }
}
