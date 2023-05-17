namespace Kachuwa.Dash.Filters
{
    public class AudioCodecFilter : FilterBase
    {
        public AudioCodec Codec { get; private set; }

        //-c:a aac /copy
        public AudioCodecFilter(AudioCodec codec)
        {
            Name = "AudioCodec";
            FilterType = FilterType.Audio;
            Codec = codec;
            Rank = 8;
        }

        public override string ToString()
        {
            var param = Codec.GetDescription();
            return string.Concat(" -c:a ", param);
        }
    }
}