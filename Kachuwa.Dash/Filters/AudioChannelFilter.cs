using System;

namespace Kachuwa.Dash.Filters
{
    /// <summary>
    /// audio channel select filter
    /// </summary>
    public class AudioChannelFilter : FilterBase
    {
        public int Channel { get; private set; }

        public AudioChannelFilter(int channel=0)
        {
            Name = "AudioChannel";
            FilterType = FilterType.Audio;
            Channel = channel;
            Rank = 8;
        }

        public override string ToString()
        {
            if (Channel == 0)
                return string.Concat(" -ac ", "copy");
            if (Channel > Source.AudioInfo.Channels)
                return string.Concat(" -ac ", 1); // throw new ApplicationException(string.Format("there only {0} channels in audio stream.", Source.AudioInfo.Channels));

            return string.Concat(" -ac ", Channel);
        }
    }
}