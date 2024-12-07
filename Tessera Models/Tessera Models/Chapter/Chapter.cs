using System.ComponentModel.DataAnnotations;
using Tessera.Constants;

namespace Tessera.Models.Chapter
{
    public class ChapterEntity
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int BookId { get; set; }
        public int Type { get; set; }

        // Navigation properties
        public DocumentEntity Document { get; set; }
        public ICollection<RowEntity> Rows { get; set; }
    }

    
    public class ChapterDto
    {
        public int ChapterId { get; set; }
        public string Title { get; set; }
        public string Description{ get; set; }
        public LeafType Type { get; set; }
    }
}
