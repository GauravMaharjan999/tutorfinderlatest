namespace Kachuwa.Dash.Filters
{
    public class ThreadFilter : FilterBase
    {
        public int Threads { get; }
        
        public ThreadFilter(int threads)
        {
            Name = "VideoBitrate";
            FilterType = FilterType.Video;
            Threads = threads;
            Rank = 6;
        }

        public override string ToString()
        {
            return string.Concat(" -threads ", Threads);
        }
    }
}