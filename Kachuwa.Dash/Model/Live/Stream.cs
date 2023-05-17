using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;

namespace Kachuwa.Dash.Live
{
    [Table("Stream")]
    public class Stream
    {
        [Key]
        public long StreamId { get; set; }
        public string StreamKey { get; set; }
        public string VideoId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Tags { get; set; }
        public string Description { get; set; }
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public bool IsOnAir { get; set; }

        public DateTime? OnAiredAt { get; set; }
        public bool IsFinished { get; set; }
        public DateTime? FinishedAt { get; set; }
      
        public string RTMP { get; set; }
        public long ViewCount { get; set; }
        public long LiveViewerCount { get; set; }
        public bool RecordVideo { get; set; }
        public string CoverImage { get; set; }
        public long Likes { get; set; }
        public long Dislikes { get; set; }
        public int DelayInSecond { get; set; }
      
      
        public long StreamedBy { get; set; }

        public bool AllowLiveChat { get; set; }
        public bool AllowQuestionaire { get; set; }
        public bool IsActive { get; set; }
      
        [IgnoreAll]
        public IFormFile CoverImageFile { get; set; }

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
        [IgnoreAll]
        public int CourseTimingId { get; set; }

        [IgnoreAll]
        public int EventId { get; set; }
        [IgnoreAll]
        public int ChooseLiveForWhat{ get; set; }

    }

    public class LiveStreamViewModel:Stream
    {

        public string StreamingBy { get; set; }
        public double AverageRating { get; set; }

    }

    public class NewStreamViewModel : Stream
    {

    }
    public class LiveStreamDetailViewModel : Stream
    {
        public string CourseName { get; set; }
        public int CourseId { get; set; }
        public string TutorName { get; set; }
        public string CourseShortDescription { get; set; }
        public int CourseTimingId { get; set; }
        public string ProfileImagePath { get; set; }
    }
    public class LiveEventsDetailViewModel : Stream
    {
        public string EventTitle { get; set; }
        public int EventId { get; set; }
        public string EventShortDescription { get; set; }
        public string ProfileImagePath { get; set; }

    }
}
