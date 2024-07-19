using Tessera.Models.ChapterComponents;

namespace Tessera.Models.ChapterComponents.Data
{
    public class FolderDto : LeafDto
    {
        public List<LeafDto> Contents { get; set; }
    }
}
