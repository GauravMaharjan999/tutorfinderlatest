using System;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using Kachuwa.Dash.Live;
using Kachuwa.Dash.Services;
using Kachuwa.Data;
using Kachuwa.Data.Extension;
using Kachuwa.Identity.Extensions;
using Kachuwa.Log;
using Kachuwa.Storage;
using Kachuwa.Training.Model;
using Kachuwa.Training.Service;
using Kachuwa.Web;
using Microsoft.AspNetCore.Mvc;

namespace Kachuwa.KLiveApp.Controllers
{
    public class StreamController : BaseController
    {
        private readonly IStreamingService _streamingService;
        private readonly IStorageProvider _storageProvider;
        private readonly ICourseTimingStreamMapping _courseTimingStreamMapping;
        private readonly ILogger _logger;
        private readonly IServerService _serverService;

        public StreamController(IStreamingService streamingService,IStorageProvider storageProvider , ICourseTimingStreamMapping courseTimingStreamMapping,ILogger logger,IServerService serverService)
        {
            _streamingService = streamingService;
            _storageProvider = storageProvider;
            _courseTimingStreamMapping = courseTimingStreamMapping;
            _logger = logger;
            _serverService = serverService;
        }
        [HttpPost]
        public async Task<IActionResult> Create(NewStreamViewModel model)
        {
            try
            {
                var storage = await _serverService.GetCurrentStorageSettingsOfServer(Environment.MachineName);

                model.VideoId = Guid.NewGuid().ToString("N");
                model.StreamKey = Guid.NewGuid().ToString("N");



                if (ModelState.IsValid)
                {
                    model.AutoFill();

                    CourseTimingStreamMapping model1 = new CourseTimingStreamMapping();
                    if (model.ChooseLiveForWhat == 1)//if courseTiming is Chosen
                    {
                        model1.ReferenceId = model.CourseTimingId;
                        model1.Type = "C";
                    }
                    else if (model.ChooseLiveForWhat == 2)//if event is chosen
                    {
                        model1.ReferenceId = model.EventId;
                        model1.Type = "E";
                    }
                    else
                    {
                        //no choosen of what to live
                        return RedirectToRoute("/user/live/create", new { model });
                    }
                    if (model.CoverImageFile != null)
                    {
                        model.CoverImage = await _storageProvider.Save("stream", model.CoverImageFile);
                    }
                    model.RTMP = storage.RTMPAddress +'/'+model.StreamKey;
                    //model.RTMP = $"rtpm://202.51.74.238/living/{ model.StreamKey}";


                    var dbFactory = DbFactoryProvider.GetFactory();
                    using (var db = (DbConnection)dbFactory.GetConnection())
                    {
                        await db.OpenAsync();
                        using (var tran = db.BeginTransaction())
                        {
                            try
                            {
                                model.StreamedBy = User.Identity.GetIdentityUserId();
                                var streamId = await _streamingService.LiveStreamService.InsertAsync<int>(db, model, tran, null);
                                model1.StreamId = streamId;
                                var status = await _courseTimingStreamMapping.CourseTimingStreamMappingCrudService.InsertAsync<int>(db, model1, tran, null);

                                tran.Commit();
                                return Redirect($"/user/live/{streamId}");
                            }

                            catch (Exception ex)
                            {
                                _logger.Log(LogType.Error, () => ex.Message, ex);
                                tran.Rollback();
                                return RedirectToRoute("/user/live/create", new { model });
                            }
                        }

                    }

                }

                else
                {
                    var d = ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList();
                    return RedirectToRoute("/user/live/create", new { model });
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }
        }
    }
}