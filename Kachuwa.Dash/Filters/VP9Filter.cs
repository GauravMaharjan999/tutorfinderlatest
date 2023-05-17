using System.Text;

namespace Kachuwa.Dash.Filters
{
    public class VP9Filter : FilterBase
    {

        public VP9Quality Quality { get; set; }

        /// <summary>
        /// set x264 to encode the movie in Constant Quantizer mode.
        /// range 0-54.
        /// a setting of 0 will produce lossless output.
        /// usually 21-26
        /// </summary>
        public int? ConstantQuantizer { get; set; }
        public int? MaxRate { get; set; }
        public int? MinRate { get; set; }

        //todo -movflags faststart

        public VP9Filter()
        {
            Name = "VP9";
            FilterType = FilterType.Video;
            Rank = 1;
            //-c:a libvorbis or libopus 
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(" -c:v libvpx-vp9");

            if (VP9Quality.Good != Quality)
            {
                var param = Quality.GetDescription();

                if (!string.IsNullOrWhiteSpace(param))
                {//-deadline or -quality 
                    builder.AppendFormat(" -quality {0}", param);
                }
            }

            if (null != ConstantQuantizer)
            {
                if (ConstantQuantizer < 0 || ConstantQuantizer > 51)
                {
                    ConstantQuantizer = 22;
                }

                builder.AppendFormat(" -crf {0}", ConstantQuantizer.Value);
            }

            return builder.ToString();
        }
    }
}