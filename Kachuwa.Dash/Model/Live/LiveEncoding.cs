using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Live
{
    [Table("LiveEncoding")]
    public class LiveEncoding
    {
        [Key]
        public int LiveEncodingId { get; set; }
        [Required(ErrorMessage = "LiveEncoding.Name.Required")]
        public string Name { get; set; }
        [Required(ErrorMessage = "LiveEncoding.Display.Required")]
        public string Display { get; set; }
        public string Remarks { get; set; }
        public string InputCommand { get; set; }
        public string OtherCommand { get; set; }
        public string OutputCommand { get; set; }
        public bool Support2K { get; set; }
        public bool Support4K { get; set; }
        public bool Support8K { get; set; }
        public bool IsDefault { get; set; }
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

        public bool IsHls { get; set; }
    }

    public class LiveEncodingViewModel : LiveEncoding
    {
        public string LiveEncodingFormatIds { get; set; }
    }
}