using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("EncodingOutputFormat")]
    public class EncodingOutputFormat
    {
        [Key]
        public int EncodingOutputFormatId { get; set; }
        [Required(ErrorMessage = "EncodingOutputFormat.Name.Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "EncodingOutputFormat.Display.Required")]
        public string Display { get; set; }
        [Required(ErrorMessage = "EncodingOutputFormat.Subtitle.Required")]
        public string Subtitle { get; set; }
        public int ParentId { get; set; }
        [IgnoreAll]
        public int TotalSteps { get; set; }
        public bool IsActive { get; set; }

        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }

        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreInsert]
        public string UpDatedBy { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreInsert]
        public DateTime UpdatedOn { get; set; }

        [IgnoreInsert]
        [AutoFill(false)]
        public bool IsDeleted { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }
    }
}