using System.Net.Http.Json;
using Tessera.Models;
using Tessera.Constants;
using Newtonsoft.Json.Linq;

namespace Tessera
{
    public interface IApiService
    {
        Task<JObject> Login(LoginDefaultModel model);
        Task<string> Register(RegisterDefaultModel model);
        // Define other methods as needed
    }

    public class ApiResponse
    {
        public string Result { get; set; }
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Register(RegisterDefaultModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Api/Register", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                if (result != null)
                {
                    JObject jsonObject = JObject.Parse(result);
                    if (jsonObject != null)
                    {
                        return jsonObject["result"]?.ToString() ?? Keys.API_REG_SUCC;

                    }
                }

                return Keys.API_GENERIC_SUCC;
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                if (errorResult != null)
                {
                    JObject jsonObject = JObject.Parse(errorResult);
                    if (jsonObject != null)
                    {
                        return jsonObject["errors"]?.ToString() ?? Keys.API_REG_FAIL;

                    }
                }

                return Keys.API_GENERIC_FAIL;
            }
        }

        public async Task<JObject> Login(LoginDefaultModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Api/Login", model);

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

                return new JObject{ ["result"] = Keys.API_GENERIC_SUCC };
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                if (errorResult != null)
                {
                    JObject jsonObject = JObject.Parse(errorResult);
                    if (jsonObject != null)
                    {
                        return jsonObject;

                    }
                }

                return new JObject { ["result"] = Keys.API_GENERIC_FAIL };
            }
        }
    }
}
