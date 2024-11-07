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
        [ClassData(typeof(RegisterRequestTestData))]
        public async Task RegisterUser(RegisterRequest request, int status)
        {
            var requestUrl = $"/api/auth/register";
            var response = await _client.PostAsJsonAsync(requestUrl, request);
            var content = await response.Content.ReadFromJsonAsync<ApiResponse>();
            Assert.Equal(status, (int)response.StatusCode);
        }


        [Theory]
        [ClassData(typeof(DeleteRequestTestData))]
        public async Task DeleteUser(string username, int status)
        {
            var requestUrl = $"/api/library/deletebook";

            // Generate a JWT with necessary claims
            var token = TokenGenerator.GenerateTestJwtToken(username);

            // Add the generated token to the Authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await _client.DeleteAsync(requestUrl);
            // Assert
            Assert.Equal(status, (int)response.StatusCode);
            // Additional assertions as needed
        }

    }

    public class RegisterRequestTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {

                new RegisterRequest
                {
                    FirstName = "Alice",
                    LastName = "Smith",
                    Email = "alice.smith@example.com",
                    ConfirmEmail = "alice.smith@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                StatusCodes.Status409Conflict
            };

            yield return new object[]
            {
                new RegisterRequest
                {
                    FirstName = " ",
                    LastName = "Doe",
                    Email = "jane.doe@example.com",
                    ConfirmEmail = "jane.doe@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                },
                false
            };
        }

        // Explicit implementation for the non-generic IEnumerator
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class DeleteRequestTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                "alice.smith@example.com",
                StatusCodes.Status409Conflict
            };
        }

        // Explicit implementation for the non-generic IEnumerator
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
