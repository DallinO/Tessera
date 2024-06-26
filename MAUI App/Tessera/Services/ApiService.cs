using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tessera_Models;
using Newtonsoft.Json.Linq;

namespace Tessera
{
    public interface IApiService
    {
        Task<string> Login(LoginDefaultModel model);
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
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                Console.WriteLine(result);
                return result?.Result ?? "User created successfully"; // Adjust based on your API's response structure
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                JObject jsonObject =  JObject.Parse(errorResult);
                string result = jsonObject["result"]?.ToString();
                return $"Error: {response.StatusCode} - {errorResult}";
            }
        }

        public async Task<string> Login(LoginDefaultModel model)
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
                        return jsonObject["result"]?.ToString() ?? "Generic Success";

                    }
                }

                return "Generic Success";
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                if (errorResult != null)
                {
                    JObject jsonObject = JObject.Parse(errorResult);
                    if (jsonObject != null)
                    {
                        return jsonObject["errors"]?.ToString() ?? "Generic Error";

                    }
                }

                return "Generic Error";
            }
        }
    }
}
