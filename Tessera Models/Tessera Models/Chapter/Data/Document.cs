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

        // Navigation properties
        public ChapterEntity Chapter { get; set; }
    }
}
