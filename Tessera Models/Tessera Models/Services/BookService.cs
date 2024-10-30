using Tessera.Models.Chapter;

namespace Tessera.Web.Services
{
    public interface IBookService
    {
        int BookId { get; set; }
        ChapterDto SelectedChapter { get; set; }
        List<ChapterDto> Chapters { get; set; }
    }

    public class BookService : IBookService
    {
        public int BookId { get; set; }
        public ChapterDto SelectedChapter { get; set; }
        public List<ChapterDto> Chapters { get; set; }


        public BookService(IApiService apiService)
        {
            Chapters = new List<ChapterDto>();
        }
    }
}