using System.ComponentModel.DataAnnotations;

namespace Kachuwa.Identity.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "UserNameOrEmailRequired")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Login.PasswordRequired")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}