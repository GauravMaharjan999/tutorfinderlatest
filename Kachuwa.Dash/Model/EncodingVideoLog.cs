using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("EncodingVideoLog")]
    public class EncodingVideoLog
    {
        [Key]
        public long EncodingVideoLogId { get; set; }
        [Required(ErrorMessage = "EncodingVideoLog.VideoId.Required")]
        public string VideoId { get; set; }
        [Range(1, 999999, ErrorMessage = "EncodingVideoLog.EncodingOutputStepId.Required")]

        public int EncodingOutputStepId { get; set; }
        public int EncodingProcessId { get; set; }
        public string JobId { get; set; }
        public string ParentJobId { get; set; } = "0";
        public string UsedLogo { get; set; }
        public string Resolution { get; set; }
        public string Commands { get; set; }
        public string InputVideo { get; set; }
        public string OutputVideo { get; set; }
        public DateTime? EncodingStartAt { get; set; }
        public DateTime? EncodingEndAt { get; set; }
        public bool IsEncodingFinished { get; set; }
        public bool IsRequiredForDash { get; set; }
        public bool IsJobStarted { get; set; }

        public DateTime? JobStartedAt { get; set; }
        public DateTime? JobEndAt { get; set; }
        public string Flag { get; set; }
        public string ErrorMessage { get; set; }
        public bool NotifyUserOnSuccess { get; set; }
        public string NotificationSucceessEmail { get; set; }
        public bool NotifyUserOnError { get; set; }
        public string NotificationErrorEmail { get; set; }
        public string NotificationListnerUrl { get; set; }

        public  string Response { get; set; }
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