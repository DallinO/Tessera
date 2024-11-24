using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

namespace Tessera.Models.Chapter
{
    
    public class DocumentDto : ChapterDto
    {
        public int DocumentId { get; set; }
        public string Content { get; set; }
    }

    public class DocumentEntity
    {
        [Key]
        public int Id { get; set; }
        public int ChapterId { get; set; }
        public string Content { get; set; } = string.Empty;
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
