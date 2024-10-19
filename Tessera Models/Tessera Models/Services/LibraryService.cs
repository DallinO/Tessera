using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using Tessera.Constants;

namespace Tessera.Web.Services
{
    public interface ILibraryService
    {
        bool IsAuthenticated { get; set; }
        bool HasBook { get; set; }
        ScribeDto Author { get; set; }



        BookDto CurrBook { get; set; }
        ChapterDto SelectedChapter { get; set; }
        List<BookDto> Books { get; set; }
        List<ChapterDto> Chapters { get; set; }


        //Task<ApiLoginResponse> LoginAsync(LoginRequest model);
        //Task<ApiResponse> RegisterAsync(RegisterRequest model);

        //Task<ApiBookReceipt> CheckoutBooksAsync();
        //Task<ApiResponse> CreateBookAsync(BookModel model);
        //Task<ApiChapterIndex> GetChaptersAsync();
        //Task<ApiResponse> AddChapterAsync(ChapterDto chapter);
        //Task<JObject> RemoveChapterAsync(string chapterName);
        //Task<JObject> AddRowAsync(int chapterIndex);
        //Task<JObject> RemoveRowAsync(int chapterIndex);
        //Task<JObject> UpdateChapterAsync(int chapterIndex);
        //Task<JObject> UpdateRowAsync(int chapterIndex);
        //Task IsAuthenticatedAsync();
    }

    public class LibraryService : ILibraryService
    {
        //private readonly IApiService _apiService;
        public bool IsAuthenticated { get; set; } = false;
        public bool HasBook { get; set; } = false;
        public ScribeDto Author { get; set; } = new ScribeDto();


        private BookDto _currBook;
        public BookDto CurrBook
        {
            get => _currBook;
            set
            {
                _currBook = value;
            }
        }

        private ChapterDto _selectedChapter;
        public ChapterDto SelectedChapter
        {
            get => _selectedChapter;
            set
            {
                _selectedChapter = value;
            }
        }

        private List<BookDto> _books;
        public List<BookDto> Books
        {
            get => _books;
            set
            {
                if (_books != value)
                {
                    _books = value;
                }
            }
        }

        private List<ChapterDto> _chapters;
        public List<ChapterDto> Chapters
        {
            get => _chapters;
            set
            {
                if (_chapters != value)
                {
                    _chapters = value;
                }
            }
        }


        public LibraryService(IApiService apiService)
        {
            //_apiService = apiService;
            _books = new List<BookDto>();
        }


        /***************************************************
         * LOGIN ASYNC
         ***************************************************/
        //public async Task<ApiLoginResponse> LoginAsync(LoginRequest model)
        //{
        //    var response = await _apiService.LoginAsync(model);
        //    if (response != null)
        //    {
        //        if (response.Success)
        //        {
        //            Author = response.Author;
        //        }

        //        return response;
        //    }
        //    else
        //    {
        //        List<string> errors = new List<string>()
        //        {
        //            "Null Api Response"
        //        };
        //        return new ApiLoginResponse(errors)
        //        {
        //            Success = false,
        //            Errors = errors
        //        };
        //    }


        //    /* OLD CODE
        //    var booksJson = jsonObj["books"].ToString();
        //    var books = JsonConvert.DeserializeObject<List<BookDto>>(booksJson);
        //    Books = books;
        //    if (jsonObj.ContainsKey("author"))
        //    {
        //        var authorJson = jsonObj["author"].ToString();
        //        var author = JsonConvert.DeserializeObject<ScribeDto>(authorJson);
        //        Author = author;
        //    }
        //    if (status)
        //    {
        //        IsAuthenticated = true;
        //        var result = new JObject
        //        {
        //            ["result"] = jsonObj["result"]
        //        };

        //        return result;
        //    }
        //    else
        //    {
        //        var result = new JObject
        //        {
        //            ["errors"] = jsonObj["errors"]
        //        };
        //    }


