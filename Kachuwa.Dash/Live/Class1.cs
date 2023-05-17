using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.IO;
using System.Text;
using Kachuwa.Dash.Model;
using Kachuwa.Dash.Services;
using Kachuwa.Data.Crud.Attribute;
using Newtonsoft.Json;
using Org.BouncyCastle.Crypto.Digests;

namespace Kachuwa.Dash.Live
{
    public class LiveProcessor
    {
        internal static string FFmpeg(string @params, Action<int> onStart = null)
        {
            return Execute(true, @params, onStart);
        }
        internal static string FFmpeg(string @params, long streamId = 0)
        {
            return Execute(true, @params, null, streamId);
        }
        public static string FFprobe(string @params, Action<int> onStart = null)
        {
            return Execute(false, @params, onStart);
        }
        public static VideoFileInfo GetVideoInfo(string inputFile)
        {
            Process p = null;

            try
            {
                const string paramStr = " -v quiet -print_format json -hide_banner -show_format -show_streams -pretty {0}";
                var @params = string.Format(paramStr, inputFile);
                using (p = new Process())
                {
                    var workdir = Path.GetDirectoryName(Config.Instance.FFmpegPath);

                    if (string.IsNullOrWhiteSpace(workdir))
                        throw new ApplicationException("work directory is null");

                    var exePath = Config.Instance.FFprobePath;

                    var info = new ProcessStartInfo(exePath)
                    {
                        Arguments = @params,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false, //allow batch script file
                        WorkingDirectory = workdir
                    };

                    p.StartInfo = info;
                    p.Start();

                    // *** Read the streams ***
                    var message = string.Empty;


                    message = p.StandardOutput.ReadToEnd();

                    if (p.HasExited)
                    {
                        int exitCode = p.ExitCode;
                        if (exitCode != 0)
                        {

                            throw new Exception(message);
                        }
                    }


                    return JsonConvert.DeserializeObject<VideoFileInfo>(message);
                }
            }
            catch (Exception ex)
            {
                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }

                throw ex;
            }
            finally
            {
                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }
            }

        }

        public static string Execute(bool userFFmpeg, string @params, Action<int> onStart = null, long streamId = 0)
        {
            Process p = null;
            var encodingService = new EncodingService();
            try
            {
                encodingService.LogToFile(@params);
                using (p = new Process())
                {
                    var workdir = Path.GetDirectoryName(Config.Instance.FFmpegPath);

                    if (string.IsNullOrWhiteSpace(workdir))
                        throw new ApplicationException("work directory is null");

                    var exePath = userFFmpeg ? Config.Instance.FFmpegPath : Config.Instance.FFprobePath;

                    var info = new ProcessStartInfo(exePath)
                    {
                        Arguments = @params,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false, //allow batch script file
                        WorkingDirectory = workdir
                    };

                    p.StartInfo = info;
                    p.Start();

                    if (streamId > 0)
                        encodingService.LogEncodingStart(streamId, p.Id);

                    if (null != onStart)
                    {
                        onStart.Invoke(p.Id);
                    }

                    // *** Read the streams ***
                    var message = string.Empty;

                    if (userFFmpeg)
                    {
                        while (!p.StandardError.EndOfStream)
                        {
                            message = p.StandardError.ReadLine();
                        }

                    }
                    else
                    {
                        message = p.StandardOutput.ReadToEnd();
                    }

                    if (p.HasExited)
                    {
                        int exitCode = p.ExitCode;
                        if (exitCode != 0)
                        {

                            throw new Exception(message);
                        }
                    }

                    if (streamId > 0)
                        encodingService.LogEncodingEnd(streamId, message, message);
                    return message;
                }
            }
            catch (Exception ex)
            {
                if (streamId > 0)
                    encodingService.LogEncodingError(streamId, ex.Message);
                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }

                throw ex;
            }
            finally
            {
                if (null != p)
                {
                    p.Close();
                    p.Dispose();
                }
            }

        }
    }
   
}
