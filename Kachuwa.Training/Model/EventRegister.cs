using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Kachuwa.Training.Model
{
    [Table("EventRegister")]
   public  class EventRegister
    {
        [Key]
        public int Id { get; set; }
        public int EventId { get; set; }
        [Required(ErrorMessage = "Email Required")]
        [RegularExpression(@"^[a-z][a-z|0-9|]*([_][a-z|0-9]+)*([.][a-z|0-9]+([_][a-z|0-9]+)*)?@[a-z][a-z|0-9|]*\.([a-z][a-z|0-9]*(\.[a-z][a-z|0-9]*)?)$", ErrorMessage = "Not a Valid Email")]
        //[EmailAddress(ErrorMessage = "Invalid Email")]
        public string EmailAddress { get; set; }
        [RegularExpression(@"^?([0-9]{3})?([0−9]3)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        public string  PhoneNumber { get; set; }
        [Required(ErrorMessage = "FullName Required")]
        public string FullName { get; set; }

        public int UserId { get; set; }
    }
}
