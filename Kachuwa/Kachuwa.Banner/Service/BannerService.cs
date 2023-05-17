using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Kachuwa.Data;

namespace Kachuwa.Banner
{
    public class BannerService : IBannerService
    {
        public async Task<IEnumerable<BannerInfo>> GetBannersByKey(string key)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                return await db.QueryAsync<BannerInfo>(
                    "declare @imSufx nvarchar(25);select @imSufx = '?w=' + cast(b.ImageWidth as nvarchar) + '&h=' + cast(b.ImageHeight as nvarchar) + '&mode=crop' from dbo.BannerSetting as b left join dbo.BannerKey as bk on bk.BannerKeyId = b.KeyId" +
                    " Where bk.Name=@Key ;" +
                    "  select b.* , IsNull(@imSufx,'?w=1920&h=680&mode=crop') as Suffix from dbo.Banner as b inner join dbo.BannerKey as bk on bk.BannerKeyId=b.KeyId Where b.IsActive=@IsActive and b.IsDeleted=@IsDeleted and bk.Name=@Key",
                    new { Key = key, IsActive = true, IsDeleted = false });
            }
        }

        public CrudService<BannerInfo> BannerCrudService { get; set; } = new CrudService<BannerInfo>();
        public CrudService<BannerKey> KeyCrudService { get; set; } = new CrudService<BannerKey>();
        public CrudService<BannerSetting> SettingCrudService { get; set; } = new CrudService<BannerSetting>();
        public async Task<IEnumerable<BannerKeyViewModel>> GetBannersList(int page, int limit, string query)
        {
            var dbfactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbfactory.GetConnection())
            {
                string sql = @"SELECT COUNT(1) OVER() AS RowTotal,t.BannerKeyId,t.Name,t.AddedOn,t.AddedBy,t.IsActive,  count(c.BannerId) as TotalBanners 
                                FROM [dbo].[BannerKey] as t inner join
                                [dbo].[Banner] as c on c.KeyId = t.BannerKeyId
                                Where t.Name Like @Query and t.IsDeleted = 0 
                                group by t.BannerKeyId,t.Name,t.AddedOn,t.AddedBy,t.IsActive 
                                Order By t.AddedOn desc OFFSET (Cast(@Page-1 as int) * Cast(@RowPerPage as int))  ROWS FETCH NEXT @RowPerPage ROWS ONLY";
                await db.OpenAsync();
                return await db.QueryAsync<BannerKeyViewModel>(sql, new { Page = page, RowPerPage = limit, Query = "%" + query + "%" });
            }
        }

        public async Task SaveBannerSetting(int keyId, int imageWidth, int imageHeight)
        {
            var dbfactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbfactory.GetConnection())
            {
                string sql = @"If(exists(select 1 from dbo.BannerSetting Where KeyId=@KeyId))
                               BEGIN 
                                      Update dbo.BannerSetting Set ImageHeight=@ImageHeight,ImageWidth=@ImageWidth  Where KeyId=@KeyId
                               END 
                               ELSE 
                               BEGIN 
                                      Insert Into dbo.BannerSetting Select @KeyId,@ImageHeight,@ImageWidth
                               END ";
                await db.OpenAsync();
                await db.ExecuteAsync(sql, new { KeyId = keyId, ImageWidth  = imageWidth, ImageHeight = imageHeight });
            }
        }
    }
}
