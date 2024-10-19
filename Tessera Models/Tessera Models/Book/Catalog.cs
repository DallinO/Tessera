using System.ComponentModel.DataAnnotations;
using Tessera.Models.Authentication;

namespace Tessera.Models.Book
{
    public class Catalog
    {
        public string ScribeId { get; set; }
        public int BookId { get; set; }
        public bool IsOwner { get; set; }

        // Navigation Properties
        public Scribe Scribe { get; set; }
        public BookEntity BookEntity { get; set; }
    }

}
