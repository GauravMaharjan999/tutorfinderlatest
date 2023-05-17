using System.ComponentModel;

namespace Kachuwa.Dash.Filters
{
    public enum AudioCodec
    {
        [Description("ac3")]
        AC3,
        [Description("aac")]
        AAC,
        [Description("libvorbis")]
        Libvorbis,
        [Description("libopus")]
        Libopus,
        [Description("copy")]
        Copy
    }
}