using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Kachuwa.Log;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Kachuwa.IO;
using Kachuwa.Storage;
using Microsoft.AspNetCore.Authorization;
using Kachuwa.Data.Extension;

namespace Kachuwa.Banner.API
{
    [Route("api/v1/banner")]
    [AllowAnonymous]
    public class BannerApiController : BaseApiController
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IStorageProvider _storageProvider;
        public IBannerService BannerService { get; private set; }
        public ILogger Logger { get; private set; }

        public BannerApiController(IBannerService bannerService, IHostingEnvironment hostingEnvironment, ILogger logger
        ,IStorageProvider storageProvider
        )
        {
            _hostingEnvironment = hostingEnvironment;
            _storageProvider = storageProvider;
            BannerService = bannerService;
            Logger = logger;
        }


        [HttpPost]
        [Route("upload")]

        public async Task<dynamic> UploadFile(IFormFile MyFile)
        {

            string relativePath = await _storageProvider.Save(MyFile);
            string physicalfilepath = Path.Combine(_hostingEnvironment.WebRootPath, relativePath.TrimStart('/').Replace(@"/", "\\"));

            
            int w = 0, h = 0;
            System.Drawing.Image img = System.Drawing.Image.FromFile(physicalfilepath);

            w = img.Width;
            h = img.Height;
            img.Dispose();

            return HttpResponse(200, "", new { isUploaded = true, filepath = relativePath, w, h });
            // return Json();
        }



        public void CopyStream(Stream stream, string destPath)
        {
            using (var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write))
            {
                stream.CopyTo(fileStream);
            }
        }


        [HttpPost]
        [Route("key/save")]
        public async Task<dynamic> SaveBannerKey(BannerKey model)
        {
            try
            {
                model.AutoFill();
                model.IsActive = true;
                var data = BannerService.KeyCrudService.Insert(model);
                //return Json(new { Code = 200, Data = data });
                return HttpResponse(200, "", data);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("keys")]
        public async Task<dynamic> GetBannerKeys()
        {
            try
            {
                var condition = "where IsDeleted = @deleted";
                var data = await BannerService.KeyCrudService.GetListAsync(condition, new { deleted = false });
                return HttpResponse(200, "", data);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("save")]
        public async Task<dynamic> Save(BannerViewModel model)
        {
            try
            {
                if (model.BannerId == 0)
                {
                    model.AutoFill();
                    model.IsActive = true;
                    await BannerService.BannerCrudService.InsertAsync<int>(model);
                    await BannerService.SaveBannerSetting(model.KeyId, model.ImageWidth, model.ImageHeight);
                    //  return Json(new { Code = 200, Data = data, IsUpdated = false });
                    return HttpResponse(200, "", true);
                }
                else
                {
                    model.AutoFill();
                    await BannerService.BannerCrudService.UpdateAsync(model);
                    // return Json(new { Code = 200, Data = data, IsUpdated = true });
                    return HttpResponse(200, "", true);
                }
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message);
            }
        }


        [HttpPost]
        [Route("crop")]
        public async Task<dynamic> CropImage(CropModel model)
        {
            if (string.IsNullOrEmpty(model.imagePath)
                || !model.cropPointX.HasValue
                || !model.cropPointY.HasValue
                || !model.imageCropWidth.HasValue
                || !model.imageCropHeight.HasValue)
            {
                return HttpResponse((int)HttpStatusCode.NotImplemented, "Invalid parameters");
            }

            string existingfilepath = _hostingEnvironment.WebRootPath + model.imagePath.Replace("/", "\\");
            byte[] imageBytes = System.IO.File.ReadAllBytes(existingfilepath);
            byte[] croppedImage = ImageHelper.CropImage(imageBytes, model.cropPointX.Value, model.cropPointY.Value, model.imageCropWidth.Value, model.imageCropHeight.Value);

            string tempFolderName = Path.Combine(_hostingEnvironment.WebRootPath, "banner");

            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(model.imagePath);
            string fileName = Path.GetFileName(existingfilepath).Replace(fileNameWithoutExtension, fileNameWithoutExtension + "_cropped");
            string newfilePath = Path.Combine(tempFolderName, fileName);
            try
            {
                FileHelper.SaveFile(croppedImage, newfilePath);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message);
                //Log an error     
                return HttpResponse(500, ex.Message);
            }

            string filepath = newfilePath.Replace(_hostingEnvironment.WebRootPath, "").Replace("\\", "/");
            return HttpResponse(200, "", filepath);
        }



        [HttpPost]
        [Route("image/delete")]
        public async Task<dynamic> DeleteBannerItem(BannerInfo model)
        {
            try
            {
                await BannerService.BannerCrudService.DeleteAsync(model.BannerId);
                return HttpResponse(200, "", true);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message);
            }
        }



        [HttpPost]
        [Route("delete")]
        public  async Task<dynamic>  DeleteBanner(BannerKey model)
        {
            try
            {
               await  BannerService.BannerCrudService.DeleteAsync("Where KeyId=@KeyId",new{ KeyId=model.BannerKeyId});
                await BannerService.KeyCrudService.DeleteAsync(model.BannerKeyId);
                return HttpResponse(200, "", true);
            }
            catch (Exception ex)
            {
                Logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message);
            }

        }


        [HttpPost]
        [Route("preview")]
        public async Task<dynamic> GetBannerPreview(BannerKey model)
        {
            var data = await BannerService.BannerCrudService.GetListAsync("Where KeyId=@KeyId and IsActive=@IsActive and IsDelete=@IsDeleted;",new{KeyId=model.BannerKeyId,IsActive=true,IsDeleted=false});
            return HttpResponse(200, "", data);
        }

    }
}
