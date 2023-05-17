using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Hangfire;
using Hangfire.States;
using Hangfire.Storage;
using Kachuwa.Dash.Codes;
using Kachuwa.Dash.Executor;
using Kachuwa.Dash.Filters;
using Kachuwa.Dash.Model;
using Kachuwa.Data;
using Kachuwa.Data.Extension;
using Kachuwa.Log;
using Kachuwa.Web;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Encoder = Kachuwa.Dash.Executor.Encoder;

namespace Kachuwa.Dash.Services
{
    public class EncodingService : IEncodingService
    {
        private readonly IBackgroundJobClient _backgroundJobClient;
        private ILogger _logger;
        private readonly IServerService _serverService;

        // private readonly IBackgroundJobClient _backgroundJobClient;
        private IStorageConnection _jobStorageConnection;

        #region Crud 
        public CrudService<EncodingOutputFormat> OutputFormatCrudService { get; set; } = new CrudService<EncodingOutputFormat>();
        public CrudService<EncodingOutputStep> OutputStepCrudService { get; set; } = new CrudService<EncodingOutputStep>();
        public CrudService<EncodingVideoLog> EncodingVideoLogCrudService { get; set; } = new CrudService<EncodingVideoLog>();
        public CrudService<DashVideoSetting> DashVideoSettingService { get; set; } = new CrudService<DashVideoSetting>();
        public CrudService<Video> VideoCrudService { get; set; } = new CrudService<Video>();
        public CrudService<EncodingVideoLog> LogCrudService { get; set; } = new CrudService<EncodingVideoLog>();

        #endregion


        public EncodingService(IBackgroundJobClient backgroundJobClient, ILogger logger, IServerService serverService)
        {
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
            _serverService = serverService;
            _jobStorageConnection = JobStorage.Current.GetConnection();
        }

