using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;
using Kachuwa.Data.Extension;

namespace Kachuwa.Banner
{
    [Table("Banner")]
    public class BannerInfo
    {
        [Key]
        public int BannerId { get; set; }

        public int KeyId { get; set; }
        public bool IsVideo { get; set; }
        public string VideoLink { get; set; }
        public string EmbeddedVideoLink { get; set; }
     
        public string Image { get; set; }

        public string HeadingText { get; set; }

        public string Content { get; set; }

        public string HeadingTextAnimation { get; set; }

        public string HeadingContentAnimation { get; set; }

        public string LinkAnimation { get; set; }

        public string BannerAnimation { get; set; }

        public string BannerContentPositionClass { get; set; }

        public string BannerContentColor { get; set; }

        public string BannerLinkColor { get; set; }

        public string BannerImagePosition { get; set; }

        public string BannerBackgroundColor { get; set; }

        public string BannerHeadingColor { get; set; }

        public string Link { get; set; }

        public bool IsTemporary { get; set; }

        [AutoFill(AutoFillProperty.CurrentDate)]
        public DateTime ExpiredOn { get; set; }

        public bool IsActive { get; set; }
        [IgnoreUpdate]
        public bool IsDeleted { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        [AutoFill(AutoFillProperty.CurrentUser)]
        [IgnoreUpdate]
        public string AddedBy { get; set; }
        [IgnoreAll]
        public int RowTotal { get; set; }
        [IgnoreAll]
        public string Suffix { get; set; }

    }
}