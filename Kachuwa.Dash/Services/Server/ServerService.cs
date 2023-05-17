using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Kachuwa.Dash.Model;
using Kachuwa.Data;
using Kachuwa.Data.Extension;

namespace Kachuwa.Dash.Services
{
    public class ServerService: IServerService
    {
        public CrudService<Server> CrudService { get; set; }=new CrudService<Server>();
        public CrudService<ServerStorage> ServerStorageService { get; set; } = new CrudService<ServerStorage>();
        public async  Task<ServerStorage> GetCurrentStorageSettingsOfServer(string machineName)
        {
            //CHANGED TO LIVE FILE SERVER
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();

                var storages= await
                    db.QueryAsync<ServerStorage>(
                        "select ds.*,s.RTMPAddress,s.MachineName from [dbo].[ServerStorage] as ds " +
                        " inner join dbo.Server as s on s.ServerId = ds.ServerId  and ServerType = 'LIVE'" +
                        " where s.MachineName = @MachineName order by UseOrder ",
                        new { MachineName = machineName }
                    );
                var activeStorage = storages.SingleOrDefault(x => x.IsActive == true);
             

                long remainingSpace = GetFreeSpace(activeStorage.RootDirectory);
                if (remainingSpace > activeStorage.MinimumSpace)
                {
                    return activeStorage;
                }
                else
                {
                    foreach (var storage in storages)
                    {
                        if (storage.IsActive == false)
                        {
                            long _nextDriveRemainingSpace =  GetFreeSpace(storage.RootDirectory);
                            if (_nextDriveRemainingSpace > storage.MinimumSpace)
                            {
                                activeStorage.IsActive = false;
                                activeStorage.AutoFill();
                                await ServerStorageService.UpdateAsync(activeStorage);

                                storage.IsActive = true;
                                storage.AutoFill();
                                await ServerStorageService.UpdateAsync(storage);
                                return storage;
                              
                            }
                        }
                    }
                }
               
                throw new Exception("No Valid Storage Drive Found!");
            }
        }

        public async Task<ServerStorage> GetCurrentStorageSettingsOfServer(string machineName, string serverType)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();

                var storages = await
                    db.QueryAsync<ServerStorage>(
                        "select ds.*,s.MachineName from [dbo].[ServerStorage] as ds " +
                        " inner join dbo.Server as s on s.ServerId = ds.ServerId  " +
                        " where s.MachineName = @MachineName and ServerType=@ServerType order by UseOrder ",
                        new { MachineName = machineName , ServerType=serverType }
                    );
                var activeStorage = storages.SingleOrDefault(x => x.IsActive == true);


                long remainingSpace = GetFreeSpace(activeStorage.RootDirectory);
                if (remainingSpace > activeStorage.MinimumSpace)
                {
                    return activeStorage;
                }
                else
                {
                    foreach (var storage in storages)
                    {
                        if (storage.IsActive == false)
                        {
                            long _nextDriveRemainingSpace = GetFreeSpace(storage.RootDirectory);
                            if (_nextDriveRemainingSpace > storage.MinimumSpace)
                            {
                                activeStorage.IsActive = false;
                                activeStorage.AutoFill();
                                await ServerStorageService.UpdateAsync(activeStorage);

                                storage.IsActive = true;
                                storage.AutoFill();
                                await ServerStorageService.UpdateAsync(storage);
                                return storage;

                            }
                        }
                    }
                }

                throw new Exception("No Valid Storage Drive Found!");
            }
        }

        private long GetFreeSpace(string path)
        {
            FileInfo file = new FileInfo(path);
            DriveInfo drive = new DriveInfo(file.FullName);

            long longAvailableFreeSpace = 0;
            longAvailableFreeSpace= drive.TotalFreeSpace;
            return longAvailableFreeSpace / 1024; //in gb


        }
    }
}