        public EncodingService()
        {

        }
        public async Task<EncodingStatus> GetEncodingStatusAsyync(string videoId)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                return await
                    db.QuerySingleOrDefaultAsync<EncodingStatus>("dbo.usp_Encoding_Status", new { VideoId = videoId },
                        commandType: CommandType.StoredProcedure);
            }
        }
        public async Task<IEnumerable<EncodingVideoLog>> GetDashReadyFiles(string videoId)
        {
            return await EncodingVideoLogCrudService.GetListAsync("Where VideoId=@videoId", new { videoId });
        }

        public async Task<string> EncodeAsnyc(string videoId)
        {

            //    Encoder.Create()
            //        .WidthInput(inputPath)
            //        .WithFilter(new VP9Filter() { Quality = VP9Quality.Good, ConstantQuantizer = 23 })
            //        .WithFilter(new VideoBitrateFilter("2M", "2M"))
            //        .WithFilter(new VideoRateFilter(28))
            //        .WithFilter(new VideoRateFilter(28))
            //        .WithFilter(new AudioBitrateFilter(128))
            //        .WithFilter(new AudioCodecFilter(AudioCodec.Libopus))//aac
            //                                                             //.WithFilter(new ThreadFilter(3))//re
            //        .WithFilter(new ImageWatermarkFilter(Path.Combine(imagePath, okLogos[Resolution.X1080P]), WatermarkPosition.BottomRight))
            //        .WithFilter(new ResizeFilter(Resolution.X1080P))
            //        .WithFilter(new SnapshotFilter(Path.Combine(appPath, "output", "output.png"), 320, 180,
            //            10)) //with snapshot
            //        .To<Webm>(Path.Combine(outputPath, "X1080P"))
            //        .Execute();

            try
            {


                var encodingStatus = await GetEncodingStatusAsyync(videoId);
                var currentDir = new FileInfo(Uri.UnescapeDataString(new Uri(Assembly.GetExecutingAssembly().CodeBase).AbsolutePath));
                string logoPath = Path.Combine(currentDir.DirectoryName, "logos", "logo_X480P.png");
               // var dashSetting = await GetServerVideoSettingsAsnc(Environment.MachineName);
                var currentStorage = await _serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName);

                if (!Directory.Exists(currentStorage.TempDirectory))
                {
                    Directory.CreateDirectory(currentStorage.TempDirectory);
                }
                if (!Directory.Exists(currentStorage.RootDirectory))
                {
                    Directory.CreateDirectory(currentStorage.RootDirectory);
                }

                var tempencodingHolds =new EncodingVideoLog();
                if (encodingStatus.IsValidVideo)
                {
                    if (!encodingStatus.IsDashDone || !encodingStatus.IsEncodingDone)
                    {
                        var videoInfo = await VideoCrudService.GetAsync("Where VideoId=@videoId", new { videoId });
                        string rootoutputDirectory = Path.Combine(currentStorage.RootDirectory, Environment.MachineName,
                            DateTime.Now.Year.ToString(), videoInfo.Author, videoInfo.CourseId.ToString());
                        string encodingDirectory = Path.Combine(rootoutputDirectory, "Converted", videoInfo.VideoId.ToString());
                        string dashDirectory = Path.Combine(rootoutputDirectory, "Dash", videoInfo.VideoId.ToString());
                        if (!Directory.Exists(encodingDirectory))
                        {
                            Directory.CreateDirectory(encodingDirectory);
                        }
                        if (!Directory.Exists(dashDirectory))
                        {
                            Directory.CreateDirectory(dashDirectory);
                        }
                        var steps = await OutputStepCrudService.GetListAsync("Where EncodingOutputFormatId=@EncodingOutputFormatId order by [Order] asc", new { videoInfo.EncodingOutputFormatId });
                        steps = steps.OrderBy(x => x.Order).ToList();
                        var counter = 0;
                        foreach (var step in steps)
                        {
                            var videoLog = await LogCrudService.GetAsync("Where EncodingOutputStepId=@EncodingOutputStepId and VideoId=@VideoId",
                                  new { EncodingOutputStepId = step.EncodingOutputStepId, videoInfo.VideoId });

                            if (videoLog == null)
                            {


                                //log on job
                                var _vlog = new EncodingVideoLog()
                                {
                                    Commands = step.Commands,
                                    UsedLogo = logoPath,
                                    Resolution = "",
                                    InputVideo = "",
                                    OutputVideo = "",
                                    IsJobStarted = true,
                                    EncodingProcessId = 0,
                                    VideoId = videoId,
                                    Flag = step.Flag,
                                    JobId = "",
                                    JobStartedAt = DateTime.Now,

                                    EncodingOutputStepId = step.EncodingOutputStepId,
                                    IsRequiredForDash = step.Flag.Trim() == "f",
                                    NotificationErrorEmail = "",
                                    NotificationSucceessEmail = "",
                                    NotificationListnerUrl = "",
                                    NotifyUserOnError = false,
                                    NotifyUserOnSuccess = false,
                                };
                                _vlog.InputVideo = videoInfo.OriginalVideoPath;
                                _vlog.OutputVideo = Path.Combine(encodingDirectory, step.Name + ".mp4");//@"D:\OK\OnlineKachhya_Dev\SampleVideo\encodings\" + step.Name + ".mp4";
                                //adding entry video encoding
                                _vlog.AutoFill();
                                var vlogId = await LogCrudService.InsertAsync<long>(_vlog);
                                _vlog.EncodingVideoLogId = vlogId;

                                if (step.WaitPreviousStep)
                                {

                                    if ((counter - 1) >= 0)
                                    {
                                        var lastStep = tempencodingHolds;
                                        //previous output video as input for next step
                                        _vlog.InputVideo = lastStep.OutputVideo;

                                        string jobId = BackgroundJob.ContinueJobWith(lastStep.JobId,
                                            () => StartEncoding(_vlog, dashDirectory), JobContinuationOptions.OnlyOnSucceededState);
                                        //updaing with job id
                                        _vlog.JobId = jobId;
                                        _vlog.JobEndAt = DateTime.Now;
                                        _vlog.ParentJobId = lastStep.JobId;
                                        await LogCrudService.UpdateAsync(_vlog);
                                        counter = 0;
                                    }
                                    else
                                    {

                                        string jobId = BackgroundJob.Enqueue(() => StartEncoding(_vlog, dashDirectory));
                                        //updaing with job id
                                        _vlog.JobId = jobId;

                                        _vlog.JobEndAt = DateTime.Now;
                                        await LogCrudService.UpdateAsync(_vlog);
                                    }
                                }
                                else
                                {
                                    string jobId = BackgroundJob.Enqueue(() => StartEncoding(_vlog, dashDirectory));
                                    //updaing with job id
                                    _vlog.JobId = jobId;
                                    _vlog.JobEndAt = DateTime.Now;
                                    await LogCrudService.UpdateAsync(_vlog);
                                }


                                tempencodingHolds=_vlog;
                            }
                            else
                            {
                                if (!videoLog.IsEncodingFinished)
                                {

                                    //TODO:: error init JobStorage.Current.GetConnection()
                                    //check job is running
                                    _jobStorageConnection = JobStorage.Current.GetConnection();
                                    JobData jobData = null;
                                    try
                                    {
                                        jobData = _jobStorageConnection.GetJobData(videoLog.JobId);
                                    }
                                    catch (Exception ex)
                                    {
                                        //string jobId = BackgroundJob.Enqueue(() => StartEncoding(videoLog, dashDirectory));
                                        ////updaing with job id
                                        //videoLog.JobId = jobId;
                                        //videoLog.JobEndAt = DateTime.Now;
                                        //await LogCrudService.UpdateAsync(videoLog);
                                       
                                    }

                                    if (jobData == null)
                                    {
                                        string jobId = BackgroundJob.Enqueue(() => StartEncoding(videoLog, dashDirectory));
                                        //updaing with job id
                                        videoLog.JobId = jobId;
                                        videoLog.JobEndAt = DateTime.Now;
                                        await LogCrudService.UpdateAsync(videoLog);
                                    }
                                    else
                                    {
                                        string stateName = jobData.State;
                                        if (stateName.ToLower() == "enqueued" || stateName.ToLower() == "awating" ||
                                            stateName.ToLower() == "processing" || stateName.ToLower() == "succeeded")
                                        {
                                            //do nothing
                                        }
                                        else if (stateName.ToLower() == "scheduled")
                                        { //case dropped error mismatched orders job

                                        }
                                        else
                                        {

                                            BackgroundJob.Requeue(videoLog.JobId);
                                        }
                                    }
                                    //if not restart again
                                }

                            }

                            counter++;
                        }

                    }


                }


                return "";
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<bool> SaveEncodingStepsAsync(List<EncodingOutputStep> steps)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                foreach (var step in steps)
                {
                    await
                        db.ExecuteAsync(
                            "Update dbo.EncodingOutputStep set [Order]=@Order Where EncodingOutputStepId=@EncodingOutputStepId",
                            new { step.EncodingOutputStepId, step.Order }
                        );
                }

                return true;

            }
        }

        public async Task<DashVideoSetting> GetServerVideoSettingsAsnc(string machineName)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();

                return await
                    db.QuerySingleOrDefaultAsync<DashVideoSetting>(
                        "select ds.*,s.MachineName from [dbo].[DashVideoSetting] as ds " +
                        " inner join dbo.Server as s on s.ServerId = ds.ServerId  and ServerType = 'file'" +
                        " where s.MachineName = @MachineName",
                        new { MachineName = machineName }
                    );

            }
        }

        public async Task<string> StartEncoding(EncodingVideoLog eVlog, string dashOutDir = "")
        {
            try
            {



                if (eVlog.Flag.Trim().ToLower() == "d")
                {
                    //var packageInfo = new DashPackagingParams();
                    //packageInfo.
                    ////dash packaging
                    //KachuwaDashEncoder.Create(DASHTool.Mp4Dash)
                    //    .WithPlainCommands(packageInfo).Execute();
                    var allvideos = await GetDashReadyFiles(eVlog.VideoId);
                    BackgroundJob.Enqueue(() => StartDashPackaging(allvideos, dashOutDir, eVlog.EncodingVideoLogId, "d"));

                }
                else if (eVlog.Flag.Trim().ToLower() == "d&h")
                {
                    //var packageInfo = new DashPackagingParams();
                    //packageInfo.
                    ////dash packaging
                    //KachuwaDashEncoder.Create(DASHTool.Mp4Dash)
                    //    .WithPlainCommands(packageInfo).Execute();
                    var allvideos = await GetDashReadyFiles(eVlog.VideoId);
                    BackgroundJob.Enqueue(() => StartDashPackaging(allvideos, dashOutDir, eVlog.EncodingVideoLogId, "d&h"));

                }
                else if (eVlog.Flag.Trim().ToLower() == "f")
                {


                    //fragmentation
                    var packageInfo = new DashPackagingParams();
                    packageInfo.InputFile = eVlog.InputVideo;
                    packageInfo.OutputFile = eVlog.OutputVideo;
                    packageInfo.Commands = eVlog.Commands;
                    packageInfo.DashTool = DASHTool.Mp4Fragment;
                    //dash packaging
                    KachuwaDashEncoder.Create(DASHTool.Mp4Fragment)
                        .WithPlainCommands(packageInfo).Execute(eVlog.EncodingVideoLogId);

                }
                else if (eVlog.Flag.Trim().ToLower() == "e")
                {
                    // -i {Input} -vf "movie=\'{Logo}\' [watermark]; [in][watermark] overlay=main_w-overlay_w-10:main_h-overlay_h-10 [out]"  -c:v libvpx-vp9 -crf 23 -b:v 2M -maxrate 2M -r 28 -b:a 128k -c:a libopus -ac 1 -s 1920x1080  D:\OK\OnlineKachhya_Dev\SampleVideo\BinodTamang\CourseName\X1080P.webm
                    //encoding
                    //fragmentation
                    var packageInfo = new EncodingParams();
                    packageInfo.InputFile = eVlog.InputVideo;
                    packageInfo.OutputFile = eVlog.OutputVideo;
                    packageInfo.Commands = eVlog.Commands;
                    packageInfo.LogoFile = eVlog.UsedLogo;

                    UpdateConvertedVideoPath(eVlog.VideoId, Path.GetDirectoryName(eVlog.OutputVideo));
                    //dash packaging
                    Encoder.Create(this)
                        .WithPlainCommands(packageInfo).Execute(eVlog.EncodingVideoLogId);
                }
                else
                {
                    return "no valid flag";
                }
                return "ok";
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw;
            }
        }



        public async Task<string> StartDashPackaging(IEnumerable<EncodingVideoLog> eVlogsEncodingVideoLogs, string outputDir, long vlogId = 0, string flag = "d")
        {
            try
            {
                var dashStep = eVlogsEncodingVideoLogs.SingleOrDefault(e => e.Flag == flag);
                var fragmentedFiles = eVlogsEncodingVideoLogs.Where(e => e.Flag == "f").OrderBy(x=>x.EncodingOutputStepId).ToList();
                string[] inputVideos = fragmentedFiles.Select(x => x.OutputVideo).ToArray();
                //fragmentation
                var packageInfo = new DashPackagingParams();
                packageInfo.InputFiles = inputVideos;
                packageInfo.Commands = dashStep.Commands;
                if (!Directory.Exists(outputDir))
                {
                    Directory.CreateDirectory(outputDir);
                }

                packageInfo.OutputDir = outputDir;
                packageInfo.OutputFileName = dashStep.VideoId;
                if (flag == "d&h")
                {
                    UpdateDashVideoPath(dashStep.VideoId, $"{outputDir}\\{dashStep.VideoId}.mpd", $"{outputDir}\\{dashStep.VideoId}.m3u8");
                    packageInfo.DashTool = DASHTool.Mp4DashWithHLS;
                    //dash packaging
                    KachuwaDashEncoder.Create(DASHTool.Mp4DashWithHLS)
                        .WithPlainCommands(packageInfo).Execute(vlogId);
                }
                else if (flag == "h")
                {
                    //UpdateDashVideoPath(dashStep.VideoId, $"{outputDir}\\{dashStep.VideoId}.mpd", null);
                    //packageInfo.DashTool = DASHTool.Mp4Dash;
                    ////dash packaging
                    //KachuwaDashEncoder.Create(DASHTool.Mp4Dash)
                    //    .WithPlainCommands(packageInfo).Execute(vlogId);
                }
                else
                {
                    UpdateDashVideoPath(dashStep.VideoId, $"{outputDir}\\{dashStep.VideoId}.mpd", null);
                    packageInfo.DashTool = DASHTool.Mp4Dash;
                    //dash packaging
                    KachuwaDashEncoder.Create(DASHTool.Mp4Dash)
                        .WithPlainCommands(packageInfo).Execute(vlogId);
                }

                return "ok";
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw;
            }

        }

        public void MarkAsDashReady(long videoLogId, bool isReady)
        {
            var log = EncodingVideoLogCrudService.Get(videoLogId);
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.Video Set IsDashReady=@isReady" +
                    " where VideoId = @VideoId",
                    new { VideoId = log.VideoId, isReady }
                );

            }
        }
        public void UpdateConvertedVideoPath(string videoId, string convertedFilePath)
        {
            // var log = EncodingVideoLogCrudService.Get("Where VideoId=@VideoId",new{ VideoId=videoId });
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.Video Set CovertedFiles=@convertedFilePath" +
                    " where VideoId = @VideoId",
                    new { VideoId = videoId, convertedFilePath }
                );

            }
        }
        public void UpdateDashVideoPath(string videoId, string dashPath, string hlsPath = null)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.Video Set DashVideoPath=@dashPath,HLSVideoPath=@hlsPath" +
                    " where VideoId = @VideoId",
                    new { VideoId = videoId, dashPath, hlsPath }
                );

            }
        }

        public void LogEncodingError(long vlogId, string errorMessage)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.EncodingVideoLog Set ErrorMessage=@ErrorMessage" +
                    " where EncodingVideoLogId = @EncodingVideoLogId",
                    new { EncodingVideoLogId = vlogId, ErrorMessage = errorMessage }
                );

            }
        }

        public void LogEncodingStart(long vlogId, int processId)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.EncodingVideoLog Set EncodingProcessId=@EncodingProcessId,EncodingStartAt=@EncodingStartAt" +
                    " where EncodingVideoLogId = @EncodingVideoLogId",
                    new { EncodingVideoLogId = vlogId, EncodingProcessId = processId, EncodingStartAt = DateTime.Now }
                );

            }
        }

        public void LogEncodingEnd(long vlogId, string message, string error)
        {try
            {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();

                db.Execute(
                    "Update dbo.EncodingVideoLog Set Response=@Response,EncodingEndAt=@EncodingEndAt,IsEncodingFinished=@IsEncodingFinished" +
                    " where EncodingVideoLogId = @EncodingVideoLogId",
                    new { EncodingVideoLogId = vlogId, Response = message, IsEncodingFinished = true, EncodingEndAt = DateTime.Now }
                );

            }
			 }
            catch (Exception e)
            {

            }
        }

        public void LogToFile(string @params)
        {
            try
            {
                if (_logger == null)
                {
                    _logger = ContextResolver.Context.RequestServices.GetService<ILogger>();
                }
                _logger.Log(LogType.Info, () => @params);
            }
            catch (Exception e)
            {

            }
        }


        public void CheckPreviousJobsAndRequee(long vlogId)
        {
            try
            {
                var log = EncodingVideoLogCrudService.Get(vlogId);
                var step = OutputStepCrudService.Get(log.EncodingOutputStepId);
                if (step.WaitPreviousStep)
                {
                    _jobStorageConnection = JobStorage.Current.GetConnection();
                    JobData jobData = null;
                    try
                    {
                        jobData = _jobStorageConnection.GetJobData(log.ParentJobId);
                        if (jobData == null)
                        { //temporary fix existed data

                          int parentId=  int.Parse(log.JobId) - 1;
                            jobData = _jobStorageConnection.GetJobData(parentId.ToString());
                            if (jobData != null)
                            {
                                string stateName = jobData.State;
                                if (stateName.ToLower() == "succeeded")
                                {
                                    var parentLog = EncodingVideoLogCrudService.Get("Where JobId=@JobId",
                                        new { JobId = parentId });
                                    if (parentLog.IsEncodingFinished)
                                    {
                                        //if job finished and still file is missing
                                        if (!File.Exists(parentLog.OutputVideo))
                                        {
                                            parentLog.IsEncodingFinished = false;
                                            EncodingVideoLogCrudService.Update(parentLog);
                                            BackgroundJob.Requeue(parentLog.JobId);
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            string stateName = jobData.State;
                            if (stateName.ToLower() == "succeeded")
                            {
                                var parentLog = EncodingVideoLogCrudService.Get("Where JobId=@JobId",
                                    new {JobId = log.ParentJobId});
                                if (parentLog.IsEncodingFinished)
                                {
                                    //if job finished and still file is missing
                                    if (!File.Exists(parentLog.OutputVideo))
                                    {
                                        parentLog.IsEncodingFinished = false;
                                        EncodingVideoLogCrudService.Update(parentLog);
                                        BackgroundJob.Requeue(parentLog.JobId);
                                    }
                                }
                            }
                            else if (stateName.ToLower() == "enqueued" || stateName.ToLower() == "awating"||
                                     stateName.ToLower() == "processing" || stateName.ToLower() == "scheduled")
                            {

                            }
                        }

                    }
                    catch (Exception e)
                    {
                        
                    }
                }
            }
            catch (Exception e)
            {
                if (_logger == null)
                {
                    _logger = ContextResolver.Context.RequestServices.GetService<ILogger>();
                }
                _logger.Log(LogType.Info, () => e.Message, e);
              //  throw e;
            }
        }

        public void DeleteConvertedFiles(long vlogId)
        {
            try
            {
                var log = EncodingVideoLogCrudService.Get(vlogId);
                var video = VideoCrudService.Get("Where VideoId=@VideoId", new {VideoId = log.VideoId});
                if (Directory.Exists(video.CovertedFiles))
                {
                    Directory.Delete(video.CovertedFiles, true);
                }
            }
            catch (Exception e)
            {
                if (_logger == null)
                {
                    _logger = ContextResolver.Context.RequestServices.GetService<ILogger>();
                }
                _logger.Log(LogType.Info, () => e.Message, e);
               
            }
        }

        public VideoFileInfo GetVideoInfo(string inputFile)
        {
            try
            {

                return Processor.GetVideoInfo(inputFile);
            }
            catch (Exception e)
            {
                if (_logger == null)
                {
                    _logger = ContextResolver.Context.RequestServices.GetService<ILogger>();
                }
                _logger.Log(LogType.Info, () => e.Message, e);
                throw e;
            }
          
        }
    }
}
