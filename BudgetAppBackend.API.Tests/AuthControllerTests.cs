using Moq;
using FluentAssertions;
using BudgetAppBackend.API.Controllers;
using BudgetAppBackend.Application.Features.Authentication.Login;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Microsoft.AspNetCore.Http;
using BudgetAppBackend.Application.Features.Authentication.Registration;
using BudgetAppBackend.Application.Features.Authentication.DeleteAccount;
using System.Security.Claims;
using BudgetAppBackend.Application.Features.Authentication.ResetPassword;
using BudgetAppBackend.Application.Features.Authentication.VerifyEmail;
using BudgetAppBackend.Application.Features.Authentication.SendVerificationCode;
using BudgetAppBackend.Application.Features.Authentication.RefToken;

namespace BudgetAppBackend.API.Tests
{
    public class AuthControllerTests
    {
        private readonly Mock<ISender> _mockSender;
        private readonly AuthController _controller;
        private readonly Mock<IResponseCookies> _mockCookies;

        public AuthControllerTests()
        {
            _mockSender = new Mock<ISender>();
            _mockCookies = new Mock<IResponseCookies>();

            var mockHttpContext = new Mock<HttpContext>();
            var mockResponse = new Mock<HttpResponse>();

            mockResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
            mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

            _controller = new AuthController(_mockSender.Object)
            {
                ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object }
            };
        }

        [Fact]
        public async Task Login_WhenCalled_ReturnsOkObjectResult_WithAuthResult()
        {
            // Arrange
            var loginDto = new LogUserDto { Email = "Admin@example.com", Password = "Root@ssword123" };
            var authResult = new AuthResult
            {
                RefreshToken = "d53a5f64-91be-4b2e-a937-bc93ef45d8a1",
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"
            };

            _mockSender.Setup(s => s.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(authResult);

            // Act
            var result = await _controller.Login(loginDto, CancellationToken.None);

            // Assert
            result.Result.Should().NotBeNull();

            var okResult = result.Result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(authResult);

            // Verify that the cookie was set
            _mockCookies.Verify(c => c.Append(
                It.Is<string>(s => s == "RefreshToken"),
                It.Is<string>(s => s == authResult.RefreshToken),
                It.IsAny<CookieOptions>()),
                Times.Once);
        }

        [Fact]
        public async Task Register_WhenCalled_ReturnsCreatedAtActionResult()
        {
            // Arrange:
            var addUserDto = new AddUserDto
            {
                FirstName = "John",
                LastName = "Smith",
                Email = "johnS@gmail.com",
                Password = "SecurePass!123",
                Confirmpassword = "SecurePass!123"
            };

            var registerResponse = new AuthResult
            {
                UserId = Guid.NewGuid()
            };

            _mockSender.Setup(s => s.Send(It.IsAny<RegisterUserCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(registerResponse);

            // Act
            var result = await _controller.Register(addUserDto, CancellationToken.None);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();

            var createdAtResult = result as CreatedAtActionResult;
            createdAtResult.StatusCode.Should().Be(201);
            createdAtResult.ActionName.Should().Be(nameof(AuthController.Register));
            createdAtResult.Value.Should().BeEquivalentTo(registerResponse);
        }

        [Fact]
        public async Task DeleteAccount_WhenCalled_ReturnsOkObjectResult()
        {
            // Arrange
            var claims = new List<Claim> { new Claim(ClaimTypes.Email, "testingUser@gmail.com") };
            var identity = new ClaimsIdentity(claims);
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var mockHttpContext = new Mock<HttpContext>();
            mockHttpContext.Setup(ctx => ctx.User).Returns(claimsPrincipal);

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            _mockSender.Setup(s => s.Send(It.IsAny<DeleteAccountCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteAccount(CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>(); 
        }

        [Fact]
        public async Task RefreshToken_WhenCalled_ReturnsOkObjectResult()
        {
            // Arrange
            var refreshTokenDto = new RefreshTokenDto { Email = "testingUser@gmail.com" };
            var authResult = new AuthResult { RefreshToken = "d53a5f64-91be-4b2e-a937-bc93ef45d8a1" };

            var mockRequestCookies = new Mock<IRequestCookieCollection>();
            mockRequestCookies.Setup(c => c["RefreshToken"]).Returns("existing-refresh-token");

            var mockHttpContext = new Mock<HttpContext>();
            var mockRequest = new Mock<HttpRequest>();
            var mockResponse = new Mock<HttpResponse>();

            mockRequest.Setup(r => r.Cookies).Returns(mockRequestCookies.Object);
            mockResponse.Setup(r => r.Cookies).Returns(_mockCookies.Object);
            mockHttpContext.Setup(ctx => ctx.Request).Returns(mockRequest.Object);
            mockHttpContext.Setup(ctx => ctx.Response).Returns(mockResponse.Object);

            _controller.ControllerContext = new ControllerContext { HttpContext = mockHttpContext.Object };

            _mockSender.Setup(s => s.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(authResult);

            // Act
            var result = await _controller.RefreshToken(refreshTokenDto, CancellationToken.None);

            // Assert
            result.Result.Should().BeOfType<OkObjectResult>();
            (result.Result as OkObjectResult).Value.Should().BeEquivalentTo(authResult);
        }


        [Fact]
        public async Task ResetPassword_WhenCalled_ReturnsOkObjectResult()
        {

            // Arrange
            var resetPasswordDto = new ResetPasswordDto 
            { 
                Email = "testingUser@gmail.com",
                NewPassword = "Password1234", 
                ConfirmNewPassword= "Password1234" 
            };

            var authResult = new AuthResult { Success = true, Message = "d53a5f64-91be-4b2e-a937-bc93ef45d8a1" };
            _mockSender.Setup(s => s.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(authResult);

            // Act
            var result = await _controller.ResetPassword(resetPasswordDto, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task SendVerificationCode_WhenCalled_ReturnsOkObjectResult()
        {
            // Arrange
            var sendVerificationCodeDto = new SendVerificationCodeDto { Email = "testingUser@gmail.com" };
            _mockSender.Setup(s => s.Send(It.IsAny<SendVerificationCodeCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(true);

            // Act
            var result = await _controller.SendVerificationCode(sendVerificationCodeDto, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

        [Fact]
        public async Task VerifyEmail_WhenCalled_ReturnsOkObjectResult()
        {
            // Arrange
            var verifyEmailDto = new VerifyEmailDto { Email = "testingUser@gmail.com", Code = "123456" };
            var AuthResult = new AuthResult
            {
                Success = true,
                Message = "Your email has been successfully verified! Welcome to BudgetApp. " +
              "You can now log in and start managing your finances with ease. "
            };
            _mockSender.Setup(s => s.Send(It.IsAny<VerifyEmailCommand>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(AuthResult);

            // Act
            var result = await _controller.VerifyEmail(verifyEmailDto, CancellationToken.None);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
        }

    }

}
