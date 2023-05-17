using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kachuwa.Dash.Executor;
using Kachuwa.Dash.Live;
using Kachuwa.Data;

namespace Kachuwa.Dash.Services
{

    public interface IRTMPService
    {

        Task<bool> CheckAsync(string streamKey);
        bool Check(string streamKey);
        TimeSpan CheckDisconnection(string streamKey);
    }
    public class RTMPService : IRTMPService
    {

        private readonly StreamingService _streamingService = new StreamingService();
        public async Task<bool> CheckAsync(string streamKey)
        {
            try
            {

                var stream = await _streamingService.LiveStreamService.GetAsync("Where StreamKey=@StremKey", new { StremKey = streamKey });
                // string response = Processor.FFprobe($" -v quiet -print_format json -show_streams {rtmpUrl}");
                CancellationTokenSource cts = new CancellationTokenSource();
                var token = cts.Token;
                Process p = null;
                Task<string> output = Task.Factory.StartNew(
                    () =>
                    {
                        try
                        {
                            using (p = new Process())
                            {
                                var workdir = Path.GetDirectoryName(Config.Instance.FFmpegPath);

                                if (string.IsNullOrWhiteSpace(workdir))
                                    throw new ApplicationException("work directory is null");

                                var exePath = Config.Instance.FFprobePath;

                                var info = new ProcessStartInfo(exePath)
                                {
                                    Arguments = $" -v quiet -print_format json -show_streams {stream.RTMP}",
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

                                return message;

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
                    }, token);


                bool didTaskRunInTime = output.Wait(5000, token);
                string response = "";
                if (didTaskRunInTime)
                {
                    response = output.Result;
                    if (response.Contains("streams"))
                    {

                        var status = new StreamStatus()
                        {
                            StreamKey = streamKey,
                            ErrorMessage = "",
                            LastChecked = DateTime.Now,
                            RecievingSignal = true,
                            IncomingIpAddress = "",

                        };
                        _streamingService.StreamStatusService.Insert<long>(status);
                        return true;
                    }
                    else
                    {
                        output.Dispose();
                        var status = new StreamStatus()
                        {
                            StreamKey = streamKey,
                            ErrorMessage = "No Rtmp signal",
                            LastChecked = DateTime.Now,
                            RecievingSignal = false,
                            IncomingIpAddress = "",

                        };
                        _streamingService.StreamStatusService.Insert<long>(status);

                        return false;
                    }
                }
                else
                {
                    if (output.Status == TaskStatus.Running)
                    {
                        p.Kill();
                    }

                }
                return false;
            }
            catch (TaskCanceledException e)
            {
                //cancel task will throw out a exception, just catch it, do nothing.
                throw e;
            }
        }

        public bool Check(string streamKey)
        {
            try
            {
                var stream = _streamingService.LiveStreamService.Get("Where StreamKey=@StremKey", new { StremKey = streamKey });
                // string response = Processor.FFprobe($" -v quiet -print_format json -show_streams {rtmpUrl}");
                CancellationTokenSource cts = new CancellationTokenSource();
                var token = cts.Token;
                Task<string> output = Task.Factory.StartNew(
                    () => Processor.FFprobe($" -v quiet -print_format json -show_streams {stream.RTMP}"), token);

                bool didTaskRunInTime = output.Wait(5000, token);
                string response = "";
                if (didTaskRunInTime)
                {
                    response = output.Result;
                    if (response.Contains("streams"))
                    {

                        var status = new StreamStatus()
                        {
                            StreamKey = streamKey,
                            ErrorMessage = "",
                            LastChecked = DateTime.Now,
                            RecievingSignal = true,
                            IncomingIpAddress = "",

                        };
                        _streamingService.StreamStatusService.Insert<long>(status);
                        return true;
                    }
                    else
                    {


                        var status = new StreamStatus()
                        {
                            StreamKey = streamKey,
                            ErrorMessage = "No Rtmp signal",
                            LastChecked = DateTime.Now,
                            RecievingSignal = false,
                            IncomingIpAddress = "",

                        };
                        _streamingService.StreamStatusService.Insert<long>(status);

                        return false;
                    }
                }
                else
                {
                    if (output.Status == TaskStatus.Running)
                    {
                        cts.Cancel();
                    }

                }
                return false;
            }
            catch (TaskCanceledException e)
            {
                //cancel task will throw out a exception, just catch it, do nothing.
                return false;
            }
        }

        public TimeSpan CheckDisconnection(string streamKey)
        {
            var statuses = _streamingService.StreamStatusService.GetList(
                "Where StreamKey=@StreamKey   Order by LastChecked desc",
                new { RecievingSignal = false, StreamKey = streamKey });
            var lastStatus = statuses.FirstOrDefault();
            if (lastStatus == null)
            {
                return TimeSpan.FromMinutes(0);
            }
            StreamStatus secondLastStaus = null;
            if (statuses.Count() > 1)
                secondLastStaus = statuses.ToList()[1];


            if (secondLastStaus != null && secondLastStaus.RecievingSignal)
            {
                //second last good last good
                if (lastStatus.RecievingSignal)
                {
                    return TimeSpan.FromMinutes(0);
                }
                //second last good last lost
                if (!lastStatus.RecievingSignal)
                {
                    return TimeSpan.FromMinutes((DateTime.Now - lastStatus.LastChecked).Minutes);
                }

            }
            if (secondLastStaus != null && !secondLastStaus.RecievingSignal)
            {
                //second last lost last good
                if (lastStatus.RecievingSignal)
                {
                    return TimeSpan.FromMinutes(0);
                }
                //second last lost last lost
                if (!lastStatus.RecievingSignal)
                {
                    return TimeSpan.FromMinutes((lastStatus.LastChecked - secondLastStaus.LastChecked).Minutes);
                }

            }
            return TimeSpan.FromMinutes(10);
        }
    }
}