namespace Kachuwa.Dash.Codes
{
    public class Mp4 : CodeBase
    {
        public Mp4()
        {
            CodeType = CodeType.Video;
            Name = "MP4";
            Extension = ".mp4";
        }
    }
    public class Webm : CodeBase
    {
        public Webm()
        {
            CodeType = CodeType.Video;
            Name = "Webm";
            Extension = ".webm";
        }
    }

    public class DASH : CodeBase
    {
        public DASH()
        {
            CodeType = CodeType.Video;
            Name = "MPD";
            Extension = ".mpd";
        }
    }
}