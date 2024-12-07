using System.ComponentModel.DataAnnotations;
using Tessera.Constants;

namespace Tessera.Models.Chapter
{
    public class ListDto : ChapterDto
    {
        public List<RowDto> Rows {get; set; } = new List<RowDto>();
    }

    public class RowDto
    {
        public int RowId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Priority Priority { get; set; }
        public bool IsComplete { get; set; }
        public DateTime Created { get; set; }
        public DateTime Due { get; set; }
        public int ChapterId { get; set; }
    }

    public class RowEntity
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Priority { get; set; }
        public bool IsComplete { get; set; }
        public DateTime Created { get; set; }
        public DateTime Due {  get; set; }
        public int ChapterId { get; set; }

        // Navigation properties
        public ChapterEntity Chapter { get; set; }
    }
}
