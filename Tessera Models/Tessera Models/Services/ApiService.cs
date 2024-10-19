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

        
        Task<ApiBookReceipt> CheckoutBooksAsync();
        Task<ApiResponse> CreateBookAsync(BookModel model);
        Task<ApiChapterIndex> GetChaptersAsync(int bookId);
        Task<ApiResponse> AddChapter(AddChapterRequest request);
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

            // Remove cookies using JavaScript Interop
            await _jsRuntime.InvokeVoidAsync("deleteCookie", "JWT");
            await _jsRuntime.InvokeVoidAsync("deleteCookie", "RefreshToken");

            _jwtCache = null;

            await Console.Out.WriteLineAsync($"Revoke gave response {response.StatusCode}");

            LoginChange?.Invoke(null);

            /* OLD CODE
            var response = await _factory.CreateClient("ServerApi").DeleteAsync("api/Authentication/Revoke");

            await _sss.RemoveItemAsync(JWT_KEY);
            await _sss.RemoveItemAsync(REFRESH_KEY);

            _jwtCache = null;

            await Console.Out.WriteLineAsync($"Revoke gave response {response.StatusCode}");

            LoginChange?.Invoke(null);
            */
        }


        /***************************************************
         * LOGIN ASYNC
         * *************************************************/
        public async Task<ApiLoginResponse> LoginAsync(LoginRequest model)
        {

            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/Authentication/CheckIn", model);

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

            //await _sss.SetItemAsync(JWT_KEY, content.JwtToken);
            //await _sss.SetItemAsync(REFRESH_KEY, content.RefreshToken);

            LoginChange?.Invoke(GetUsername(content.JwtToken)); // -- What is the purpose of this?

            //return content.Expiration; // -- may need this in order to set up a session time.
            return content;



            /*
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<ApiLoginResponse>();

                if (content != null)
                {
                    await _sss.SetItemAsync(JWT_KEY, content.JwtToken);


                    //// Store the access token in local storage
                    //var token = jsonObject["token"]?.ToString();
                    //if (!string.IsNullOrEmpty(token))
                    //{
                    //    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", token);
                    //}

                    //return jsonObject["result"]?.ToString() == Keys.API_LOGIN_SUCC ?
                    //    (jsonObject, true) : (jsonObject, false);
                }

                return (new JObject { ["result"] = Keys.API_GENERIC_SUCC }, false);
            }
            //
            // LOGIN FAIL
            //
            else
            {
                if (result != null)
                {
                    JObject jsonObject;
                    try
                    {
                        jsonObject = JObject.Parse(result);
                    }
                    catch (Newtonsoft.Json.JsonException)
                    {
                        jsonObject = new JObject { ["errors"] = result };
                    }
                    if (jsonObject != null)
                    {
                        return (jsonObject, false);

                    }
                }

                return (new JObject { ["result"] = Keys.API_GENERIC_FAIL }, false);
            }
        }
            */
        }


        /***************************************************
         * REGISTER
         * *************************************************/
        public async Task<ApiResponse> RegisterAsync(RegisterRequest model)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/Authentication/Register", model);

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

            /* OLD CODE
            //
            // REGISTER SUCCESS
            //
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (result != null)
                {
                    JObject jsonObject = JObject.Parse(result);
                    if (jsonObject != null)
                    {
                        return jsonObject["result"]?.ToString() == Keys.API_REG_SUCC ?
                            (jsonObject, true) : (jsonObject, false);

                    }
                }

                return (new JObject { ["result"] = Keys.API_GENERIC_SUCC }, false);
            }
            //
            // REGISTER FAIL
            //
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                if (errorResult != null)
                {
                    JObject jsonObject;
                    try
                    {
                        jsonObject = JObject.Parse(errorResult);
                    }
                    catch (Newtonsoft.Json.JsonException)
                    {
                        jsonObject = new JObject { ["errors"] = errorResult };
                    }
                    if (jsonObject != null)
                    {
                        return (jsonObject, false);
                    }
                }

                return (new JObject { ["errors"] = Keys.API_GENERIC_FAIL }, false);
            }
            */
        }


        /*########################## LIBRARY ##########################*/
        /*########################## LIBRARY ##########################*/
        /*########################## LIBRARY ##########################*/


        /***************************************************
         * CHECKOUT BOOK
         **************************************************/
        public async Task<ApiBookReceipt> CheckoutBooksAsync()
        {
            var response = await _factory.CreateClient("ServerApi").GetAsync("api/Library/CheckoutBooks");

            var content = await response.Content.ReadFromJsonAsync<ApiBookReceipt>();
            if (content == null)
            {
                return new ApiBookReceipt()
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
        public async Task<ApiResponse> CreateBookAsync(BookModel model)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/Library/CreateBook", model);
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
            var url = $"api/Library/FetchChapters?bookId={bookId}";
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
         * GET CHAPTERS
         * *************************************************/
        public async Task<ApiResponse> AddChapter(AddChapterRequest request)
        {
            var response = await _factory.CreateClient("ServerApi").PostAsJsonAsync("api/Library/AddChapter", request);
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
         * UPDATE HTTP CLIENT ASCYNC
         ***************************************
        private async Task UpdateHttpClientAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _factory.CreateClient("ServerApi").DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        */


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

            /* OLD CODE
            var model = new RefreshModel
            {
                AccessToken = await _sss.GetItemAsync<string>(JWT_KEY),
                RefreshToken = await _sss.GetItemAsync<string>(REFRESH_KEY)
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

            await _sss.SetItemAsync(JWT_KEY, content.JwtToken);
            await _sss.SetItemAsync(REFRESH_KEY, content.RefreshToken);

            _jwtCache = content.JwtToken;

            return true;
            */
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
