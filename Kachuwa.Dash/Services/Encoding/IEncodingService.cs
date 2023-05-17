using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Dash.Model;
using Kachuwa.Data;

namespace Kachuwa.Dash.Services
{
    public interface IEncodingService
    {

        CrudService<EncodingOutputFormat> OutputFormatCrudService { get; set; }
        CrudService<EncodingOutputStep> OutputStepCrudService { get; set; }
        CrudService<EncodingVideoLog> EncodingVideoLogCrudService { get; set; }
        CrudService<DashVideoSetting> DashVideoSettingService { get; set; }
        CrudService<Video> VideoCrudService { get; set; }
        CrudService<EncodingVideoLog> LogCrudService { get; set; }


        //return job id
        Task<string> EncodeAsnyc(string videoId);
        Task<bool> SaveEncodingStepsAsync(List<EncodingOutputStep> steps);
        Task<DashVideoSetting> GetServerVideoSettingsAsnc(string machineName);
       void LogToFile(string @params);
        void UpdateConvertedVideoPath(string videoId, string convertedFilePath);
        void UpdateDashVideoPath(string videoId, string dashPath, string hlsPath = null);
        void MarkAsDashReady(long videoLogId, bool isReady);
        VideoFileInfo GetVideoInfo(string inputFile);
    }
}