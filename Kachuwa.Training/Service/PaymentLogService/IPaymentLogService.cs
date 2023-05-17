using Kachuwa.Data;
using Kachuwa.Training.Model;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Kachuwa.Training.Service
{
   public interface IPaymentLogService
    {
        CrudService<PaymentLog> PaymentLogCrudService { get; set; }
        Task<IEnumerable<PaymentLogViewModel>> AllPaymentList(int pq_curpage, int pq_rpp);
        Task<PaymentLogViewModel> PaymentDetailByPaymentId(int paymentLogId);
        Task<IEnumerable<PaymentLogViewModel>> PaymentListbyUserId(int userId, int pq_curpage, int pq_rpp);
    }
}
