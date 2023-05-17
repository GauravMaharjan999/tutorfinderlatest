using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kachuwa.Dash.Live
{
    [Table("StreamStatus")]
    public class StreamStatus
    {
        [Key]
        public long StreamStatusId { get; set; }
        public string StreamKey { get; set; }
        public bool RecievingSignal { get; set; }
        public string ErrorMessage { get; set; }
        public string IncomingIpAddress { get; set; }
        public DateTime LastChecked { get; set; }

    }
}