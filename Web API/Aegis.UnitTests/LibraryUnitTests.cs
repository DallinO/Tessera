using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Tessera.Models.Authentication;

namespace Aegis.UnitTests.Book
{
    public class LibraryControllerTests : IClassFixture<WebApplicationFactory<Aegis.Api.Program>>
    {
        private readonly HttpClient _client;

        public LibraryControllerTests(WebApplicationFactory<Aegis.Api.Program> factory)
        {
            _client = factory.CreateClient();
        }


        [Theory]
        [ClassData(typeof(CreateBookTestData))]
        public async Task CreateBook(string username, int status)
        {
            // Arrange
            var requestUrl = "/api/library/createbook";

            // Generate a JWT with necessary claims
            var token = TokenGenerator.GenerateTestJwtToken(username);

            // Add the generated token to the Authorization header
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.PostAsync(requestUrl, null);

            // Assert
            response.EnsureSuccessStatusCode(); // Should return Status Code 200-299
        }
    }

    public class CreateBookTestData : IEnumerable<object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return new object[]
            {
                "alice.smith@example.com", // Email string for the first case
                StatusCodes.Status409Conflict
            };

            yield return new object[]
            {
                "jane.doe@example.com", // Email string for the second case
                false
            };
        }

        // Explicit implementation for the non-generic IEnumerator
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


}

