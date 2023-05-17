using System;
using System.Threading.Tasks;
using Kachuwa.FileUploader;
using Kachuwa.FileUploader.Model;
using Kachuwa.Log;
using Kachuwa.Web.Module;
using Kachuwa.Web.Service;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.FileUploader.Components
{
    public class FileUploaderViewComponent : KachuwaModuleViewComponent<KachuwaFileUploaderModule>
    {
        private readonly ILogger _logger;


        public FileUploaderViewComponent(IModuleManager moduleManager,ILogger logger) : base(moduleManager)
        {
            _logger = logger;
        }
        //public async Task<IViewComponentResult> InvokeAsync()
        //{
        //    return View(new FileUploadSetting());

        //}
        public async Task<IViewComponentResult> InvokeAsync(FileUploadSetting setting=null)
        {
            setting = setting ?? new FileUploadSetting();
            return View(setting);

        }

        public override string DisplayName { get; } = "File Uploader";
        public override bool IsVisibleOnUI { get; } = true;
    }
}