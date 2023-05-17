using System.Threading.Tasks;
using Kachuwa.Dash.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Api.V1.KachawaSchedular
{

    [Route("api/v1/test")]
    public class TestController : Controller
    {
        private readonly IEncodingService _encodingService;

        public TestController(IEncodingService encodingService)
        {
            _encodingService = encodingService;
        }
        [AllowAnonymous]
        [HttpGet]
        public async Task<dynamic> Index()
        {
            await _encodingService.EncodeAsnyc("A5FE3044-46CA-452F-9998-4DB3DFEB0E28");
            return "Ok";
        }
    }
}