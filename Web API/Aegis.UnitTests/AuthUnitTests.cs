using Microsoft.AspNetCore.Mvc.Testing;
using Tessera.Models.Authentication;
using System.Collections;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace Aegis.UnitTests.Auth
{
    public class AuthControllerTests : IClassFixture<WebApplicationFactory<Aegis.Api.Program>>
    {
        private readonly HttpClient _client;

        public AuthControllerTests(WebApplicationFactory<Aegis.Api.Program> factory)
        {
            _client = factory.CreateClient();
        }


        [Theory]
        [ClassData(typeof(RegisterTestCases))]
        public async Task RegisterUser(RegisterRequest request, int status)
        {
            var requestUrl = $"/api/auth/register";
            var response = await _client.PostAsJsonAsync(requestUrl, request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Equal(status, (int)response.StatusCode);
        }


        [Theory]
        [ClassData(typeof(DeleteTestData))]
        public async Task DeleteUser(string username, int status)
        {
            var requestUrl = $"/api/auth/deleteuser";

            // Generate a JWT with necessary claims
            var token = TokenGenerator.GenerateTestJwtToken(username);
            Console.WriteLine(token);

            // Add the generated token to the Authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync(requestUrl);
            // Assert
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Equal(status, (int)response.StatusCode);

            // Additional assertions as needed
        }

    }
}
