using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using Tessera.Constants;
using Tessera.Models.Authentication;
using Tessera.Models.Book;
using Tessera.Models.ChapterComponents;

namespace Tessera
{
    public interface IApiService
    {
        Task<(JObject, bool)> LoginAsync(LoginDefaultModel model);
        Task<(JObject, bool)> RegisterAsync(RegisterDefaultModel model);
        Task<JObject> CreateBookAsync(BookModel model);
        Task<(List<ChapterDto>, string)> GetChapters(string dbName);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        
        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /***************************************************
         * LOGIN
         * *************************************************/
        public async Task<(JObject, bool)> LoginAsync(LoginDefaultModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Api/Login", model);
            //
            // LOGIN SUCCESS
            //
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (result != null)
                {
                    JObject jsonObject = JObject.Parse(result);
                    if (jsonObject != null)
                    {
                        return jsonObject["result"]?.ToString() == Keys.API_LOGIN_SUCC ? 
                            (jsonObject, true) : (jsonObject, false);
                    }
                }

                return (new JObject { ["result"] = Keys.API_GENERIC_SUCC }, false);
            }
            //
            // LOGIN FAIL
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

                return (new JObject { ["result"] = Keys.API_GENERIC_FAIL }, false);
            }
        }


        /***************************************************
         * REGISTER
         * *************************************************/
        public async Task<(JObject, bool)> RegisterAsync(RegisterDefaultModel model)
        {
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
         * CREATE ORGANIZATION
         * *************************************************/
        public async Task<JObject> CreateBookAsync(BookModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Api/CreateOrg", model);
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

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/Api/FetchChapters", dbName);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    chapters = JsonConvert.DeserializeObject<List<ChapterDto>>(content);
                    return (chapters, null);
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
    }

}
