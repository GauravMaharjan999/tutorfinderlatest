using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.FileUploader.Constants;
using Kachuwa.Log;
using Kachuwa.Storage;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Kachuwa.FileUploader.Api
{

    [Route("api/v1/fileuploader")]
    public class FileUploaderApiController : BaseApiController
    {


        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStorageProvider _storageProvider;
        public ILogger _logger { get; private set; }

        public FileUploaderApiController(IHostingEnvironment hostingEnvironment, ILogger logger, IStorageProvider storageProvider)
        {
            _hostingEnvironment = hostingEnvironment;
            _storageProvider = storageProvider;
            _logger = logger;
        }

        [HttpPost]
        [Route("upload")]
        [AllowAnonymous]
        public async Task<dynamic> UploadFile()
        {
            // try
            // {
            var files = Request.Form.Files;


            StringValues accepectedFile, dir;
            Request.Headers.TryGetValue(FUConstants.AcceptFiles, out accepectedFile);
            Request.Headers.TryGetValue(FUConstants.UploadDir, out dir);
         
            if (files == null || !files.Any())
            {
                // return ErrorResponse(new string[] { "Please upload files." });
                throw new Exception("Please upload files.");
            }

            if (!string.IsNullOrEmpty(accepectedFile))
            {
                var str_accepttypes = accepectedFile.ToString();
                string contentType = files.First().ContentType;
                var accepttypes = str_accepttypes.Split(",");
                if (str_accepttypes.Trim() != "*")
                {
                    if (!accepttypes.Any(x => x.Contains(contentType.ToLower())))
                    {
                        //return ErrorResponse(new string[] { "Unsupported files detected." });
                        throw new Exception("Unsupported files detected.");
                    }
                }
            }

            string uploadedPath = "";
            try
            {
                var file = files.First();
                uploadedPath = await _storageProvider.Save(string.IsNullOrEmpty(dir) ? "FU" : dir.ToString(), file);
            }
            catch (Exception ex)
            {
                _logger.Log(LogType.Error, () => ex.Message, ex);
                throw new Exception("File could not save.");
            }

            return HttpResponse(200, "",  uploadedPath);
            // }
            //            catch (Exception e)
            //            {
            //                _logger.Log(LogType.Error, () => e.Message.ToString(), e);
            //                return ErrorResponse(500, e.Message);
            //            }

        }


        [HttpPost]
        [Route("revert")]
        [AllowAnonymous]
        public async Task<dynamic> Remove()
        {
            try
            {
                string file = "";
                using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    file= await reader.ReadToEndAsync();
                }
                await _storageProvider.Delete(file);
                return HttpResponse(200, "", file);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message.ToString(), e);
                return ErrorResponse(500, e.Message);
            }

        }
        [HttpPost]
        [Route("remove")]
        [AllowAnonymous]
        public async Task<dynamic> RemoveFile(string file)
        {
            try
            {
               
                await _storageProvider.Delete(file);
                return HttpResponse(200, "", file);
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message.ToString(), e);
                return ErrorResponse(500, e.Message);
            }

        }
        [HttpGet]
        [Route("load")]
        [AllowAnonymous]
        public async Task<dynamic> FakeLoad()
        {
            try
            {

                return HttpResponse(200, "", "OK");
            }
            catch (Exception e)
            {
                _logger.Log(LogType.Error, () => e.Message.ToString(), e);
                return ErrorResponse(500, e.Message);
            }

        }
    }
}
