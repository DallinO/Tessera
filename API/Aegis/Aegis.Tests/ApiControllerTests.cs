//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Authentication;
//using Moq;
//using Aegis.Controllers; // Update with your actual namespace
//using Microsoft.AspNetCore.Http;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Tessera.Models;

//public class RegisterControllerTests
//{
//    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
//    private readonly Mock<SignInManager<ApplicationUser>> _signInManagerMock;
//    private readonly ApiController _controller;

//    public RegisterControllerTests()
//    {
//        _userManagerMock = MockUserManager<ApplicationUser>();
//        _signInManagerMock = MockSignInManager<ApplicationUser>(_userManagerMock.Object);
//        _controller = new ApiController(_userManagerMock.Object, _signInManagerMock.Object);
//    }

//    [Fact]
//    public async Task Register_ValidModel_ReturnsOk()
//    {
//        // Arrange
//        var model = new RegisterDefaultModel
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            Email = "john.doe@example.com",
//            ConfirmEmail = "john.doe@example.com",
//            Password = "Password1!",
//            ConfirmPassword = "Password1!"
//        };

//        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), model.Password))
//            .ReturnsAsync(IdentityResult.Success);

//        // Act
//        var result = await _controller.Register(model);

//        // Assert
//        var okResult = Assert.IsType<OkObjectResult>(result);
//        var returnValue = Assert.IsType<dynamic>(okResult.Value);
//        Assert.Equal("User created successfully", returnValue.Result);
//    }

//    [Fact]
//    public async Task Register_InvalidModel_ReturnsBadRequest()
//    {
//        // Arrange
//        var model = new RegisterDefaultModel(); // Invalid model

//        _controller.ModelState.AddModelError("FirstName", "Required");

//        // Act
//        var result = await _controller.Register(model);

//        // Assert
//        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//        var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
//        Assert.True(modelState.ContainsKey("FirstName"));
//    }

//    [Fact]
//    public async Task Register_EmailMismatch_ReturnsBadRequest()
//    {
//        // Arrange
//        var model = new RegisterDefaultModel
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            Email = "john.doe@example.com",
//            ConfirmEmail = "jane.doe@example.com",
//            Password = "Password1!",
//            ConfirmPassword = "Password1!"
//        };

//        // Act
//        var result = await _controller.Register(model);

//        // Assert
//        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//        var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
//        Assert.True(modelState.ContainsKey("ConfirmEmail"));
//    }

//    [Fact]
//    public async Task Register_PasswordMismatch_ReturnsBadRequest()
//    {
//        // Arrange
//        var model = new RegisterDefaultModel
//        {
//            FirstName = "John",
//            LastName = "Doe",
//            Email = "john.doe@example.com",
//            ConfirmEmail = "john.doe@example.com",
//            Password = "Password1!",
//            ConfirmPassword = "Password2!"
//        };

//        // Act
//        var result = await _controller.Register(model);

//        // Assert
//        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
//        var modelState = Assert.IsType<SerializableError>(badRequestResult.Value);
//        Assert.True(modelState.ContainsKey("ConfirmPassword"));
//    }

//    private static Mock<UserManager<TUser>> MockUserManager<TUser>() where TUser : class
//    {
//        var store = new Mock<IUserStore<TUser>>();
//        var mgr = new Mock<UserManager<TUser>>(store.Object, null, null, null, null, null, null, null, null);
//        return mgr;
//    }

//    private static Mock<SignInManager<TUser>> MockSignInManager<TUser>(UserManager<TUser> userManager) where TUser : class
//    {
//        var contextAccessor = new Mock<IHttpContextAccessor>();
//        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<TUser>>();
//        var options = new Mock<IOptions<IdentityOptions>>();
//        var logger = new Mock<ILogger<SignInManager<TUser>>>();
//        var schemes = new Mock<IAuthenticationSchemeProvider>();
//        var confirmation = new Mock<IUserConfirmation<TUser>>();

//        return new Mock<SignInManager<TUser>>(userManager, contextAccessor.Object, claimsFactory.Object, options.Object, logger.Object, schemes.Object, confirmation.Object);
//    }

//}
