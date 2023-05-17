using Kachuwa.Caching;
using Kachuwa.Dash.Services;
using Kachuwa.Log;
using Kachuwa.Storage;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Kachuwa.KLiveApp.Controllers
{
    public class FileController : Controller
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStorageProvider _storageProvider;
        private readonly IStreamingService _streamingService;
        private readonly ILogger _logger;

        private readonly ICacheService _cacheService;
        private readonly IServerService _serverService;

        public FileController(IHostingEnvironment hostingEnvironment, IStorageProvider storageProvider,
            IStreamingService streamingService, ILogger logger, ICacheService cacheService, IServerService serverService)
        {
            _hostingEnvironment = hostingEnvironment;
            _storageProvider = storageProvider;
            _streamingService = streamingService;
            _logger = logger;
            _cacheService = cacheService;
            _serverService = serverService;
        }

        [Route("{videoId}/hls")]
        public async Task GetHlsManifest(string videoId)
        {

            try
            {

                var storage = await _serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName);
                var video = await _streamingService.LiveStreamService.GetAsync("Where VideoId=@VideoId", new { VideoId = videoId });
                if (video == null)
                    throw new Exception("Invalid video");

                string path = Path.Combine(storage.RootDirectory, videoId, videoId, $"index.m3u8");
                byte[] bytes = null;

                bytes = await _cacheService.GetAsync<byte[]>($"{videoId}.m3u8", () =>
                {
                    byte[] _bytes = null;
                    using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                    {
                        _bytes = ReadFully(stream);
                        stream.Close();
                    }

                    return Task.FromResult(_bytes);
                });


                var response = Response;
                response.ContentType = "vnd.apple.mpegURL";
                // response.ContentLength = session.FileInfo.FileSize;
                response.Headers["Content-Disposition"] = "attachment; fileName=" + videoId + "";
                using (var sw = new BinaryWriter(Response.Body))
                {
                    sw.Write(bytes);
                }
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message, e);
                throw e;
            }

        }
        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private string GetContentType(string fileExtension)
        {
            string contentType = "";
            new FileExtensionContentTypeProvider().TryGetContentType(fileExtension, out contentType);
            return contentType ?? "text/plain";
        }
        private void ProcessFile(string path, string videoId, string contentType)
        {
            // string path = Path.Combine(_hostingEnvironment.WebRootPath, "tero", $"{videoId}.mpd");
            if (System.IO.File.Exists(path))
            {
                byte[] bytes = null;
                using (Stream stream = System.IO.File.Open(path, FileMode.Open))
                {

                    //_stream.Close();
                    bytes = ReadFully(stream);
                    stream.Close();
                }

                var response = Response;

                response.ContentType = contentType;
                // response.ContentLength = session.FileInfo.FileSize;
                //response.Headers["Content-Disposition"] = "attachment; fileName=" + videoId + "";
                using (var sw = new BinaryWriter(Response.Body))
                {

                    sw.Write(bytes);

                }
            }
        }

        /////video/avc1/1/init.mp4?
        //[Route("{videoId}/video/{codec}/{quality}/init.mp4", Name = "VideoInit")]
        //public async Task GetVideoInitFile(string videoId, string codec, string quality, [FromQuery]bool dash = true)
        //{
        //    var searchResult = await _luceneService.SearchVideoAsync(Environment.MachineName, "", videoId);

        //    if (searchResult == null)
        //    {
        //        throw new Exception("Invalid video");
        //    }
        //    else
        //    {
        //        if (searchResult.Count > 0)
        //        {
        //            var currentServerResult = searchResult.SingleOrDefault(x =>
        //                x.Server.ToLower() == Environment.MachineName.ToLower());
        //            if (currentServerResult != null)
        //            {
        //                string path = Path.Combine(currentServerResult.Path, "video", codec, quality,
        //                    $"init.mp4");
        //                if (System.IO.File.Exists(path))
        //                {

        //                    ProcessFile(path, videoId, GetContentType(".mp4"));
        //                }
        //                else
        //                {
        //                    throw new Exception("Invalid");
        //                }
        //            }
        //        }
        //    }

        //}
        [Route("{videoId}/{quality}/manifest.m3u8", Name = "HlsVideoInit")]
        public async Task GetHlsVideoInitFile(string videoId, string codec, string quality, [FromQuery]bool dash = true)
        {
            var storage = await _serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName);

            string path = Path.Combine(storage.RootDirectory, videoId, videoId, quality, $"manifest.m3u8");
            if (System.IO.File.Exists(path))
            {
                ProcessFile(path, videoId, GetContentType(".m3u8"));
            }
            else
            {
                throw new Exception("Invalid");
            }




        }
        ///stream/video/avc1/1/seg-1.m4s?
        [Route("{videoId}/{quality}/{segment}", Name = "VideoSegment")]
        public async Task GetVideoSegment(string videoId, string codec, string quality, string segment, [FromQuery]bool dash = true)
        {

            var storage = await _serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName);

            string path = Path.Combine(storage.RootDirectory, videoId, videoId, quality, segment);
            if (System.IO.File.Exists(path))
            {
                ProcessFile(path, videoId, GetContentType(Path.GetExtension(segment)));
            }
            else
            {
                throw new Exception("Invalid");
            }




        }
        /////stream/video/avc1/1/init.mp4?customQuery=value
        //[Route("{videoId}/audio/{lang}/{codec}/init.mp4", Name = "AudioInit")]
        //public async Task GetAudioInitFile(string videoId, string codec, string lang, [FromQuery]bool dash = true)
        //{
        //    var searchResult = await _luceneService.SearchVideoAsync(Environment.MachineName, "", videoId);

        //    if (searchResult == null)
        //    {
        //        throw new Exception("Invalid video");
        //    }
        //    else
        //    {
        //        if (searchResult.Count > 0)
        //        {
        //            var currentServerResult = searchResult.SingleOrDefault(x =>
        //                x.Server.ToLower() == Environment.MachineName.ToLower());
        //            if (currentServerResult != null)
        //            {
        //                string path = Path.Combine(currentServerResult.Path, "audio", lang, codec, $"init.mp4");
        //                if (System.IO.File.Exists(path))
        //                {
        //                    ProcessFile(path, videoId, GetContentType(".mp4"));
        //                }
        //                else
        //                {
        //                    throw new Exception("Invalid");
        //                }
        //            }
        //        }
        //    }

        //}
        //[Route("{videoId}/audio/{lang}/{codec}/media.m3u8", Name = "HlsAudioInit")]
        //public async Task GetHlsAudioInitFile(string videoId, string codec, string lang, [FromQuery]bool dash = true)
        //{
        //    var searchResult = await _luceneService.SearchVideoAsync(Environment.MachineName, "", videoId);

        //    if (searchResult == null)
        //    {
        //        throw new Exception("Invalid video");
        //    }
        //    else
        //    {
        //        if (searchResult.Count > 0)
        //        {
        //            var currentServerResult = searchResult.SingleOrDefault(x =>
        //                x.Server.ToLower() == Environment.MachineName.ToLower());
        //            if (currentServerResult != null)
        //            {
        //                string path = Path.Combine(currentServerResult.Path, "audio", lang, codec, $"media.m3u8");
        //                if (System.IO.File.Exists(path))
        //                {
        //                    ProcessFile(path, videoId, GetContentType(".m3u8"));
        //                }
        //                else
        //                {
        //                    throw new Exception("Invalid");
        //                }
        //            }
        //        }
        //    }


        //}

        ////stream/audio/en/mp4a/seg-1.m4s
        //[Route("{videoId}/audio/{lang}/{codec}/{segment}", Name = "AudioSegment")]
        //public async Task GetAudioSegment(string videoId, string codec, string lang, string segment, [FromQuery]bool dash = true)
        //{
        //    var searchResult = await _luceneService.SearchVideoAsync(Environment.MachineName, "", videoId);

        //    if (searchResult == null)
        //    {
        //        throw new Exception("Invalid video");
        //    }
        //    else
        //    {
        //        if (searchResult.Count > 0)
        //        {
        //            var currentServerResult = searchResult.SingleOrDefault(x =>
        //                x.Server.ToLower() == Environment.MachineName.ToLower());
        //            if (currentServerResult != null)
        //            {
        //                string path = Path.Combine(currentServerResult.Path, "audio", lang, codec, segment);
        //                if (System.IO.File.Exists(path))
        //                {
        //                    ProcessFile(path, videoId, GetContentType(Path.GetExtension(segment)));
        //                }
        //                else
        //                {
        //                    throw new Exception("Invalid");
        //                }
        //            }
        //        }
        //    }

        //}
    }
}