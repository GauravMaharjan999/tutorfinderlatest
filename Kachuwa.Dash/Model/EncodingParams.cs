using System;
using System.Drawing;
using System.Text;
using Kachuwa.Dash.Filters;

namespace Kachuwa.Dash.Model
{
    public class EncodingParams
    {
        public string InputFile { get; set; }
        public string OutputFile { get; set; }
        public string Commands { get; set; }
        public string LogoFile { get; set; }

        public override string ToString()
        {


            var builder = new StringBuilder();

            builder.Append(" -i");
            builder.Append(" ");
            builder.Append(this.InputFile);
            builder.Append(" ");

            WatermarkPosition position = WatermarkPosition.BottomRight;
            Point Offset = new Point(10, 10);
            string overlayFormat;

            switch (position)
            {
                case WatermarkPosition.TopLeft:
                    overlayFormat = "{0}:{1}";
                    break;
                case WatermarkPosition.TopRight:
                    overlayFormat = "main_w-overlay_w-{0}:{1}";
                    break;
                case WatermarkPosition.BottomLeft:
                    overlayFormat = "{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.BottomRight:
                    overlayFormat = "main_w-overlay_w-{0}:main_h-overlay_h-{1}";
                    break;
                case WatermarkPosition.Center:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleLeft:
                    overlayFormat = "{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.MiddleRight:
                    overlayFormat = "main_w-overlay_w-{0}:(main_h-overlay_h)/2-{1}";
                    break;
                case WatermarkPosition.CenterTop:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:{1}";
                    break;
                case WatermarkPosition.CenterBottom:
                    overlayFormat = "(main_w-overlay_w)/2-{0}:main_h-overlay_h-{1}";
                    break;

                default:
                    throw new ArgumentException("unknown wartermark position");

            }

            var overlayPostion = String.Format(overlayFormat, Offset.X, Offset.Y);
            builder.AppendFormat(" -vf \"movie=\\'{0}\\' [watermark]; [in][watermark] overlay={1} [out]\"",
                LogoFile.Replace("\\", "\\\\"), overlayPostion);

            builder.Append(this.Commands);

            builder.Append(this.OutputFile);

            return builder.ToString();


        }
    }
}