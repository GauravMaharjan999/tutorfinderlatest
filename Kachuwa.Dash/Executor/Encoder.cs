using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.Dash.Codes;
using Kachuwa.Dash.Filters;
using Kachuwa.Dash.Media;
using Kachuwa.Dash.Model;
using Kachuwa.Dash.Services;

namespace Kachuwa.Dash.Executor
{
    public class Encoder : IExecutor
    {
        private readonly List<FilterBase> _filters;


        private string _inputPath;
        private string _outputPath;
        private MediaStream _source;
        private CodeBase _code;
        private IEncodingService _encodingService { get; set; }

        private Encoder()
        {
            _filters = new List<FilterBase>();
        }
        private Encoder(IEncodingService encodingService)
        {
            _filters = new List<FilterBase>();
            _encodingService = encodingService;
        }

        private string _commands;
        private bool _isDirectCommands = false;
        public Encoder WithPlainCommands(EncodingParams encodingParams)
        {
            _commands = encodingParams.ToString();
              // _inputPath = $"\"{filePath}\""; ;
              _source = new MediaStream(encodingParams.InputFile);
            _isDirectCommands = true;
            _inputPath = encodingParams.InputFile;
            _outputPath = encodingParams.OutputFile;
            return this;
        }

        public Encoder WidthInput(string filePath)
        {
            _inputPath = filePath;
            // _inputPath = $"\"{filePath}\""; ;
            _source = new MediaStream(_inputPath);
            return this;
        }

        public Encoder WithFilter(FilterBase filter)
        {
            if (_filters.Any(x => x.Name.Equals(filter.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var old = _filters.First(x => x.Name.Equals(filter.Name, StringComparison.OrdinalIgnoreCase));
                _filters.Remove(old);
            }

            _filters.Add(filter);
            return this;
        }

        public Encoder To<T>(string ouputPath) where T : CodeBase, new()
        {
            _outputPath = ouputPath;
            _code = new T();
            return this;
        }

        public static Encoder Create()
        {
            return new Encoder();
        }
        public static Encoder Create(IEncodingService encodingService)
        {
            return new Encoder(encodingService);
        }

        public string Execute(long vlogId = 0)
        {
            Validate();

            FixFilters();
            var message = "";
            if (_isDirectCommands)
            {
                message = Processor.FFmpeg(this._commands, vlogId );
            }
            else
            {
                var @params = BuildParams();
                 message = Processor.FFmpeg(@params,  vlogId);
            }

            //if (message.IndexOf("Qavg", StringComparison.InvariantCultureIgnoreCase) > 0)
            //{
            //    return message;
            //}
            //else if (message.IndexOf("video", StringComparison.InvariantCultureIgnoreCase) > 0)
            //{
            //    // //video:2049kB audio:748kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: 0.893171% from vp9 
            //    return message;//vp 9
            //}
            //else 
            //if ((!string.IsNullOrWhiteSpace(message) && -1 == message.IndexOf("kb/s", StringComparison.InvariantCultureIgnoreCase)) )
            //    throw new ApplicationException(message);

            return message;
        }

        private string BuildParams()
        {
            // //    ffmpeg -i sourcevideo.mkv -an -movflags +faststart -vsync 1 -c:v libx264 -r 23.976 -preset slower -vf 640:-2 -b:v 800k -bufsize 800k -maxrate 880k -bf 3 -refs 3 -force_key_frames "expr:eq(mod(n,$keyframe_interval),0)" -x264-params rc-lookahead=${keyframe_interval}:keyint=${keyint}:min-keyint=${keyframe_interval} --pass 2 out360.mp4

            var builder = new StringBuilder();

            builder.Append(" -i");
            builder.Append(" ");
            builder.Append(_inputPath);

            builder.Append(" -pix_fmt yuv420p ");

            foreach (var filter in _filters.OrderBy(x => x.Rank))
            {
                filter.Source = _source;
                builder.Append(filter.Execute());
            }

            var dir = Path.GetDirectoryName(_outputPath);

            if (string.IsNullOrWhiteSpace(dir))
                throw new ApplicationException("output directory error.");

            var fileName = Path.GetFileNameWithoutExtension(_outputPath);

            if (string.IsNullOrWhiteSpace(fileName))
                throw new ApplicationException("output filename is null");
         
            builder.Append(
                "     -x264-params \"rc-lookahead = 100:keyint = 200:min-keyint = 100:hrd = 1:vbv_maxrate = 12000:vbv_bufsize = 12000:no-open-gop = 1\" ");
            builder.Append(
                "  -force_key_frames  \"expr: eq(mod(n, 100), 0)\" ");
            builder.AppendFormat(" {0}\\{1}{2}", dir, fileName, _code.Extension);

            return builder.ToString();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(_inputPath))
            {
                throw new ApplicationException("input file is null.");
            }

            if (string.IsNullOrWhiteSpace(_outputPath))
            {
                throw new ApplicationException("outout path is null");
            }

            var outdir = Path.GetDirectoryName(_outputPath);

            if (!string.IsNullOrWhiteSpace(outdir) && !Directory.Exists(outdir))
            {
                Directory.CreateDirectory(outdir);
            }
        }

        private void FixFilters()
        {
            if (_filters.Any(x => x.RunIndependently == true))
            {
                var iFilters = _filters.Where(x => x.RunIndependently == true).ToList();
                foreach (var iFilter in iFilters)
                {
                    iFilter.Source = _source;

                    _filters.Remove(iFilter);

                    Task.Run(() =>
                    {
                        Processor.FFmpeg(iFilter.ToString(),0);
                    });
                }

            }

            if (_source.AudioInfo != null)
            {
                if (!_source.AudioInfo.CodecName.Equals("aac", StringComparison.OrdinalIgnoreCase) &&
                    !_filters.Any(x => x.Name.Equals("AudioChannel", StringComparison.OrdinalIgnoreCase)))
                {
                    _filters.Add(new AudioChannelFilter(2));
                    // _filters.Add(new AudioChannelFilter(1));
                }
            }
            else
            {//copy
                _filters.Add(new AudioChannelFilter());
            }

            if (_filters.Any(x => x.Name.Equals("X264", StringComparison.OrdinalIgnoreCase)) &&
                 !_filters.Any(x => x.Name.Equals("Resize", StringComparison.OrdinalIgnoreCase)))
            {
                _filters.Add(new ResizeFilter());
            }

            if (!_isDirectCommands)
            {
                if (_code.Name.Equals("flv", StringComparison.OrdinalIgnoreCase))
                {
                    WithFilter(new AudioRatelFilter(44100));
                }
            }

            //    ffmpeg -i sourcevideo.mkv -an -movflags +faststart -vsync 1 -c:v libx264 -r 23.976 -preset slower -vf 640:-2 -b:v 800k -bufsize 800k -maxrate 880k -bf 3 -refs 3 -force_key_frames "expr:eq(mod(n,$keyframe_interval),0)" -x264-params rc-lookahead=${keyframe_interval}:keyint=${keyint}:min-keyint=${keyframe_interval} --pass 2 out360.mp4
        }
    }
}