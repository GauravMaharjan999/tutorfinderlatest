using System;
using System.Collections.Generic;
using System.Text;
using Kachuwa.Data.Crud.Attribute;

namespace Kachuwa.Banner
{
    public class BannerKeyViewModel:BannerKey
    {
        public int TotalBanners { get; set; }

    }
    public class BannerViewModel : BannerInfo
    {
        [IgnoreAll]
        public int ImageHeight { get; set; }
        [IgnoreAll]
        public int ImageWidth { get; set; }

    }
}
