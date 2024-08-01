namespace Tessera.Models.Chapter
{
    public class ChapterEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description{ get; set; }
    }
    
    public class ChapterDto
    {
        public string Title { get; set; }
        public string Description{ get; set; }
        public List<LeafDto> Contents { get; set; } = new List<LeafDto>();
    }


}
