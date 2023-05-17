using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Banner
{
    [Table("BannerSetting")]
    public class BannerSetting
    {

        [Key]
        public int BannerSettingId { get; set; }

        public int KeyId { get; set; }
        
        public int ImageHeight { get; set; }

        public int ImageWidth { get; set; }
    }
}
