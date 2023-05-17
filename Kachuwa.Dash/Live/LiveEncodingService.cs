using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Hangfire;
using Kachuwa.Dash.Executor;
using Kachuwa.Dash.Services;
using Kachuwa.Log;
using Microsoft.AspNetCore.Hosting;

namespace Kachuwa.Dash.Live
{
    public interface ILiveEncodingService
    {
        void Encode(string @params, Action<int> onStart = null, string streamKey = "", string loggerPath = "");
        void StartEncodingAsync(string streamKey);
    }
    public class LiveEncodingService : ILiveEncodingService
    {
        private readonly StreamingService _liveStreamingService = new StreamingService();
        private readonly RTMPService _rtmpService = new RTMPService();
        public void StartEncodingAsync(string streamKey)
        {
            string videoId = _liveStreamingService.GetVideoIdByStreamKey(streamKey);
            var streamingService = new StreamingService();
            var serverService = new ServerService();
            var currentStorage = serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName, "LIVE").Result;
            if (!Directory.Exists(currentStorage.TempDirectory))
            {
                Directory.CreateDirectory(currentStorage.TempDirectory);
            }
            if (!Directory.Exists(currentStorage.RootDirectory))
            {
                Directory.CreateDirectory(currentStorage.RootDirectory);
            }
            //string rootoutputDirectory = Path.Combine(currentStorage.RootDirectory, streamKey);
            string rootoutputDirectory = Path.Combine(currentStorage.RootDirectory, videoId);
            if (!Directory.Exists(rootoutputDirectory))
            {
                Directory.CreateDirectory(rootoutputDirectory);
            }
            var userSettings = streamingService.GetStreamingSettings(streamKey).Result;
            string @param = LiveParamBuilder.Build(streamKey,videoId, rootoutputDirectory, userSettings.Encoding, userSettings.Formats);
            // var _jobStorageConnection = JobStorage.Current.GetConnection();

            Encode(@param, null, streamKey, rootoutputDirectory);
        }

        public void Encode(string @params, Action<int> onStart = null, string streamKey = "", string loggerPath = "")
        {
            Process p = null;

            try
            {
                _liveStreamingService.LogToFile(@params);
                using (p = new Process())
                {
                    var workdir = Path.GetDirectoryName(Config.Instance.FFmpegPath);

                    if (string.IsNullOrWhiteSpace(workdir))
                        throw new ApplicationException("work directory is null");

                    var exePath = Config.Instance.FFmpegPath;

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

                    if (!string.IsNullOrEmpty(streamKey))
                        _liveStreamingService.LogEncodingStart(streamKey, p.Id);
                    p.EnableRaisingEvents = true;


                    if (null != onStart)
                    {
                        onStart.Invoke(p.Id);
                    }
                    // string jobId= BackgroundJob.Enqueue(() => _rtmpService.CheckAsync(streamKey));
                    string jobName = $"RTMP_CHECK_{streamKey}";
                    RecurringJob.AddOrUpdate(jobName, () => _rtmpService.CheckAsync(streamKey), "*/1 * * * *");
                    //var con = Hangfire.JobStorage.Current.GetConnection();

                    var log = string.Empty;
                    var lastMessage = string.Empty;
                    var encodingLogger = new EncodingLogger(loggerPath);
                    System.Timers.Timer timer =
                        new System.Timers.Timer() { Enabled = true, Interval = (0.5 * 60 * 1000) };
                    timer.AutoReset = false;
                    timer.Elapsed += (o, args) =>
                    {
                        try
                        {


                            RecurringJob.RemoveIfExists(jobName);
                            p.Kill();

                            timer.Stop();
                            timer.Dispose();
                            encodingLogger.Log(LogType.Error, () => "Network/Encoding process timeout.");
                        }
                        catch (Exception e)
                        {
                            encodingLogger.Log(LogType.Error, () => "Network/Encoding process timeout.", e);
                        }

                    };
                    timer?.Start();

                    while (!p.StandardError.EndOfStream)
                    {
                        try
                        {
                            lastMessage = p.StandardError.ReadLine() + Environment.NewLine;
                            log += lastMessage;
                            timer?.Stop();
                            timer?.Start();
                            var message = lastMessage;
                            encodingLogger.Log(LogType.Info, () => message);
                        }
                        catch (TaskCanceledException e)
                        {
                            break;
                        }
                    }


                    if (p.HasExited)
                    {
                        int exitCode = p.ExitCode;

                        if (exitCode != 0)
                        { //forcly closed externally
                            if (exitCode == 1)
                            {
                                if (!string.IsNullOrEmpty(streamKey))
                                    _liveStreamingService.LogEncodingEnd(streamKey, log, log);
                            }
                            if (exitCode == -1)
                            {//network dissconnection and timer closed in 30 seconds
                                if (!string.IsNullOrEmpty(streamKey))
                                    _liveStreamingService.LogEncodingEnd(streamKey, log, log);
                            }
                            else
                            {
                                throw new Exception(lastMessage);
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(streamKey))
                        _liveStreamingService.LogEncodingEnd(streamKey, log, log);
                    // return message;
                }
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(streamKey))
                    _liveStreamingService.LogEncodingError(streamKey, ex.Message);
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