using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("ServerStorage")]
    public class ServerStorage
    {
        [Key]
        public int ServerStorageId { get; set; }
        public int ServerId { get; set; }

        //[Required(ErrorMessage = "DashVideoSetting.MachineName.Required")]
        [IgnoreAll]
        public string MachineName { get; set; }

        [Required(ErrorMessage = "DashVideoSetting.TempDirectory.Required")]
        public string TempDirectory { get; set; }
        [Required(ErrorMessage = "DashVideoSetting.RootDirectory.Required")]
        public string RootDirectory { get; set; }
        public bool IsRootDirectoryFull { get; set; }
        public int MinimumSpace { get; set; }
        public int UseOrder { get; set; }

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

        [Data.Crud.Attribute.NotMapped]
        public string RTMPAddress { get; set; }
    }

    
}