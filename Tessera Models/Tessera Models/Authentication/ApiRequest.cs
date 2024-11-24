using Tessera.Constants;
using Tessera.Models.Chapter;

namespace Tessera.Models.Authentication
{
    public class ApiRequest
    {
    }

    public class ApiChapterRequest
    {
        public int BookId { get; set; }
        public int ChapterId { get; set; }
        public LeafType Type { get; set; }
    }

    public class SaveDocumentRequest
    {
        public int BookId { get; set; }
        public DocumentDto Document { get; set; }
    }


}
