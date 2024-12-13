using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Authentication
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Username or Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public bool RememberMe { get; set; }
    }

    public class ResetPasswordRequest
    {
        [Required(ErrorMessage = "Email is required")]
        [Display(Name = "Username or Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*\W).{8,}$", ErrorMessage = "Password must contain at least one number, one uppercase letter, and one special character.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }

}
