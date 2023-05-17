using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.Dash.Codes;
using Kachuwa.Dash.Executor;
using Kachuwa.Dash.Filters;
using Kachuwa.Dash.Media;
using Kachuwa.Dash.Model;

namespace Kachuwa.Dash
{
    public class KachuwaDashEncoder : IExecutor
    {
        private readonly List<BaseDashFilter> _filters;


        private string _inputPath;
        private string _outputDir;
        private string _fileName;
       // private MediaStream _source;
        private CodeBase _code;
        private DASHTool _dashTool;

        private KachuwaDashEncoder(DASHTool dashTool)
        {
            _dashTool = dashTool;
               _filters = new List<BaseDashFilter>();
        }
        private string _commands;
        private bool _isDirectCommands = false;
        public KachuwaDashEncoder WithPlainCommands(DashPackagingParams encodingParams)
        {
            _commands = encodingParams.ToString();
            // _inputPath = $"\"{filePath}\""; ;
          
            _isDirectCommands = true;
            return this;
        }
        public KachuwaDashEncoder WidthInput(string filePath)
        {
            _inputPath = filePath;
            // _inputPath = $"\"{filePath}\""; ;
           // _source = new MediaStream(_inputPath);
            return this;
        }
        public KachuwaDashEncoder WidthMultipleInputs(string[] filePaths)
        {
            _inputPath = string.Join(" ", filePaths);
            // _inputPath = $"\"{filePath}\""; ;
           // _source = new MediaStream(_inputPath);
            return this;
        }

        public KachuwaDashEncoder WithFilter(BaseDashFilter filter)
        {
            if (_filters.Any(x => x.Name.Equals(filter.Name, StringComparison.OrdinalIgnoreCase)))
            {
                var old = _filters.First(x => x.Name.Equals(filter.Name, StringComparison.OrdinalIgnoreCase));
                _filters.Remove(old);
            }

            _filters.Add(filter);
            return this;
        }

        public KachuwaDashEncoder To<T>(string outputDir) where T : CodeBase, new()
        {
            _outputDir = outputDir;
            _code = new T();
            return this;
        }
        public KachuwaDashEncoder To<T>(string outputDir, string fileName) where T : CodeBase, new()
        {
            _outputDir = outputDir;
            _fileName = fileName;
            _code = new T();
            return this;
        }

        public static KachuwaDashEncoder Create(DASHTool dashTool)
        {
           // Config.Instance.Bento4Path[dashTool];
            return new KachuwaDashEncoder(dashTool);
        }

        public string Execute(long videoLogId=0)
        {
            // Validate();

            // FixFilters();
            var message = "";
            if (_isDirectCommands)
            {
              

                message = KachuwaDashProcessor.Dash(_dashTool, _commands, videoLogId);
            }
            else
            {
                var @params = BuildParams();

                 message = KachuwaDashProcessor.Dash(_dashTool, @params, videoLogId);
            }
           
            // frames per second

            if (message.IndexOf("frames per second", StringComparison.InvariantCultureIgnoreCase) > 0)
            {
                return "OK";
            }
            else if (message.IndexOf("video", StringComparison.InvariantCultureIgnoreCase) > 0)
            {
                // //video:2049kB audio:748kB subtitle:0kB other streams:0kB global headers:0kB muxing overhead: 0.893171% from vp9 
                return "OK";//vp 9
            }
            //else
            //if ((!string.IsNullOrWhiteSpace(message) && -1 == message.IndexOf("kb/s", StringComparison.InvariantCultureIgnoreCase)))
            //    throw new ApplicationException(message);

            return message;
        }

        private string BuildParams()
        {
            var builder = new StringBuilder();

         
            builder.Append(" ");
            builder.Append(_inputPath);

            foreach (var filter in _filters.OrderBy(x => x.Rank))
            {
              //  filter.Source = _source;
                builder.Append(filter.Execute());
            }

            var dir = Path.GetDirectoryName(_outputDir);

            if (string.IsNullOrWhiteSpace(dir))
                throw new ApplicationException("output directory error.");

            var fileName = Path.GetFileNameWithoutExtension(_outputDir);
            if (!Directory.Exists(_outputDir))
            {
                Directory.CreateDirectory(_outputDir);
            }
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ApplicationException("output filename is null");

            // builder.AppendFormat(" {0}\\{1}{2}", dir, fileName, _code.Extension);
            builder.Append(" ");

            if (_dashTool == DASHTool.Mp4Dash)
            {
                //output specified through filters
            }
            else
            {
                builder.Append(Path.Combine(_outputDir, _fileName + _code.Extension));
            }

            return builder.ToString();
        }

        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(_inputPath))
            {
                throw new ApplicationException("input file is null.");
            }

            if (string.IsNullOrWhiteSpace(_outputDir))
            {
                throw new ApplicationException("outout path is null");
            }

            var outdir = Path.GetDirectoryName(_outputDir);

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
                  //  iFilter.Source = _source;

                    _filters.Remove(iFilter);

                    Task.Run(() =>
                    {
                        Processor.FFmpeg(iFilter.ToString(),0);
                    });
                }

            }

            //if (!_source.AudioInfo.CodecName.Equals("aac", StringComparison.OrdinalIgnoreCase) &&
            //    !_filters.Any(x => x.Name.Equals("AudioChannel", StringComparison.OrdinalIgnoreCase)))
            //{
            //    //_filters.Add(new AudioChannelFilter(2));
            //    _filters.Add(new AudioChannelFilter(1));
            //}

            //if (_filters.Any(x => x.Name.Equals("X264", StringComparison.OrdinalIgnoreCase)) &&
            //    !_filters.Any(x => x.Name.Equals("Resize", StringComparison.OrdinalIgnoreCase)))
            //{
            //    _filters.Add(new ResizeFilter());
            //}

            //if (_code.Name.Equals("flv", StringComparison.OrdinalIgnoreCase))
            //{
            //    WithFilter(new AudioRatelFilter(44100));
            //}
        }
    }
}