using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Kachuwa.Dash.Utils;

namespace Kachuwa.Dash.Filters
{
    /// <summary>
    /// video resize filter
    /// </summary>
    public class ResizeFilter2 : FilterBase
    {
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
        private static readonly Dictionary<Resolution, string> Sufixes = new Dictionary<Resolution, string>
        {
            {Resolution.X144P, "_144P"},
            {Resolution.X240P, "_240P"},
            {Resolution.X360P, "_360P"},
            {Resolution.X480P, "_480P"},
            {Resolution.X720P, "_720"},
            {Resolution.X1080P,"_1080P"},
            {Resolution.X2K, "_2KP"},
            {Resolution.XQHD, "_qhd"},
            {Resolution.X4K, "_4K"},
            {Resolution.X8K, "_8K"},
        };

        public Resolution Resolution { get; private set; }

        private readonly string _inputeFile;
        private readonly string _videoId;
        public string OutputDir = "";

        public ResizeFilter2(string inputeFile,string videoId,string output="",Resolution resolution = Resolution.X480P)
        {
            Name = "Resize";
            FilterType = FilterType.Video;
            Resolution = resolution;
            _inputeFile = inputeFile;
            _videoId = videoId;
            OutputDir = output;
            this.RunIndependently = true;

        }
        public ResizeFilter2(Resolution resolution = Resolution.X480P)
        {
            Name = "Resize";
            FilterType = FilterType.Video;
            Resolution = resolution;
            OutputDir = "";
            this.RunIndependently = true;

        }
       
        public override string ToString()
        {
            var size = Sizes[Resolution];

            var builder = new StringBuilder();

            builder.Append(" -i");
            builder.Append(" ");
            builder.Append(_inputeFile);
           

            var dir = Path.GetDirectoryName(OutputDir);

            if (string.IsNullOrWhiteSpace(dir))
                throw new ApplicationException("output directory error.");

            string outputPath = Path.Combine(OutputDir, _videoId + ".mp4");
            var fileName = Path.GetFileNameWithoutExtension(outputPath);

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ApplicationException("output filename is null");
            //ffmpeg -i video_320x180.mp4 -vf scale=160:90 video_180x90.mp4 -hide_banner
            builder.AppendFormat(" -c:v libx265 -preset ultrafast -crf 28 -c:a aac -b:a 128k -b:v 2M -maxrate 2M -bufsize 1M -r 24 -vf scale={0}:{1} {2} -hide_banner", size.Width, size.Height, outputPath);

            return builder.ToString();
           
        }
    }
}