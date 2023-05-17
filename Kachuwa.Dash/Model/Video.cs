using Kachuwa.Data.Crud.Attribute;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kachuwa.Dash.Model
{
    [Table("Video")]
    public class Video
    {
        [Key]
        public Guid VideoId { get; set; }
      
        public string OriginalVideoPath { get; set; }
        public string DashVideoPath { get; set; }
        public string HLSVideoPath { get; set; }
        public string CovertedFiles { get; set; }
        public string TempPath { get; set; }
        public int CourseId { get; set; }
        public int ChapterId { get; set; }
        public bool IsDashReady { get; set; }
        public int Year { get; set; }
        public string ServerName { get; set; }
        public string Author { get; set; } = "";
        public int Duration { get; set; }
        public int EncodingOutputFormatId { get; set; }

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
