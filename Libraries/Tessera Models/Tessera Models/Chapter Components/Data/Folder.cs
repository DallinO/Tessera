namespace Tessera.Models
{
    public class FolderDto : LeafDto
    {
        public List<LeafDto> Contents { get; set; }
    }
}
