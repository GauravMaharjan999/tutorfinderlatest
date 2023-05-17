using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Kachuwa.Dash.Live
{
    public class LiveParamBuilder
    {
        public static string Build(string streamKey,string videoId, string outputDir, LiveEncoding encoding, List<LiveEncodingFormat> formats)
        {
            string rtmp = "rtmp://localhost/living/" + streamKey;
            string outputDirectory = Path.Combine(outputDir, videoId);
            if (!Directory.Exists(outputDirectory))
            {
                Directory.CreateDirectory(outputDirectory);
            }
            var builder = new StringBuilder();
            //Just add option stimeout ( in microseconds )
            // -reconnect 1 -reconnect_at_eof 1 -reconnect_streamed 1 -reconnect_delay_max 4294 -stimeout 10000000
            builder.Append($" -i {rtmp}  -max_muxing_queue_size 9999 ");
            builder.Append(encoding.InputCommand);//-max_muxing_queue_size 9999 -async 1 -vf yadif -g 29.97 -r 23 
            foreach (var v in formats)
            {
                builder.Append(v.Commands);
            }

            builder.Append(" -c:a aac -ar 48000 ");
            if (encoding.IsHls)
            {
                builder.Append("-f hls");
            }
            builder.Append(" -var_stream_map ");
            string xval = "";
            int counter = 0;
            foreach (var v in formats)
            {
                xval += $" v:{counter},a:{counter} ";
                counter++;
            }
            builder.Append("\"" + xval + " \" ");
            builder.Append(encoding.OtherCommand);
            string output = encoding.OutputCommand.Replace("{outputDir}", outputDirectory);
            output = output.Replace("/", @"\");
            builder.Append(output);

            return builder.ToString();
        }
    }
}