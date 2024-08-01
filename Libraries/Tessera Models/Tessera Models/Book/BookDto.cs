using Tessera.Models.Chapter;

namespace Tessera.Models.Book
{
    public class BookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public bool IsOwner { get; set; }
        public List<ChapterDto>? Chapters { get; set; }

    }
}
