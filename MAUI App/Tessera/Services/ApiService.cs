using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Tessera_Models;

namespace Tessera
{
    public interface IApiService
    {
        Task<string> Login(LoginDefaultModel model);
        Task<string> Register(RegisterDefaultModel model);
        // Define other methods as needed
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
                return result?.Result ?? "User created successfully"; // Adjust based on your API's response structure
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorResult}";
            }
        }

        public async Task<string> Login(LoginDefaultModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/Api/Login", model);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<dynamic>();
                return result?.Result ?? "User Access Granted"; // Adjust based on your API's response structure
            }
            else
            {
                var errorResult = await response.Content.ReadAsStringAsync();
                return $"Error: {response.StatusCode} - {errorResult}";
            }
        }
    }
}
