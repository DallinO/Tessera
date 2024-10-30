using Swashbuckle.AspNetCore.Filters;
using Tessera.Models.Authentication;

namespace Aegis.SwaggerTest
{
    public class CheckInRequestExample : IExamplesProvider<LoginRequest>
    {
        public LoginRequest GetExamples()
        {
            return new LoginRequest
            {
                Email = "td1@test.com",
                Password = "Test@123",
                RememberMe = true
            };
        }

    }

    public class RegisterRequestExample : IExamplesProvider<RegisterRequest>
    {
        public RegisterRequest GetExamples()
        {
            return new RegisterRequest
            {
                FirstName = "TestDummy",
                LastName = "Nine",
                Email = "td9@test.com",
                ConfirmEmail = "td9@test.com",
                Password = "Test@123",
                ConfirmPassword = "Test@123"
            };
        }
    }
}
