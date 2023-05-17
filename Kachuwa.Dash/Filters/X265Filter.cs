using System.Text;

namespace Kachuwa.Dash.Filters
{
    public class X265Filter : FilterBase
    {
        /// <summary>
        /// change options to trade off compression efficiency against encoding speed.
        /// default: Medium
        /// </summary>
        public X264Preset Preset { get; set; }
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

        public X265Filter()
        {
            Name = "X265";
            FilterType = FilterType.Video;
            Preset = X264Preset.Superfast;
            Rank = 1;
        }
        //ffmpeg -i sample2.mp4 -c:v libx265 -preset ultrafast -crf 28 -c:a aac -b:a 128k -b:v 2M -maxrate 2M -bufsize 1M -r 24 output.mp4
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.Append(" -c:v libx265");

            if (X264Preset.Medium != Preset)
            {
                var param = Preset.GetDescription();

                if (!string.IsNullOrWhiteSpace(param))
                {
                    builder.AppendFormat(" -preset {0}", param);
                }
            }

            if (null != ConstantQuantizer)
            {
                if (ConstantQuantizer < 0 || ConstantQuantizer > 51)
                {
                    ConstantQuantizer = 22;
                }

                builder.AppendFormat(" -qp {0}", ConstantQuantizer.Value);
            }
         
            return builder.ToString();
        }
    }
}