using Tessera.Models.Authentication;
using Tessera.Models.Chapter;

namespace Tessera.Web.Services
{
    public interface IBookService
    {
        int BookId { get; set; }
        bool IsAuthenticated { get; set; }
        DateTime? TokenExp { get; set; }
        ChapterDto SelectedChapter { get; set; }
        List<ChapterDto> Chapters { get; set; }
        AppUserDto AppUser { get; set; }
    }

    public class BookService : IBookService
    {
        public int BookId { get; set; }
        public bool IsAuthenticated { get; set; } = false;
        public DateTime? TokenExp { get; set; }
        public ChapterDto SelectedChapter { get; set; }
        public List<ChapterDto> Chapters { get; set; }
        public AppUserDto AppUser { get; set; }


        public BookService(IApiService apiService)
        {
            Chapters = new List<ChapterDto>();
        }
    }
}