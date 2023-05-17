using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Kachuwa.Dash.Live;
using Kachuwa.Dash.Model;
using Kachuwa.Data;
using Stream = Kachuwa.Dash.Live.Stream;

namespace Kachuwa.Dash.Services
{
    public class StreamingService : IStreamingService
    {
        public CrudService<Stream> LiveStreamService { get; set; } = new CrudService<Stream>();
        public CrudService<StreamComment> CommentService { get; set; } = new CrudService<StreamComment>();

        public CrudService<LiveChat> LiveChatService { get; set; } = new CrudService<LiveChat>();

        public CrudService<UserAudio> AudioService { get; set; } = new CrudService<UserAudio>();
        public CrudService<StreamStatus> StreamStatusService { get; set; } = new CrudService<StreamStatus>();

        public CrudService<UserStreamSetting> StreamSettingService { get; set; } = new CrudService<UserStreamSetting>();

        public CrudService<LiveEncodingFormat> EncodingFormatService { get; set; } = new CrudService<LiveEncodingFormat>();

        public CrudService<LiveEncoding> EncodingService { get; set; } = new CrudService<LiveEncoding>();
        
        public string GetVideoIdByStreamKey(string streamKey)
        {

            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();
                var videoId = db.QueryFirstOrDefault<string>("select VideoId from dbo.Stream where StreamKey=@StreamKey ", new { StreamKey = streamKey });
                return videoId;
            }
        }


        public bool CheckStreamExists(string publishingName)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                db.Open();
                var status = db.ExecuteScalar<int>("select 1 from dbo.Stream where StreamKey=@StreamKey and IsFinished=0 ", new { StreamKey = publishingName });
                return status == 1;
            }
        }

        public async Task<UserStreamSettingViewModel> GetUserStreamingSetting(long userId)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                var encoding = await db.QuerySingleOrDefaultAsync<LiveEncodingViewModel>(" Select l.*,u.LiveEncodingFormatIds from dbo.LiveEncoding as l inner join dbo.UserStreamSetting as u on u.LiveEncodingId=l.LiveEncodingId " +
                                                                   " where u.UserId=@UserId ", new { UserId = userId });

                var formats = await db.QueryAsync<LiveEncodingFormat>("select * from dbo.LiveEncodingFormat where LiveEncodingFormatId in @IDs", new { Ids = encoding.LiveEncodingFormatIds.Split(",") });
                return new UserStreamSettingViewModel
                {
                    Encoding = encoding,
                    Formats = formats.ToList()
                };
            }
        }

        public async Task<UserStreamSettingViewModel> GetStreamingSettings(string streamKey)
        {
            var streamInfo = LiveStreamService.Get("Where StreamKey=@streamKey", new { streamKey });
            return await GetUserStreamingSetting(streamInfo.StreamedBy);

        }
        public void LogEncodingStart(long streamId, int pId)
        {

        }

        public void LogEncodingEnd(long streamId, string message, string s)
        {

        }

        public void LogEncodingError(long streamId, string exMessage)
        {

        }

        public void LogToFile(string @params)
        {

        }

        internal void LogEncodingError(string streamKey, string message)
        {

        }

        internal void LogEncodingEnd(string streamKey, string message1, string message2)
        {

        }

        internal void LogEncodingStart(string streamKey, int id)
        {

        }

        public async Task<IEnumerable<LiveStreamViewModel>> GetLatestLiveStreams(string query, int page, int limit)
        {
            var dbFactory = DbFactoryProvider.GetFactory();
            using (var db = (DbConnection)dbFactory.GetConnection())
            {
                await db.OpenAsync();
                return await db.QueryAsync<LiveStreamViewModel>("select * from dbo.Stream where isonair=1 and IsFinished=0 and title like @Query order by OnAiredAt desc", new { Query = "%" + query + "%", page, limit });

            }
        }

        public async Task<IEnumerable<LiveStreamDetailViewModel>> GetAllCoursesLiveStream()
        {
            try
            {
                string sql = @"  select TC.Id as CourseId,TC.Name as CourseName,TC.ShortDescription as CourseShortDescription ,TC.ProfileImagePath as ProfileImagePath,TT.Name as TutorName,CTS.*,S.*  from CourseTimingStreamMapping CTS
                                        left join Stream S on S.StreamId =CTS.StreamId
                                        left join TrainingCourseTiming Ct on ct.id = CTS.ReferenceId
                                        left join TrainingCourse TC on TC.Id = Ct.CourseId
                                        left join TrainingCourseTutor TCT on TCT.CourseId = Ct.CourseId
                                        left Join TrainingTutor TT on TT.Id = TCT.TutorId

                                        where S.IsOnAir = @isOnAir and S.IsFinished = @isFinished and CTS.Type='C'
                                        order by s.OnAiredAt desc
                                       ";

                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (DbConnection)dbFactory.GetConnection())
                {
                    await db.OpenAsync();
                    return await db.QueryAsync<LiveStreamDetailViewModel>(sql, new { @isOnAir = true, @isFinished = false });

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
        public async Task<IEnumerable<LiveEventsDetailViewModel>> GetAllEventsLiveStream()
        {
            try
            {
                string sql = @"  select E.Id as EventId ,E.Title as EventTitle,E.ShortDescription as EventShortDescription,E.ProfileImagePath as ProfileImagePath,CTS.*,S.* from CourseTimingStreamMapping CTS
                                         left join Stream S on S.StreamId =CTS.StreamId
                                         left join Event E on E.Id =CTS.ReferenceId
                                         where S.IsOnAir = @isOnAir and S.IsFinished = @isFinished and CTS.Type='E'
                                        order by s.OnAiredAt desc
                                       ";

                var dbFactory = DbFactoryProvider.GetFactory();
                using (var db = (DbConnection)dbFactory.GetConnection())
                {
                    await db.OpenAsync();
                    return await db.QueryAsync<LiveEventsDetailViewModel>(sql, new { @isOnAir = true, @isFinished = false });

                }
            }
            catch (Exception ex)
            {

                throw ex;
            }

        }
    }
}
