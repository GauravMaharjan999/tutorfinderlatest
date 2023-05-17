namespace Kachuwa.Dash.Filters
{
    public class VideoBitrateFilter : FilterBase
    {
        public string Bitrate { get; private set; }
        public string MaxRate { get; }

        /// <summary>
        /// set frame rate
        /// </summary>
        /// <param name="bitrate">Hz value, fraction or abbreviation(eg. 64k)</param>
        public VideoBitrateFilter(string bitrate,string maxRate)
        {
            Name = "VideoBitrate";
            FilterType = FilterType.Video;
            Bitrate = bitrate;
            MaxRate = maxRate;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -b:v ", Bitrate) + string.Concat(" -maxrate ", MaxRate);
        }
    }
}