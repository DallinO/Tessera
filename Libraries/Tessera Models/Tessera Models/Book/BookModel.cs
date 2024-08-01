using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Book
{
    public class BookModel
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        [RegularExpression(@"^[a-zA-Z\s-']+$", ErrorMessage = "Name can only contain letters, spaces, hyphens, and apostrophes")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Confirm name is required")]
        [Compare("Name", ErrorMessage = "Names do not match.")]
        public string ConfirmName { get; set; }
    }
}
