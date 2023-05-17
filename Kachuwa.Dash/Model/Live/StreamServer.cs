using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Live
{
    [Table("StreamServer")]
    public class StreamServer
    {
        [Key]
        public int StreamServerId { get; set; }
        public string MachineName { get; set; }
        public string IpAddress { get; set; }
        public string RTMPAddress { get; set; }
        public bool IsActive { get; set; }

        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }
        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreInsert]
        public string UpDatedBy { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreInsert]
        public DateTime UpdatedOn { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        [IgnoreInsert]
        [AutoFill(false)]
        public bool IsDeleted { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }

    }
}