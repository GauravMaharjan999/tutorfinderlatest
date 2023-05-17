using Kachuwa.Log;
using Kachuwa.Training.Service;
using Kachuwa.Web.API;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Api
{
    [Route("api/v1/payment")]
    public  class PaymentLogApiController : BaseApiController
    {
        
        private readonly IPaymentLogService _paymentLogService;
        private ILogger _logger;

        public PaymentLogApiController(IPaymentLogService paymentLogService,ILogger logger)
        {
            _paymentLogService = paymentLogService;
            _logger = logger;
        }

        [Route("allpaymentlist")]
        public async Task<object> AllPaymentList([FromQuery]int pq_curpage = 1, [FromQuery] int pq_rpp = 20)
        {

            try
            {
                var data = await _paymentLogService.AllPaymentList(pq_curpage, pq_rpp);
                return HttpResponse(200, "Success", data, pq_curpage);
            }
            catch (Exception ex)
            {

                _logger.Log(LogType.Error, () => ex.Message, ex);
                return HttpResponse(500, ex.Message, "");
            }
        }
    }
}
