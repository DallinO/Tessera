using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Tessera.Models.Authentication;
using Microsoft.JSInterop;
using Blazored.SessionStorage;
using Microsoft.Extensions.Logging;

namespace Tessera.Web.Services
{
    public interface IApiService
    {
        // Authentication
        Task<ApiResponse> RegisterAsync(RegisterRequest model);
        Task<ApiLoginResponse> LoginAsync(LoginRequest model);
        Task<bool> LogoutAsync();
        Task<ApiResponse> DeleteAccountAsync();
        // Tokens
        ValueTask<string> GetJwtAsync();
        Task<bool> RefreshAsync();
        // Book
        Task<ApiResponse> CreateBookAsync();
        Task<ApiBookResponse> GetBookIdAsync();
        // Chapter
        Task<ApiResponse> AddChapterAsync(ApiChapterRequest request);
        Task<ApiChapterIndex> GetChaptersAsync(int bookId);
        Task<ApiResponse> UpdateChapterAsync(ApiChapterRequest request);
        Task<ApiResponse> DeleteChapterAsync(int bookId, int chapterId);
        // Document
        Task<ApiDocumentResponse> GetDocumentAsync(int bookId, int chapterId);
        Task<ApiResponse> SaveDocumentAsync(SaveDocumentRequest request);
        Task<ApiResponse> DeleteDocumentAsync(int bookId, int chapterId);
        // Row
        Task<ApiListResponse> GetListAsync(int bookId, int chapterId);
        Task<ApiResponse> DeleteListRowAsync(int bookId, int chapterId);
        Task<ApiResponse> AddRowAsync(ApiRowRequest request);
        Task<ApiResponse> UpdateRowAsync(ApiRowRequest request);
        Task<ApiResponse> DeleteRowAsync(int bookId, int chapterId, int rowId);
    }

    public class ApiService : IApiService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IJSRuntime _jsRuntime;
        private readonly ISessionStorageService _sss;
        private readonly ILogger<ApiService> _logger;
        private const string JWT_KEY = nameof(JWT_KEY);
        private const string REFRESH_KEY = nameof(REFRESH_KEY);

        private string? _jwtCache;
        public event Action<string?>? LoginChange;


