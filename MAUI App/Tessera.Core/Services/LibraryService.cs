using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using Tessera.Constants;

namespace Tessera.Core.Services
{
    public interface ILibraryService
    {
        bool IsAuthenticated { get; set; }
        bool HasBook { get; set; }
        ScribeDto Author {  get; set; }



        BookDto CurrBook { get; set; }
        ChapterDto SelectedChapter { get; set; }
        List<BookDto> Books { get; set; }


        Task<JObject> LoginAsync(LoginRequest model);
        Task<JObject> RegisterAsync(RegisterDefaultModel model);
        Task<JObject> CreateBookAsync(BookModel model);
        Task<string> GetChaptersAsync();
        Task<JObject> AddChapterAsync(ChapterDto chapter);
        Task<JObject> RemoveChapterAsync(string chapterName);
        Task<JObject> AddRowAsync(int chapterIndex);
        Task<JObject> RemoveRowAsync(int chapterIndex);
        Task<JObject> UpdateChapterAsync(int chapterIndex);
        Task<JObject> UpdateRowAsync(int chapterIndex);
        Task FetchTokenAsync();
    }

    public class LibraryService : ILibraryService
    {
        private readonly IApiService _apiService;
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


        public LibraryService(IApiService apiService)
        {
            _apiService = apiService;
            _books = new List<BookDto>();
        }


        /***************************************************
         * LOGIN ASYNC
         ***************************************************/
        public async Task<JObject> LoginAsync(LoginRequest model)
        {
            var (jsonObj, status) = await _apiService.LoginAsync(model);
            if (jsonObj.ContainsKey("books"))
            {
                var booksJson = jsonObj["books"].ToString();
                var books = JsonConvert.DeserializeObject<List<BookDto>>(booksJson);
                Books = books;
            }
            if (jsonObj.ContainsKey("author"))
            {
                var authorJson = jsonObj["author"].ToString();
                var author = JsonConvert.DeserializeObject<ScribeDto>(authorJson);
                Author = author;
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


        /***************************************************
         * REGISTRATION ASYNC
         ***************************************************/
        public async Task<JObject> RegisterAsync(RegisterDefaultModel model)
        {
            var (jsonObj, status) = await _apiService.RegisterAsync(model);
            if (status)
            {
                IsAuthenticated = true;
            }
            return jsonObj;
        }


        /***************************************************
         * CREATE BOOK ASYNC
         ***************************************************/
        public async Task<JObject> CreateBookAsync(BookModel model)
        {
            var result = await _apiService.CreateBookAsync(model);
            if (result.ContainsKey("result"))
            {
                if (result.ContainsKey("books"))
                {
                    var BooksJson = result["books"].ToString();
                    var books = JsonConvert.DeserializeObject<List<BookDto>>(BooksJson);
                    Books = books;
                    if (books.Count == 1)
                    {
                        CurrBook = books[0];
                    }
                }

                return new JObject
                {
                    ["result"] = result["result"]
                };
            }
            else if (result.ContainsKey("errors"))
            {
                var jsonObj = JObject.FromObject(result["errors"]);
                return jsonObj;
            }

            return null;
        }


        /***************************************************
         * GET CHAPTERS ASYNC
         ***************************************************/
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


        /***************************************************
         * ADD CHAPTER ASYNC
         ***************************************************/
        public async Task<JObject> AddChapterAsync(ChapterDto chapter)
        {
            AddChapterRequest request = new()
            {
                BookId = CurrBook.Id,
                Title = chapter.Title,
                Description = chapter.Description
            };

            var (response, status) = await _apiService.AddChapter(request);
            if (status)
                CurrBook.Chapters.Add(chapter);

            if (response != null)
                return response;

            else
                return new JObject() { ["Errors"] = "Unknown Error" };
        }

        public async Task<JObject> RemoveChapterAsync(string chapterName)
        {
            return new JObject();
        }

        public async Task<JObject> AddRowAsync(int chapterIndex)
        {
            return new JObject();
        }

        public async Task<JObject> RemoveRowAsync(int chapterIndex)
        {
            return new JObject();
        }

        public async Task<JObject> UpdateChapterAsync(int chapterIndex)
        {
            return new JObject();
        }

        public async Task<JObject> UpdateRowAsync(int chapterIndex)
        {
            return new JObject();
        }

        public async Task FetchTokenAsync()
        {
            IsAuthenticated = await _apiService.ValidateTokenAsync();
        }

    }
}
