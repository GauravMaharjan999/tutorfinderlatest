using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Kachuwa.Dash.Executor;

namespace Kachuwa.Dash.Filters
{
    public class ImageWatermarkFilter : FilterBase
    {
        public string ImageFile { get; private set; }

        public WatermarkPosition Position { get; private set; }
        public Point Offset { get; private set; }
        private static readonly Dictionary<Resolution, Size> Sizes = new Dictionary<Resolution, Size>
        {
            {Resolution.X144P, new Size(256, 144)},
            {Resolution.X240P, new Size(424, 240)},
            {Resolution.X360P, new Size(640, 360)},
            {Resolution.X480P, new Size(848, 480)},
            {Resolution.X720P, new Size(1280, 720)},
            {Resolution.X1080P, new Size(1920, 1080)},
            {Resolution.X2K, new Size(2048, 1080)},
            {Resolution.XQHD, new Size(2560 , 1440 )},
            {Resolution.X4K, new Size(3840 , 2160 )},
            {Resolution.X8K, new Size(7680 , 4320 )},
        };
        public ImageWatermarkFilter(string imageFile, WatermarkPosition position, Point offset)
        {
            Name = "ImageWatermark";
            FilterType = FilterType.Video;
            ImageFile = imageFile;
            Position = position;
            Offset = offset;
            Rank = 0;
        }

        public ImageWatermarkFilter(string imageFile, WatermarkPosition position)
        {
            Name = "ImageWatermark";
            FilterType = FilterType.Video;
            ImageFile = imageFile;
            Position = position;
            Offset = new Point(10, 10);
            Rank = 0;
        }

        private string outputDir = "";
        //public ImageWatermarkFilter(string imageFile, WatermarkPosition position,Resolution resolution = Resolution.X480P,string output="")
        //{
        //    Name = "ImageWatermark";
        //    FilterType = FilterType.Video;
        //    ImageFile = imageFile;
        //    Position = position;
        //    Offset = new Point(10, 10);
        //    Rank = 0;
        //    outputDir = output;
        //}
        public Resolution Resolution { get; private set; }
        public override string ToString()
        {
            if (!File.Exists(ImageFile))
                throw new ApplicationException("image file not exists.");

            var builder = new StringBuilder();
            var size = Sizes[Resolution];
            string overlayFormat;

            switch (Position)
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
            //if (!Directory.Exists(outputDir))
            //{
            //    Directory.CreateDirectory(outputDir);
            //}
            //var output = Path.Combine(outputDir, $"logo_{size.Width}.png");

            //Processor.Execute(true,
            //        $"-i {ImageFile.Replace("\\", "\\\\")} -y -v quiet -vf scale={size.Width}*0.15:-1 {output}", null);

            //builder.AppendFormat(" -vf \"movie=\\'{0}\\' [watermark]; [in][watermark] overlay={1} [out]\"",
            //    output.Replace("\\", "\\\\"), overlayPostion);
            //ffmpeg -i logo.png -y -v quiet -vf scale=1280*0.15:-1 scaled.png
            builder.AppendFormat(" -vf \"movie=\\'{0}\\' [watermark]; [in][watermark] overlay={1} [out]\"",
                ImageFile.Replace("\\", "\\\\"), overlayPostion);

            return builder.ToString();
        }
    }
}