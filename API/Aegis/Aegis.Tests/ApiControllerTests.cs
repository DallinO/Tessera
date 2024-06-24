using Aegis.Controllers;
using Aegis.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Aegis.Tests
{
    public class ApiControllerTests
    {
        private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
        private readonly Mock<SignInManager<ApplicationUser>> _mockSignInManager;
        private readonly ApiController _controller;

        public ApiControllerTests()
        {
            // Setup mock UserManager
            _mockUserManager = new Mock<UserManager<ApplicationUser>>(
                Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);

            // Setup mock SignInManager
            _mockSignInManager = new Mock<SignInManager<ApplicationUser>>(
                _mockUserManager.Object, Mock.Of<IHttpContextAccessor>(), Mock.Of<IUserClaimsPrincipalFactory<ApplicationUser>>(),
                null, null, null, null);

            // Create controller instance with mocked dependencies
            _controller = new ApiController(_mockUserManager.Object, _mockSignInManager.Object);
        }

        public async Task RegisterAndAssertAsync(RegisterDefaultModel model, string expectedResult)
        {

        }

        [Fact]
        public async Task Register_Should_Create_User()
        {
            // Arrange
            var model = new RegisterDefaultModel
            {
                FirstName = "Test",
                LastName = "User",
                Email = "testuser@example.com",
                ConfirmEmail = "testuser@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };

            _mockUserManager.Setup(u => u.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _controller.Register(model);

            // Assert
            Assert.IsType<OkObjectResult>(result); // Ensure the result is an OkObjectResult
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);

            var expectedResult = "User created successfully";
            var actualResult = okResult.Value?.GetType().GetProperty("Result")?.GetValue(okResult.Value, null) as string;

            Assert.Equal(expectedResult, actualResult);

            // Clean up (optional)
            // You may need to clean up if necessary, like deleting the test user from the mock UserManager
        }


        /**************************************************
         * FIRST NAME CONSTRAINTS 
         * - Expected Outcome: FAIL
         * - Reason: SPECIAL CHARACTERS
         *************************************************/
        [Fact]
        public async Task First_Name_Constraints_1()
        {
            var model = new RegisterDefaultModel
            {
                FirstName = "Test@123",
                LastName = "User",
                Email = "testuser@example.com",
                ConfirmEmail = "testuser@example.com",
                Password = "Test@1234",
                ConfirmPassword = "Test@1234"
            };
        }
    }
}
