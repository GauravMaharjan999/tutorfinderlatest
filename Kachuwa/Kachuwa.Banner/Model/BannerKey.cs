using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kachuwa.Data.Crud.Attribute;
using Kachuwa.Data.Extension;

namespace Kachuwa.Banner
{
    [Table("BannerKey")]
    public class BannerKey
    {
        [Key]
        public int BannerKeyId { get; set; }
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [AutoFill(true)]
        public bool IsActive { get; set; }
        [AutoFill(false)]
        public bool IsDeleted { get; set; }
        [AutoFill(AutoFillProperty.CurrentDate)]
        [IgnoreUpdate]
        public DateTime AddedOn { get; set; }
        [IgnoreUpdate]
        [AutoFill(AutoFillProperty.CurrentUser)]
        public string AddedBy { get; set; }
        [IgnoreAll]
        public int RowTotal { get; set; }
    }
}