        public ApiService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime, ISessionStorageService sss, ILogger<ApiService> logger)
        {
            _factory = httpClientFactory;
            _jsRuntime = jsRuntime;
            _sss = sss;
            _logger = logger;
        }


        /* ########## AUTHENTICATION ########## */

        /****************************************
         * REGISTER ASYNC                (CREATE)
         ****************************************/
        public async Task<ApiResponse> RegisterAsync(RegisterRequest request)
        {
            try 
            { 
                var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/auth/register", request);

                var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
                if (content == null)
                {
                    List<string> errors = new List<string>()
                    {
                        "Null Api Response"
                    };

                    return new ApiResponse(errors)
                    {
                        Success = false
                    };
                }
                else
                {
                    return content;
                }
            }
            catch (Exception ex)
            {
                return new ApiResponse
                {
                    Success = false,
                    Errors = new List<string> { $"{ex.Message}" }
                };
            }
        }

        /****************************************
         * LOGIN ASYNC                     (READ)
         ****************************************/
        public async Task<ApiLoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/auth/login", request);

                var content = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

                if (content == null)
                {
                    return new ApiLoginResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Null Api Response"
                        }
                    };
                }

                await _jsRuntime.InvokeVoidAsync("setCookie", "JWT", content.JwtToken, 1);
                await _jsRuntime.InvokeVoidAsync("setCookie", "RefreshToken", content.RefreshToken, 1);

                var jwt = new JwtSecurityToken(content.JwtToken);
                var userId =  jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value;
                LoginChange?.Invoke(userId);

                return content;
            }
            catch (Exception ex)
            {
                return new ApiLoginResponse
                {
                    Success = false,
                    Errors = new List<string> { $"{ex.Message}" }
                };
            }
        }

        /****************************************
         * LOGOUT ASYNC                  (UPDATE)
         ****************************************/
        public async Task<bool> LogoutAsync()
        {
            var response = await _factory.CreateClient("ServerApi").DeleteAsync("api/Authentication/Revoke");

            await _jsRuntime.InvokeVoidAsync("deleteCookie", "JWT");
            await _jsRuntime.InvokeVoidAsync("deleteCookie", "RefreshToken");

            _jwtCache = null;

            await Console.Out.WriteLineAsync($"Revoke gave response {response.StatusCode}");

            LoginChange?.Invoke(null);
            return true; // UPDATE THIS!!!
        }

        /****************************************
         * DELETE ACCOUNT ASYNC          (DELETE)
         ****************************************/
        public async Task<ApiResponse> DeleteAccountAsync()
        {
            return new ApiResponse();
        }





        /* ############## TOKEN ############### */

        /****************************************
         * GET JWT ASCYNC
         ****************************************/
        public async ValueTask<string> GetJwtAsync()
        {
            if (string.IsNullOrEmpty(_jwtCache))
            {
                //_jwtCache = await _sss.GetItemAsync<string>(JWT_KEY);
                _jwtCache = await _jsRuntime.InvokeAsync<string>("getCookie", "JWT");
            }

            return _jwtCache;
        }

        /****************************************
         * REFRESH ASCYNC
         ****************************************/
        public async Task<bool> RefreshAsync()
        {
            var accessToken = await _jsRuntime.InvokeAsync<string>("getCookie", "JWT");
            var refreshToken = await _jsRuntime.InvokeAsync<string>("getCookie", "RefreshToken");

            var model = new RefreshModel
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            var response = await _factory.CreateClient("ServerApi").PostAsync("api/Authentication/Refresh",
                JsonContent.Create(model));

            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return false;
            }

            var content = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

            if (content == null)
                throw new InvalidDataException();

            // Set the new tokens in cookies using JavaScript Interop
            await _jsRuntime.InvokeVoidAsync("setCookie", "JWT", content.JwtToken, 1);
            await _jsRuntime.InvokeVoidAsync("setCookie", "RefreshToken", content.RefreshToken, 1);

            _jwtCache = content.JwtToken;

            return true;

           
        }





        /* ############### BOOK ############### */

        /****************************************
         * CREATE BOOK ASYNC             (CREATE)
         * **************************************/
        public async Task<ApiResponse> CreateBookAsync()
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/library/createbook", new { });
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content == null)
            {
                return new ApiResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /****************************************
         * GET BOOK ID ASYNC               (READ)
         ****************************************/
        public async Task<ApiBookResponse> GetBookIdAsync()
        {
            var response = await _factory.CreateClient("ServerApi").GetAsync("api/library/getbookid");

            var content = await response.Content.ReadFromJsonAsync<ApiBookResponse>();
            if (content == null)
            {
                return new ApiBookResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }





        /* ############# CHAPTER ############## */

        /****************************************
         * ADD CHAPTER ASYNC             (CREATE)
         ****************************************/
        public async Task<ApiResponse> AddChapterAsync(ApiChapterRequest request)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/library/createchapter", request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content == null)
            {
                return new ApiResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /****************************************
         * GET CHAPTERS ASYNC              (READ)
         ****************************************/
        public async Task<ApiChapterIndex> GetChaptersAsync(int bookId)
        {
            var client = _factory.CreateClient("ServerApi");
            var url = $"api/library/getchapters?bookId={bookId}";
            var response = await client.GetAsync(url);

            var content = await response.Content.ReadFromJsonAsync<ApiChapterIndex>();
            if (content == null)
            {
                return new ApiChapterIndex()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /****************************************
         * UPDATE CHAPTER ASYNC          (Update)
         ****************************************/
        public async Task<ApiResponse> UpdateChapterAsync(ApiChapterRequest request)
        {
            return new ApiResponse();
        }

        /****************************************
         * DELETE CHAPTER ASYNC          (DELETE)
         ****************************************/
        public async Task<ApiResponse> DeleteChapterAsync(int bookId, int chapterId)
        {
            return new ApiResponse();
        }





        /* ############# DOCUMENT ############# */

        /****************************************
         * SAVE DOCUMENT DATA ASYNC      (UPDATE)
         ****************************************/
        public async Task<ApiResponse> SaveDocumentAsync(SaveDocumentRequest request)
        {
            var response = await _factory.CreateClient("ServerApi").PatchAsJsonAsync("api/library/savedocument", request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content == null)
            {
                return new ApiResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /****************************************
         * GET DOCUMENT DATA ASYNC         (READ)
         ****************************************/
        public async Task<ApiDocumentResponse> GetDocumentAsync(int bookId, int chapterId)
        {
            try
            {
                var client = _factory.CreateClient("ServerApi");
                var url = $"api/library/getdocument?bookId={bookId}&chapterId={chapterId}";
                var response = await client.GetAsync(url);

                var content = await response.Content.ReadFromJsonAsync<ApiDocumentResponse>();
                if (content == null)
                {
                    return new ApiDocumentResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Null Api Response"
                        }
                    };
                }
                else
                {
                    return content;
                }
            }
            catch (Exception ex)
            {
                return new ApiDocumentResponse()
                {
                    Success = false,
                    Errors = new List<string> { $"{ex.Message}" }
                };
            }
        }

        /****************************************
         * DELETE DOCUMENT DATA ASYNC    (DELETE)
         ****************************************/
        public async Task<ApiResponse> DeleteDocumentAsync(int bookId, int chapterId)
        {
            return new ApiResponse();
        }





        /* ############### LIST ############### */

        /***************************************************
         * GET LIST DATA ASYNC
         * *************************************************/
        public async Task<ApiListResponse> GetListAsync(int bookId, int chapterId)
        {
            try
            {
                var client = _factory.CreateClient("ServerApi");
                var url = $"api/library/getlist?bookId={bookId}&chapterId={chapterId}";
                var response = await client.GetAsync(url);

                var content = await response.Content.ReadFromJsonAsync<ApiListResponse>();
                if (content == null)
                {
                    return new ApiListResponse()
                    {
                        Success = false,
                        Errors = new List<string>()
                        {
                            "Null Api Response"
                        }
                    };
                }
                else
                {
                    return content;
                }
            }
            catch (Exception ex)
            {
                return new ApiListResponse()
                {
                    Success = false,
                    Errors = new List<string> { $"{ex.Message}" }
                };
            }
        }

        /***************************************************
         * DELETE LIST DATA ASYNC
         * *************************************************/
        public async Task<ApiResponse> DeleteListRowAsync(int bookId, int chapterId)
        {
            return new ApiResponse();
        }

        /***************************************************
         * UPDATE LIST DATA ASYNC
         * *************************************************/
        public async Task<ApiResponse> AddRowAsync(ApiRowRequest request)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/library/addrow", request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content == null)
            {
                return new ApiResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /***************************************************
         * UPDATE LIST DATA ASYNC
         * *************************************************/
        public async Task<ApiResponse> UpdateRowAsync(ApiRowRequest request)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/library/updaterow", request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            if (content == null)
            {
                return new ApiResponse()
                {
                    Success = false,
                    Errors = new List<string>()
                    {
                        "Null Api Response"
                    }
                };
            }
            else
            {
                return content;
            }
        }

        /***************************************************
         * DELETE ROW DATA ASYNC
         * *************************************************/
        public async Task<ApiResponse> DeleteRowAsync(int bookId, int chapterId, int rowId)
        {
            return new ApiResponse();
        }
    }
}
