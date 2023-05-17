using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Dash.Model;
using Kachuwa.Data;

namespace Kachuwa.Dash.Services
{
    public interface IServerService
    {
        CrudService<Server> CrudService { get; set; }
        CrudService<ServerStorage> ServerStorageService { get; set; }
        Task<ServerStorage> GetCurrentStorageSettingsOfServer(string machineName);
        Task<ServerStorage> GetCurrentStorageSettingsOfServer(string machineName, string serverType);
    }
}