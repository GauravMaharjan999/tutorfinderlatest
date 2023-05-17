//using System;
//using System.Linq;
//using System.Threading.Tasks;
//using Kachuwa.Log;
//using Kachuwa.Web.API;
//using Microsoft.AspNetCore.Mvc;
//using OnlineKachhya.Lucene;

//namespace Api.V1.KachawaSchedular
//{
//    [Route("api/v1/lucene")]
//    public class LuceneApiController : BaseApiController
//    {
//        private readonly ILuceneService _luceneService;
//        private readonly ILogger _logger;

//        public LuceneApiController(ILuceneService luceneService, ILogger logger)
//        {
//            _luceneService = luceneService;
//            _logger = logger;
//        }
       
//        [Route("clear/video/index")]
//        public async Task<dynamic> ClearVideoIndex()
//        {
//            try
//            {
//                await _luceneService.ClearVideoIndexAsync(Environment.MachineName);
//                return HttpResponse(200, "success", true);
//            }
//            catch (Exception e)
//            {
//                _logger.Log(LogType.Error, () => e.Message, e);
//                return ErrorResponse(500, e.Message);
//            }
//        }

//        [Route("start/video/index")]
//        public async Task<dynamic> StartVideoIndexAsync()
//        {
//            try
//            {
//                await _luceneService.StartVideoIndexing(Environment.MachineName);
//                return HttpResponse(200, "success", true);
//            }
//            catch (Exception e)
//            {
//                _logger.Log(LogType.Error, () => e.Message, e);
//                return ErrorResponse(500, e.Message);
//            }
//        }
//        [Route("trigger/video/index/update")]
//        public async Task<dynamic> StartVideoIndexAsync(NewVideoAddedViewModel model)
//        {
//            try
//            {
//                if (ModelState.IsValid)
//                {

//                    await _luceneService.AddNewVideoPathAsync(model.Path, model.VideoId, model.MachineName);
//                    return HttpResponse(200, "success", true);
//                }
//                else
//                {
//                    return ValidationResponse(
//                        ModelState.Values.SelectMany(x => x.Errors).Select(e => e.ErrorMessage).ToList());
//                }
//            }
//            catch (Exception e)
//            {
//                _logger.Log(LogType.Error, () => e.Message, e);
//                return ErrorResponse(500, e.Message);
//            }
//        }
//    }
//}