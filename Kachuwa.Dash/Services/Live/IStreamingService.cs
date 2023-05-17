using System.Collections.Generic;
using System.Threading.Tasks;
using Kachuwa.Dash.Live;
using Kachuwa.Dash.Model;
using Kachuwa.Data;
using Stream = Kachuwa.Dash.Live.Stream;

namespace Kachuwa.Dash.Services
{
   

    public interface IStreamingService
    {
        CrudService<Stream> LiveStreamService { get; set; }
        CrudService<StreamComment> CommentService { get; set; }
        CrudService<LiveChat> LiveChatService { get; set; }
        CrudService<UserAudio> AudioService { get; set; }
        CrudService<StreamStatus> StreamStatusService { get; set; }
         string GetVideoIdByStreamKey(string streamKey);
        bool CheckStreamExists(string publishingName);

        Task<UserStreamSettingViewModel> GetUserStreamingSetting(long userId);
        Task<IEnumerable<LiveStreamViewModel>> GetLatestLiveStreams(string query, int page, int limit);

        Task<IEnumerable<LiveStreamDetailViewModel>> GetAllCoursesLiveStream();
        Task<IEnumerable<LiveEventsDetailViewModel>> GetAllEventsLiveStream();
    }
}