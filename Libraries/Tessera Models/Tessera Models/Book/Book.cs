using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Book
{
    // Book Base
    public class Preface
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        public string Title { get; set; }
        public string OwnerId { get; set; }
    }

    public class BookEntity : Preface
    {

    }
}
