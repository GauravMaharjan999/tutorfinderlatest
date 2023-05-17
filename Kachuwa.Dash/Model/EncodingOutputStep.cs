using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("EncodingOutputStep")]
    public class EncodingOutputStep
    {
        [Key]
        public int EncodingOutputStepId { get; set; }
        public int EncodingOutputFormatId { get; set; }

        [Required(ErrorMessage = "EncodingOutputStep.Name.Required")]
        public string Name { get; set; }

        [Required(ErrorMessage = "EncodingOutputStep.Subtitle.Required")]
        public string Subtitle { get; set; }

        [Required(ErrorMessage = "EncodingOutputStep.Commands.Required")]
        public string Commands { get; set; }
        public int Order { get; set; }

        public string Flag { get; set; }

        public bool IsRequiredForDash { get; set; }
        public bool WaitPreviousStep { get; set; }
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