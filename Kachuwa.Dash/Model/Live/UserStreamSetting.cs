using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kachuwa.Dash.Live
{
    [Table("UserStreamSetting")]
    public class UserStreamSetting
    {
        [Key]
        public long StreamSettingId { get; set; }
        public long UserId { get; set; }
        public long LiveEncodingId { get; set; }
        public string LiveEncodingFormatIds { get; set; }


    }

    public class UserStreamSettingViewModel
    {
        public LiveEncoding Encoding { get; set; }
        public List<LiveEncodingFormat> Formats { get; set; }
    }
}