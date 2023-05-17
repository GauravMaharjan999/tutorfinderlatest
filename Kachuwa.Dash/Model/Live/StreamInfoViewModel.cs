using System.Collections.Generic;

namespace Kachuwa.Dash.Live
{
    public class StreamInfoViewModel
    {
        public Stream StreamInfo { get; set; }
        public UserStreamSetting Setting { get; set; }
        public List<LiveEncodingFormat> StreamVideoQualities { get; set; }

    }
}