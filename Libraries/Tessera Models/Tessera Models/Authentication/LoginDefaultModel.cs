using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Authentication
{
    public class LoginDefaultModel
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Username or Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
