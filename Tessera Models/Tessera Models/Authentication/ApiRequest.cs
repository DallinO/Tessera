using Tessera.Constants;
using Tessera.Models.Chapter;
using Tessera.Models.Chapter.Data;

namespace Tessera.Models.Authentication
{
    public class SaveDocumentRequest
    {
        public int BookId { get; set; }
        public DocumentDto? Document { get; set; }
    }

    public class ApiChapterRequest
    {
        public int BookId { get; set; }
        public ChapterDto? Chapter { get; set; }
    }

    public class ApiRowRequest
    {
        public int BookId { get; set; }
        public int ChapterId { get; set; }
        public RowDto Row { get; set; }
    }

    public class ApiDeleteRowRequest
    {
        public int BookId { get; set; }
        public int ChapterId { get; set; }
        public int RowId { get; set; }
    }

    public class ApiAddNotificationRequest
    {
        public int BookId { get; set; }
        public NotificationEntity Notification { get; set; }
    }


}
