using System.ComponentModel.DataAnnotations;

namespace Tessera.Models
{
    public class RegisterDefaultModel
    {
        [Required(ErrorMessage = "First name is required")]
        [StringLength(4)]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First name can only contain letters, spaces, and hyphens")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Last name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-]+$", ErrorMessage = "Last name can only contain letters, spaces, and hyphens")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [StringLength(100, ErrorMessage = "Email cannot be longer than 100 characters")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Confirm email is required")]
        [Compare("Email", ErrorMessage = "The email and confirmation email do not match.")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string ConfirmEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be between 8 and 100 characters")]
        [RegularExpression(@"^(?=.*\d)(?=.*[A-Z])(?=.*\W).{8,}$", ErrorMessage = "Password must contain at least one number, one uppercase letter, and one special character.")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
