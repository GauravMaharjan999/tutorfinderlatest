namespace Kachuwa.Dash.Filters
{
    public class VideoRateFilter : FilterBase
    {
        public int Rate { get; private set; }

        /// <summary>
        /// set frame rate
        /// </summary>
        /// <param name="rate">Hz value, fraction or abbreviation</param>
        public VideoRateFilter(int rate)
        {
            Name = "VideoRate";
            FilterType = FilterType.Video;
            Rate = rate;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -r ", Rate);
        }
    }

    public class LossLessFilter : FilterBase
    {
        public int Rate { get; private set; }

        /// <summary>
        /// set frame rate
        /// </summary>
        /// <param name="rate">Hz value, fraction or abbreviation</param>
        public LossLessFilter(int rate)
        {
            Name = "LossLess";
            FilterType = FilterType.Video;
            Rate = rate;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -lossless ", Rate);
        }
    }
    public class SpeedFilter : FilterBase
    {
        public int Rate { get; private set; }

        /// <summary>
        /// set frame rate
        /// </summary>
        /// <param name="rate">Hz value, fraction or abbreviation</param>
        public SpeedFilter(int rate)
        {
            //rate =0 to 4
            //0=>highest quality 
            //4=>lowest quality
            //5-8 For live streaming Valid values are 5 to 8, 
            Name = "Speed";
            FilterType = FilterType.Video;
            Rate = rate;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -speed ", Rate);
        }
    }
}