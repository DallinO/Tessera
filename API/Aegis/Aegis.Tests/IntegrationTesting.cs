using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using Xunit;
using Aegis; // Update with your actual namespace

//public class RegisterIntegrationTests : IClassFixture<CustomWebApplicationFactory<Program>>
//{
//    private readonly HttpClient _client;

//    public RegisterIntegrationTests(CustomWebApplicationFactory<Program> factory)
//    {
//        _client = factory.CreateClient();
//    }

//    [Fact]
//    public async Task Register_InvalidModel_ReturnsBadRequest()
//    {
//        // Arrange
//        var model = new
//        {
//            FirstName = "", // Invalid FirstName
//            LastName = "Doe",
//            Email = "john.doe@example.com",
//            ConfirmEmail = "john.doe@example.com",
//            Password = "Password1!",
//            ConfirmPassword = "Password1!"
//        };

//        var jsonContent = new StringContent(JsonConvert.SerializeObject(model), Encoding.UTF8, "application/json");

//        // Act
//        var response = await _client.PostAsync("/api/register", jsonContent);

//        // Assert
//        Assert.Equal(System.Net.HttpStatusCode.BadRequest, response.StatusCode);
//    }
//}
