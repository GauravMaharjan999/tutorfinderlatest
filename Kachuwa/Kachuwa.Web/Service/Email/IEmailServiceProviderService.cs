using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Data;
using Kachuwa.Log;

namespace Kachuwa.Web.Services
{
    public interface IEmailServiceProviderService
    {
        CrudService<EmailLog> LogCrudService { get; set; }
        CrudService<EmailServiceProvider> ProviderCrudService { get; set; }
        CrudService<EmailServiceProviderSetting> SettingCrudService { get; set; }
        Task<IEmailSender> GetDefaultEmailSender();
        Task<IEnumerable<EmailServiceProviderSetting>> GetSettings(int emailServiceProviderId);
        T GetSettings<T>(int emailServiceProviderId) where T : class;
        Task<bool> SaveSetting<T>(T setting, int emailServiceProviderId);
        Task<bool> SetDefaultProviderAsync(int id);
        Task<EmailServiceProvider> GetDefaultProviderAsync();
        Task<bool> UpdateSettings(List<EmailServiceProviderSetting> settings);
        Task<bool> UpdateStatus(EmailServiceProvider model);
    }
}