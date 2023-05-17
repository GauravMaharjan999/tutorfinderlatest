using System.Collections.Generic;
using System.Reflection;
using Kachuwa.Web.Module;

namespace Kachuwa.Banner
{
    public class BannerModule : IModule
    {
        public string Name { get; set; } = "Banner";
        public string Version { get; set; } = "1.0.0.0";
        public List<string> SupportedVersions { get; set; } = new List<string>() { "1.0.0" };
        public string Author { get; set; } = "Binod tamang";
        public Assembly Assembly { get; set; } = typeof(BannerModule).GetTypeInfo().Assembly;
        public bool IsInstalled { get; set; } = true;
        public bool RequireSettingComponent { get; set; } = true;

        public string ModuleSettingComponent { get; set; } = "BannerSetting";
    }
}