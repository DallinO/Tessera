using Tessera.Models.ChapterComponents;

namespace Tessera.Models.ChapterComponents.Data
{
    public class Folder : Leaf
    {
        public List<LeafModel> Contents { get; set; }
    }
}
