using System;
using Kachuwa.Dash.Filters;
using Kachuwa.Dash.Media;

namespace Kachuwa.Dash
{
    public class BaseDashFilter:IFilter
    {
        public MediaStream Source = null;

        public int Rank { get; protected set; }
        public string Name { get; protected set; }
        protected FilterType FilterType { get; set; }

        protected BaseDashFilter()
        {
            Rank = 9;
        }

        protected virtual void ValidateVideoStream()
        {
            if (null == Source)
                throw new ApplicationException("source file is null.");

            if (null == Source.VideoInfo)
                throw new ApplicationException("non video stream found in source file.");
        }

        protected virtual void ValidateAudioStream()
        {
            if (null == Source)
                throw new ApplicationException("source file is null.");

            if (null == Source.AudioInfo)
                throw new ApplicationException("non audio stream found in source file.");
        }

        public bool RunIndependently { get; set; } = false;

        public string Execute()
        {
            //multimple video files exists on dash so no validation
            //switch (FilterType)
            //{
            //    case FilterType.Audio:
            //        ValidateAudioStream();
            //        break;
            //    case FilterType.Video:
            //        ValidateVideoStream();
            //        break;
            //    default:
            //        throw new ApplicationException("unknown filter type.");
            //}
            return ToString();
        }
    }
}