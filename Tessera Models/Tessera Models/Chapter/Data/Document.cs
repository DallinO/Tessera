using System.ComponentModel.DataAnnotations;

namespace Tessera.Models.Chapter
{
    
    public class DocumentDto : ChapterDto
    {
        public List<LeafDto> Sections { get; set; } = new List<LeafDto>();
    }

    public class LeafDto
    {
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Section { get; set; } = string.Empty;
    }

    public class LeafEntity
    {
        [Key]
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Section { get; set; } = string.Empty;
    }


}
