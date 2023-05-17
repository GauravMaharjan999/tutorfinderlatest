using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Data;

namespace Kachuwa.Banner
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerInfo>> GetBannersByKey(string key);
        CrudService<BannerInfo> BannerCrudService { get; set; }
        CrudService<BannerKey> KeyCrudService { get; set; }
        CrudService<BannerSetting> SettingCrudService { get; set; }

        Task<IEnumerable<BannerKeyViewModel>> GetBannersList(int page, int limit, string query);
        Task SaveBannerSetting(int modelKeyId, int modelImageWidth, int modelImageHeight);
    }
}