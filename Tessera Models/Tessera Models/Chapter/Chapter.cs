using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Chapter
{
    public class ChapterEntity
    {
        [Key]
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int BookId { get; set; }
    }

    
    public class ChapterDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description{ get; set; }
        public List<LeafDto> Contents { get; set; } = new List<LeafDto>();
    }


    public class AddChapterRequest
    {
        public int BookId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }


}
