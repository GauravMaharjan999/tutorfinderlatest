using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Kachuwa.Dash
{
    /// <summary>
    /// base config
    /// </summary>
    public sealed class Config
    {


        /// <summary>
        /// the path for ffmpeg.exe
        ///  </summary>
        /// <remarks>
        /// default:{Host runtime Directory}/external/ffmpeg/{OS architecture(for 32bit is x86,for 64bit is x64)}/ffmpeg.exe
        /// </remarks>
        public string FFmpegPath { get; set; }

        /// <summary>
        /// the path for ffprobe.exe
        ///  </summary>
        /// <remarks>
        /// default:{Host runtime Directory}/external/ffmpeg/{OS architecture(for 32bit is x86,for 64bit is x64)}/ffprobe.exe
        /// </remarks>
        public string FFprobePath { get; set; }
        public Dictionary<DASHTool, string> Bento4Path = new Dictionary<DASHTool, string>();

        private Config()
        {
            var currentDir = new FileInfo(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
            //var appPath = currentDir.DirectoryName;
            var arch = Environment.Is64BitOperatingSystem ? "x64" : "x86";
            FFmpegPath = Path.Combine(currentDir.DirectoryName, "external", "ffmpeg", arch, "ffmpeg.exe");
            FFprobePath = Path.Combine(currentDir.DirectoryName, "external", "ffmpeg", arch, "ffprobe.exe");

            Bento4Path.Add(DASHTool.AAC2MP4, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "aac2mp4.exe"));
            Bento4Path.Add(DASHTool.Mp4Compact, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4compact.exe"));
            Bento4Path.Add(DASHTool.Mp4DCFPackager, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4dcfpackager.exe"));
            Bento4Path.Add(DASHTool.Mp4Decrypt, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4decrypt.exe"));
            Bento4Path.Add(DASHTool.Mp4Dump, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4dump.exe"));
            Bento4Path.Add(DASHTool.Mp4Dash, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4dash.bat"));
            Bento4Path.Add(DASHTool.Mp4HLS, Path.Combine(currentDir.DirectoryName, "external", "bento4", "bin", "mp4hls.bat"));
            Bento4Path.Add(DASHTool.Mp4DashWithHLS, Path.Combine(currentDir.DirectoryName, "external", "bento4", "bin", "mp4dash.bat"));
            Bento4Path.Add(DASHTool.Mp4Edit, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4edit.exe"));
            Bento4Path.Add(DASHTool.Mp4Encrypt, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4encrypt.exe"));

            Bento4Path.Add(DASHTool.Mp4Extract, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4extract.exe"));

            Bento4Path.Add(DASHTool.Mp4Fragment, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4fragment.exe"));
            Bento4Path.Add(DASHTool.Mp4FrameIndex, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "Mp4IframeIndex.exe"));
            Bento4Path.Add(DASHTool.Mp4Info, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4info.exe"));
            Bento4Path.Add(DASHTool.Mp4Mux, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4mux.exe"));
            Bento4Path.Add(DASHTool.Mp4RPTHint, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4rtphintinfo.exe"));
            Bento4Path.Add(DASHTool.Mp4Split, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4split.exe"));
            Bento4Path.Add(DASHTool.Mp4Tag, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp4tag.exe"));
            Bento4Path.Add(DASHTool.Mp42AAC, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp42aac.exe"));
            Bento4Path.Add(DASHTool.Mp42AVC, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp42avc.exe"));
            Bento4Path.Add(DASHTool.Mp42HEVC, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp42hevc.exe"));
            Bento4Path.Add(DASHTool.Mp42HLS, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp42hls.exe"));
            Bento4Path.Add(DASHTool.Mp42TS, Path.Combine(currentDir.DirectoryName, "external", "bento4","bin",  "mp42ts.exe"));

        }




        /// <summary>
        /// The single instance for FFmpegConfg
        /// </summary>
        public static Config Instance
        {
            get { return ConfigInstance.instance; }
        }

        /// <summary>
        /// default output directory path;
        /// </summary>
        /// <remarks>
        /// it will not work when it's null or empty
        /// </remarks>
        public string OutputPath { get; set; }

        /// <summary>
        /// nested class for single instance
        /// </summary>
        class ConfigInstance
        {
            internal static readonly Config instance = new Config();

            static ConfigInstance()
            {

            }
        }
    }
}