namespace Tessera.Models.Chapter
{
    public class FolderDto : LeafDto
    {
        public List<LeafDto> Contents { get; set; }
    }
}
