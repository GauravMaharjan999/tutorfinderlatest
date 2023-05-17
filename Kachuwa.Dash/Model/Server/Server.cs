using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("Server")]
    public class Server
    {
        [Key]
        public int ServerId { get; set; }
        [Required(ErrorMessage = "Server.DisplayName.Required")]
        public string DisplayName { get; set; }
        [Required(ErrorMessage = "Server.MachineName.Required")]
        public string MachineName { get; set; }
        [Required(ErrorMessage = "Server.DisplayName.Required")]
        public string IPAddress { get; set; }
        [Required(ErrorMessage = "Server.UserName.Required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Server.Password.Required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Server.ServerType.Required")]
        public string ServerType { get; set; }
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
