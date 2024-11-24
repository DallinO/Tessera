using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using Tessera.Constants;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using Microsoft.JSInterop;
using Blazored.SessionStorage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Http;

namespace Tessera.Web.Services
{
    public interface IApiService
    {
        ValueTask<string> GetJwtAsync();
        Task LogoutAsync();
        Task<ApiLoginResponse> LoginAsync(LoginRequest model);
        Task<ApiResponse> RegisterAsync(RegisterRequest model);
        Task<bool> RefreshAsync();
        Task<bool> IsAuthenticatedAsync();


        Task<ApiBookResponse> GetBookIdAsync();
        Task<ApiResponse> CreateBookAsync();
        Task<ApiChapterIndex> GetChaptersAsync(int bookId);
        Task<ApiDocumentResponse> GetDocumentDataAsync(int bookId, int chapterId);
        Task<ApiResponse> SaveDocumentDataAsync(SaveDocumentRequest request);
        Task<ApiResponse> CreateChapterAsync(AddChapterRequest request);
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

        /*########################## AUTHENTICATION ##########################*/
        /*########################## AUTHENTICATION ##########################*/
        /*########################## AUTHENTICATION ##########################*/

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
         * LOGOUT ASYNC
         ****************************************/
        public async Task LogoutAsync()
        {
            var response = await _factory.CreateClient("ServerApi").DeleteAsync("api/Authentication/Revoke");

            await _jsRuntime.InvokeVoidAsync("deleteCookie", "JWT");
            await _jsRuntime.InvokeVoidAsync("deleteCookie", "RefreshToken");

            _jwtCache = null;

            await Console.Out.WriteLineAsync($"Revoke gave response {response.StatusCode}");

            LoginChange?.Invoke(null);
        }


        /***************************************************
         * LOGIN ASYNC
         * *************************************************/
        public async Task<ApiLoginResponse> LoginAsync(LoginRequest model)
        {
            try
            {
                var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/auth/login", model);

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

                LoginChange?.Invoke(GetUsername(content.JwtToken));
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


        /***************************************************
         * REGISTER
         * *************************************************/
        public async Task<ApiResponse> RegisterAsync(RegisterRequest model)
        {
            try 
            { 
                var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/auth/register", model);

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


        /*########################## LIBRARY ##########################*/
        /*########################## LIBRARY ##########################*/
        /*########################## LIBRARY ##########################*/


        /***************************************************
         * CHECKOUT BOOK
         **************************************************/
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


        /***************************************************
         * CREATE BOOK
         * *************************************************/
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


        /***************************************************
         * GET CHAPTERS
         * *************************************************/
        public async Task<ApiChapterIndex> GetChaptersAsync(int bookId)
        {
            var client = _factory.CreateClient("ServerApi");
            var url = $"api/library/getchapterlist?bookId={bookId}";
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


        /***************************************************
        * SAVE CHAPTERS
        * *************************************************/
        public async Task<ApiResponse> SaveDocumentDataAsync(SaveDocumentRequest request)
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


        public async Task<ApiDocumentResponse> GetDocumentDataAsync(int bookId, int chapterId)
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

        /***************************************************
         * GET CHAPTERS
         * *************************************************/
        public async Task<ApiResponse> CreateChapterAsync(AddChapterRequest request)
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


        /****************************************
         * GET USERNAME
         ****************************************/
        private static string GetUsername(string token)
        {
            var jwt = new JwtSecurityToken(token);

            return jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value;
        }



        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _sss.GetItemAsync<string>(JWT_KEY);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            // Check if the token is expired
            var jwtHandler = new JwtSecurityTokenHandler();
            var jwtToken = jwtHandler.ReadToken(token) as JwtSecurityToken;
            if (jwtToken == null)
            {
                return false;
            }

            return jwtToken.ValidTo > DateTime.UtcNow;
        }
    }
}
