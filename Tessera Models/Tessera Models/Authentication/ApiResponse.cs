using Tessera.Models.Book;
using Tessera.Models.Chapter;

namespace Tessera.Models.Authentication
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public ApiResponse() { }
        public ApiResponse(List<string> errors)
        {
            Errors = errors;
        }
    }


    public class ApiLoginResponse : ApiResponse
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
        public ScribeDto Author { get; set; }

        public ApiLoginResponse() { }
        public ApiLoginResponse(List<string> errors) : base(errors) { }
    }


    public class ApiBookResponse : ApiResponse
    {
        public int BookId { get; set; }

        public ApiBookResponse() { }
        public ApiBookResponse(List<string> errors) : base(errors) { }
    }

    public class ApiChapterIndex : ApiResponse
    {
        public List<ChapterDto> Chapters { get; set; }
        public ApiChapterIndex() { }
        public ApiChapterIndex(List<string> errors) : base(errors) { }
    }

    public class ApiChapterData : ApiResponse
    {
        public ChapterDto Chapter { get; set; }
        public ApiChapterData() { }
        public ApiChapterData(List<string> errors) : base(errors) { }
    }

}