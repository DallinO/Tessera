using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Constants;
using Tessera.Core.Services;
using Tessera.Models.ChapterComponents;

namespace Tessera
{
    public interface ILibraryService
    {
        bool IsAuthenticated { get; set; }
        bool HasBook { get; set; }
        BookDto CurrBook { get; set; }
        ChapterDto SelectedChapter { get; set; }
        BookDto[] Books { get; set; }
        Task<JObject> LoginAsync(LoginDefaultModel model);
        Task<JObject> RegisterAsync(RegisterDefaultModel model);
        Task<JObject> CreateBookAsync(BookModel model);
        Task<string> GetChaptersAsync();
    }

    public class LibraryService : ILibraryService
    {
        private readonly IApiService _apiService;
        public bool IsAuthenticated { get; set; } = false;
        public bool HasBook { get; set; } = false;


        private BookDto _currBook;
        public BookDto CurrBook
        {
            get => _currBook;
            set
            {
                _currBook = value;
            }
        }
        private BookDto[] _books;


        private ChapterDto _selectedChapter;
        public ChapterDto SelectedChapter
        {
            get => _selectedChapter;
            set
            {
                _selectedChapter = value;
            }
        }


        public BookDto[] Books
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

        public LibraryService(IApiService apiService)
        {
            _apiService = apiService;
        }

        /***************************************************
         * LOGIN
         ***************************************************/
        public async Task<JObject> LoginAsync(LoginDefaultModel model)
        {
            var (jsonObj, status) = await _apiService.LoginAsync(model);
            if (jsonObj.ContainsKey("books"))
            {
                var booksJson = jsonObj["books"].ToString();
                var books = JsonConvert.DeserializeObject<BookDto[]>(booksJson);
                Books = books;
            }
            if (status)
            {
                IsAuthenticated = true;
                var result = new JObject
                {
                    ["result"] = jsonObj["result"]
                };

                return result;
            }
            else
            {
                var result = new JObject
                {
                    ["errors"] = jsonObj["errors"]
                };
            }


            return new JObject { ["errors"] = Keys.API_GENERIC_FAIL };
        }


        public async Task<JObject> RegisterAsync(RegisterDefaultModel model)
        {
            var (jsonObj, status) = await _apiService.RegisterAsync(model);
            if (status)
            {
                IsAuthenticated = true;
            }
            return jsonObj;
        }


        public async Task<JObject> CreateBookAsync(BookModel model)
        {
            var result = await _apiService.CreateBookAsync(model);
            if (result.ContainsKey("result"))
            {
                var jsonObj = JObject.FromObject(result["result"]);
                if (result.ContainsKey("books"))
                {
                    var BooksJson = result["books"].ToString();
                    var books = JsonConvert.DeserializeObject<BookDto[]>(BooksJson);
                    Books = books;
                    if (books.Length == 1)
                    {
                        CurrBook = books[0];
                    }
                }

                return jsonObj;
            }
            else if (result.ContainsKey("errors"))
            {
                var jsonObj = JObject.FromObject(result["errors"]);
                return jsonObj;
            }

            return null;
        }

        public async Task<string> GetChaptersAsync()
        {
            var (chapters, errorMsg) = await _apiService.GetChapters(CurrBook.Title);
            
            if (errorMsg != null)
            {
                return errorMsg;
            }

            CurrBook.Chapters = chapters;
            return null;

        }
    }
}
