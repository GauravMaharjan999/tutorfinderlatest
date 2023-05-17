using Kachuwa.Data.Crud.Attribute;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kachuwa.Dash.Model
{
    [Table("DashVideoSetting")]
    public class DashVideoSetting
    {
        [Key]
        public int DashVideoSettingId { get; set; }
      
        [Range(1,999999,ErrorMessage = "DashVideoSetting.DefaultEncodingOutputFormatId.Required")]
        public int DefaultEncodingOutputFormatId { get; set; }

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

        [IgnoreAll]
        public bool IsDeleted { get; set; }
       
        [IgnoreAll]
        public int RowTotal { get; set; }
    }
}
