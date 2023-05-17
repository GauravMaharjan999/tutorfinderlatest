using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Dash.Model
{
    [Table("DashVideoLog")]
    public class DashVideoLog
    {
        [Key]
        public long DashVideoLogId { get; set; }
        public string VideoId { get; set; }
        public string UsedLogo { get; set; }
        public string Resolution { get; set; }
        public string InputVideo { get; set; }
        public int ConverstionProcessId { get; set; }
        public string ConvertedVideo { get; set; }
        public string FragmentedVideo { get; set; }
        public DateTime CoversionStartAt { get; set; }
        public DateTime ConversionEndAt { get; set; }
        public bool IsConvertionFinished { get; set; }
        public int FragmentationProcessId { get; set; }
        public DateTime FragmentationStartAt { get; set; }
        public DateTime FragmentationEndAt { get; set; }
        public bool IsFragmentationFinished { get; set; }
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
        public bool IsDeleted { get; set; }
        [IgnoreAll]
        public int RowTotal { get; set; }
    }
}