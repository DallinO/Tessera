using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Tessera.Models.Chapter;

namespace Tessera.Models.Book
{
    // Book Base
    public class BookEntity
    {
        [Key]
        public int Id { get; set; }
        public string ScribeId { get; set; }
        public string Database { get; set; }
    }

    public class BookDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Database { get; set; }
    }

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
