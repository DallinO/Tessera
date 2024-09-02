using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Tessera.Constants;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.Chapter;
using Microsoft.JSInterop;
using Blazored.SessionStorage;

namespace Tessera.Core.Services
{
    public interface IApiService
    {
        Task<(JObject, bool)> LoginAsync(LoginRequest model);
        Task<(JObject, bool)> RegisterAsync(RegisterDefaultModel model);
        Task<JObject> CreateBookAsync(BookModel model);
        Task<(List<ChapterDto>, string)> GetChapters(string dbName);
        Task<(JObject, bool)> AddChapter(AddChapterRequest request);
        Task<bool> ValidateTokenAsync();
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IJSRuntime _jsRuntime;
        private readonly ISessionStorageService _sss;
        private const string JWT_KEY = nameof(JWT_KEY);
        private string? _jwtCache;

        public ApiService(HttpClient httpClient, IJSRuntime jsRuntime, ISessionStorageService sss)
        {
            _httpClient = httpClient;
            _jsRuntime = jsRuntime;
            _sss = sss;
        }

        public async ValueTask<string> GetJwtAsync()
        {
            if (string.IsNullOrEmpty(_jwtCache))
            {
                _jwtCache = await _sss.GetItemAsync<string>(JWT_KEY);
            }

            return _jwtCache;
        }


        public async Task LogoutAsync()
        {
            await _sss.RemoveItemAsync(JWT_KEY);
            _jwtCache = null;
        }



        /***************************************************
         * LOGIN ASYNC
         * *************************************************/
        public async Task<(JObject, bool)> LoginAsync(LoginRequest model)
        {
            await UpdateHttpClientAsync();


            var response = await _httpClient.PostAsJsonAsync("api/Api/Login", model);

            if (!response.IsSuccessStatusCode)
                throw new UnauthorizedAccessException("Login failed.");

            var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

            if (content == null)
                throw new InvalidDataException();

            await _sss.SetItemAsync(JWT_KEY, content.JwtToken);
            await _sss.SetItemAsync(REFRESH_KEY, content.RefreshToken);

            LoginChange?.Invoke(GetUsername(content.JwtToken));

            return content.Expiration;

            //if (response.IsSuccessStatusCode)
            //{
            //    var content = await response.Content.ReadFromJsonAsync<LoginResponse>();

            //    if (content != null)
            //    {
            //        await _sss.SetItemAsync(JWT_KEY, content.JwtToken);


            //        //// Store the access token in local storage
            //        //var token = jsonObject["token"]?.ToString();
            //        //if (!string.IsNullOrEmpty(token))
            //        //{
            //        //    await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "accessToken", token);
            //        //}

            //        //return jsonObject["result"]?.ToString() == Keys.API_LOGIN_SUCC ?
            //        //    (jsonObject, true) : (jsonObject, false);
            //    }

            //    return (new JObject { ["result"] = Keys.API_GENERIC_SUCC }, false);
            //}
            ////
            //// LOGIN FAIL
            ////
            //else
            //{
            //    if (result != null)
            //    {
            //        JObject jsonObject;
            //        try
            //        {
            //            jsonObject = JObject.Parse(result);
            //        }
            //        catch (Newtonsoft.Json.JsonException)
            //        {
            //            jsonObject = new JObject { ["errors"] = result };
            //        }
            //        if (jsonObject != null)
            //        {
            //            return (jsonObject, false);

            //        }
            //    }

            //    return (new JObject { ["result"] = Keys.API_GENERIC_FAIL }, false);
            //}
        }


        /***************************************************
         * CHECKOUT BOOK
         **************************************************/
        public async Task<(JObject, bool)> CheckoutBookAsync(RegisterDefaultModel model)
        {
            await UpdateHttpClientAsync();
            return (null, false);
        }


        /***************************************************
         * REGISTER
         * *************************************************/
        public async Task<(JObject, bool)> RegisterAsync(RegisterDefaultModel model)
        {
            await UpdateHttpClientAsync();
            var response = await _httpClient.PostAsJsonAsync("api/Api/Register", model);

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
        }


        /***************************************************
         * CREATE BOOK
         * *************************************************/
        public async Task<JObject> CreateBookAsync(BookModel model)
        {
            await UpdateHttpClientAsync();
            var response = await _httpClient.PostAsJsonAsync("api/Api/CreateBook", model);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (result != null)
                {
                    JObject jsonObject = JObject.Parse(result);
                    if (jsonObject != null)
                    {
                        return jsonObject;
                    }
                }

                return new JObject { ["result"] = Keys.API_GENERIC_SUCC }; ;
            }
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
                        return jsonObject;
                    }
                }

                return new JObject { ["errors"] = Keys.API_GENERIC_FAIL };
            }
        }


        /***************************************************
         * GET CHAPTERS
         * *************************************************/
        public async Task<(List<ChapterDto>, string)> GetChapters(string dbName)
        {
            List<ChapterDto> chapters = null;
            await UpdateHttpClientAsync();

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Api/FetchChapters", dbName);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    chapters = JsonConvert.DeserializeObject<List<ChapterDto>>(content);
                    return (chapters, null);
                }
                else if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return (new List<ChapterDto>(), null);
                }
                else
                {
                    // Handle HTTP error response
                    return (null, $"Failed to fetch chapters. Status code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Handle other exceptions
                return (null, $"Error fetching chapters: {ex.Message}");
            }
        }

        /***************************************************
         * GET CHAPTERS
         * *************************************************/
        public async Task<(JObject, bool)> AddChapter(AddChapterRequest request)
        {
            await UpdateHttpClientAsync();
            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Api/AddChapter", request);
                var result = await response.Content.ReadAsStringAsync();
                if (result != null)
                {
                    JObject jsonObject = JObject.Parse(result);
                    if (jsonObject != null)
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            return (new JObject { ["result"] = jsonObject["result"] }, true);
                        }
                        else
                        {
                            return (new JObject { ["errors"] = jsonObject["errors"] }, false);
                        }
                    }
                }

                return (null, false);
            }
            catch (Exception ex)
            {
                return (null, false);
            }
        }

        public async Task<bool> ValidateTokenAsync()
        {
            await UpdateHttpClientAsync();


            // Custom logic to validate the token, possibly making a request to an API
            var response = await _httpClient.PostAsync("api/Api/ValidateToken", null);
            return response.IsSuccessStatusCode;
        }

        private async Task UpdateHttpClientAsync()
        {
            var token = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", "accessToken");

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
    }
}
