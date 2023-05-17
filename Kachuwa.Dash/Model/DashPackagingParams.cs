using System.Text;

namespace Kachuwa.Dash.Model
{
    public class DashPackagingParams
    {
        public string[] InputFiles { get; set; }

        public string OutputFileName { get; set; }
        public string OutputDir { get; set; }
        public string Commands { get; set; }
        public DASHTool DashTool { get; set; }
        public string InputFile { get; set; }
        public string OutputFile { get; set; }

        public override string ToString()
        {


            var builder = new StringBuilder();

            switch (DashTool)
            {
                case DASHTool.Mp4Fragment:


                    builder.Append(" ");
                    builder.Append(this.InputFile);
                    builder.Append(" ");
                    //--fragment-duration 2000 --index --force-i-frame-sync all 
                    builder.Append(this.Commands);
                    builder.Append(" ");

                    builder.Append(this.OutputFile);

                    // return builder.ToString();

                    break;
                case DASHTool.Mp4Dash:

                    foreach (var input in this.InputFiles)
                    {
                        builder.Append(input);
                        builder.Append(" ");
                    }
                    builder.Append(" ");
                    builder.Append(this.Commands);
                    builder.Append(" ");
                    builder.Append(" --o ");
                    builder.Append(this.OutputDir);

                    builder.Append(" ");
                    builder.Append(" --mpd-name ");
                    builder.Append(this.OutputFileName + ".mpd");

                    builder.Append(" --force ");
                    break;

                case DASHTool.Mp4HLS:

                    foreach (var input in this.InputFiles)
                    {
                        builder.Append(input);
                        builder.Append(" ");
                    }
                    builder.Append(" ");
                    builder.Append(this.Commands);
                    builder.Append(" ");
                    builder.Append(" --output-dir ");
                    builder.Append(this.OutputDir);

                    builder.Append(" ");
                    builder.Append(" --master-playlist-name=");
                    builder.Append(this.OutputFileName + ".m3u8");

                    builder.Append(" --force ");
                    break;
                case DASHTool.Mp4DashWithHLS:

                    foreach (var input in this.InputFiles)
                    {
                        builder.Append(input);
                        builder.Append(" ");
                    }
                    builder.Append(" ");
                    builder.Append(this.Commands);
                    builder.Append(" ");
                    builder.Append(" --o ");
                    builder.Append(this.OutputDir);

                    builder.Append(" ");
                    builder.Append(" --mpd-name ");
                    builder.Append(this.OutputFileName + ".mpd");

                    builder.Append(" --hls ");

                    builder.Append(" --hls-master-playlist-name=");
                    builder.Append(this.OutputFileName + ".m3u8");
                    builder.Append(" --force ");
                    break;
            }


            return builder.ToString();


        }
    }
}