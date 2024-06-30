using System.Net.Http.Json;
using Tessera.Models;
using Tessera.Constants;
using Newtonsoft.Json.Linq;

namespace Tessera
{
    public interface IApiService
    {
        Task<JObject> Login(LoginDefaultModel model);
        Task<JObject> Register(RegisterDefaultModel model);
        Task<JObject> CreateOrg(OrganizationModel mnodel);
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

        public async Task<JObject> Register(RegisterDefaultModel model)
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
                        return jsonObject;

                    }
                }

                return new JObject { ["result"] = Keys.API_GENERIC_SUCC }; ;
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
                        return jsonObject;
                    }
                }

                return new JObject { ["errors"] = Keys.API_GENERIC_FAIL };
            }
        }

        public async Task<JObject> Login(LoginDefaultModel model)
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
                        return jsonObject;

                    }
                }

                return new JObject{ ["result"] = Keys.API_GENERIC_SUCC };
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
                        return jsonObject;

                    }
                }

                return new JObject { ["result"] = Keys.API_GENERIC_FAIL };
            }
        }

        public async Task<JObject> CreateOrg(OrganizationModel model)
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
    }
}
