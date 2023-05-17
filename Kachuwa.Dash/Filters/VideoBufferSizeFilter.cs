namespace Kachuwa.Dash.Filters
{
    public class VideoBufferSizeFilter : FilterBase
    {
        public string Size { get; private set; }

        /// <summary>
        /// set frame rate
        /// </summary>
        /// <param name="size">Hz value, fraction or abbreviation(eg. 64k)</param>
        public VideoBufferSizeFilter(string size)
        {
            Name = "VideoBufferSize";
            FilterType = FilterType.Video;
            Size = size;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -bufsize ", Size);
        }
    }
}