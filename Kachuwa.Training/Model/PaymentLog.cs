using Kachuwa.Data.Crud.Attribute;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("PaymentLog")]
    public class PaymentLog
    {
        [Key]
        public int Id { get; set; }
        public int EnrollId { get; set; }
        public int UserId { get; set; }
        public int CourseId { get; set; }
        public string PaymentSource { get; set; }
        public string  VoucherNumber { get; set; }
        public string DepositedBy { get; set; }
        public string  VoucherAttachmentPath { get; set; }
        public bool IsVerified { get; set; }
        [IgnoreAll]
        public IFormFile VoucherAttachment { get; set; }

        [IgnoreAll]
        public int RowTotal { get; set; }
    }
    public class PaymentLogViewModel : PaymentLog
    {
        public string UserEmail { get; set; }
        public string CourseName { get; set; }
        public decimal CourseFee { get; set; }

       
    }
}