        //    return new JObject { ["errors"] = Keys.API_GENERIC_FAIL };
        //    */
        //}


        /***************************************************
         * REGISTRATION ASYNC
         ***************************************************/
        //public async Task<ApiResponse> RegisterAsync(RegisterRequest model)
        //{
        //    var response = await _apiService.RegisterAsync(model);
        //    if (response != null)
        //    {
        //        return response;
        //    }
        //    else
        //    {
        //        List<string> errors = new List<string>()
        //        {
        //            "Null Api Response"
        //        };
        //        return new ApiResponse(errors)
        //        {
        //            Success = false,
        //            Errors = errors
        //        };
        //    }
        //}


        /***************************************************
        * CREATE BOOK ASYNC
        ***************************************************/
        //public async Task<ApiBookReceipt> CheckoutBooksAsync()
        //{
        //    var response = await _apiService.CheckoutBooksAsync();
        //    if (response != null)
        //    {
        //        if (response.Success)
        //        {
        //            Books = response.Books;
        //        }

        //        return response;
        //    }
        //    else
        //    {
        //        List<string> errors = new List<string>()
        //        {
        //            "Null Api Response"
        //        };

        //        return new ApiBookReceipt(errors)
        //        {
        //            Success = false,
        //            Errors = errors
        //        };
        //    }
        //}

        /***************************************************
         * CREATE BOOK ASYNC
         ***************************************************/
        //public async Task<ApiResponse> CreateBookAsync(BookModel model)
        //{
        //    var result = await _apiService.CreateBookAsync(model);
        //    if (result == null)
        //    {
        //        return new ApiResponse()
        //        {
        //            Success = false,
        //            Errors = new List<string>()
        //            {
        //                "Null Api Response"
        //            }
        //        };
        //    }
        //    else
        //    {
        //        return result;
        //    }
        //}



        /***************************************************
         * GET CHAPTERS ASYNC
         ***************************************************/
        //public async Task<ApiChapterIndex> GetChaptersAsync()
        //{
        //    var result = await _apiService.GetChaptersAsync(CurrBook.Title);
        //    if (result == null)
        //    {
        //        return new ApiChapterIndex()
        //        {
        //            Success = false,
        //            Errors = new List<string>()
        //            {
        //                "Null Api Response"
        //            }
        //        };
        //    }
        //    else
        //    {
        //        Chapters = result.Chapters;
        //        return result;
        //    }

        //}


        /***************************************************
         * ADD CHAPTER ASYNC
         ***************************************************/
        //public async Task<ApiResponse> AddChapterAsync(ChapterDto chapter)
        //{
        //    AddChapterRequest request = new()
        //    {
        //        BookId = CurrBook.Id,
        //        Title = chapter.Title,
        //        Description = chapter.Description
        //    };

        //    var response = await _apiService.AddChapter(request);
        //    if (response == null)
        //    {
        //        return new ApiChapterIndex()
        //        {
        //            Success = false,
        //            Errors = new List<string>()
        //            {
        //                "Null Api Response"
        //            }
        //        };
        //    }
        //    else
        //    {
        //        return response;
        //    }
        //}

        //public async Task<JObject> RemoveChapterAsync(string chapterName)
        //{
        //    return new JObject();
        //}

        //public async Task<JObject> AddRowAsync(int chapterIndex)
        //{
        //    return new JObject();
        //}

        //public async Task<JObject> RemoveRowAsync(int chapterIndex)
        //{
        //    return new JObject();
        //}

        //public async Task<JObject> UpdateChapterAsync(int chapterIndex)
        //{
        //    return new JObject();
        //}

        //public async Task<JObject> UpdateRowAsync(int chapterIndex)
        //{
        //    return new JObject();
        //}

        //public async Task IsAuthenticatedAsync()
        //{

        //    IsAuthenticated = await _apiService.IsAuthenticatedAsync();
        //}
    }
}