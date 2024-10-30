using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Tessera.Models.Authentication;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Json;

public class BooksControllerTests : IClassFixture<WebApplicationFactory<Aegis.Api.Program>>
{
    private readonly HttpClient _client;

    public BooksControllerTests(WebApplicationFactory<Aegis.Api.Program> factory)
    {
        _client = factory.CreateClient();
    }


    [Theory]
    [ClassData(typeof(RegisterRequestTestData))]
    public async Task RegisterUser_ReturnsOk(RegisterRequest request)
    {
        // Arrange
        var bookId = 1; // Assume this book exists in your test database
        var requestUrl = $"/api/Library/deletebook?bookId={bookId}";

        // Act
        var response = await _client.DeleteAsync(requestUrl);
        var content = await response.Content.ReadFromJsonAsync<ApiResponse>();

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        // Additional assertions as needed
    }


    [Fact]
    public async Task DeleteBook_ReturnsOk_WhenBookExists()
    {
        // Arrange
        var bookId = 1; // Assume this book exists in your test database
        var requestUrl = $"/api/Library/deletebook?bookId={bookId}";

        // Act
        var response = await _client.DeleteAsync(requestUrl);

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299
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
            true
        };

        yield return new object[]
        {
            new RegisterRequest
            {
                FirstName = "",
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