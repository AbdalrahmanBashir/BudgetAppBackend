using System.Security.Claims;
using BudgetAppBackend.Application.DTOs.AuthenticationDTOs;
using BudgetAppBackend.Application.Features.Authentication.DeleteAccount;
using BudgetAppBackend.Application.Features.Authentication.Login;
using BudgetAppBackend.Application.Features.Authentication.RefToken;
using BudgetAppBackend.Application.Features.Authentication.Registration;
using BudgetAppBackend.Application.Features.Authentication.ResetPassword;
using BudgetAppBackend.Application.Features.Authentication.SendVerificationCode;
using BudgetAppBackend.Application.Features.Authentication.VerifyEmail;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetAppBackend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ISender Sender;

        public AuthController(ISender sender)
        {
            Sender = sender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] AddUserDto addUserDto, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(new RegisterUserCommand { AddUser = addUserDto }, cancellationToken);
            return CreatedAtAction(nameof(Register), new { userId = result.UserId }, result);
        }


        [HttpPost("login")]
        public async Task<ActionResult<AuthResult>> Login([FromBody] LogUserDto LogUserDto, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(new LoginCommand { LogUser = LogUserDto }, cancellationToken);

            HttpContext.Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.RefreshTokenExpiry
            });
            
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<AuthResult>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto, CancellationToken cancellationToken)
        {
            var refreshTokenFromCookie = HttpContext.Request.Cookies["RefreshToken"];
          
            var result = await Sender.Send(new RefreshTokenCommand { Token = refreshTokenFromCookie, Email = refreshTokenDto.Email }, cancellationToken);

            Response.Cookies.Append("RefreshToken", result.RefreshToken, new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None,
                Expires = result.RefreshTokenExpiry
            });
            return Ok(result);
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto, CancellationToken cancellationToken)
        {
           var result = await Sender.Send(new ResetPasswordCommand { resetPassword = resetPasswordDto }, cancellationToken);
            return Ok(result);
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailDto verifyEmailDto, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(new VerifyEmailCommand { VerifyEmailDto = verifyEmailDto }, cancellationToken);
            return Ok(result);
        }

        [HttpPost("send-verification-code")]
        public async Task<ActionResult> SendVerificationCode([FromBody] SendVerificationCodeDto sendVerificationCodeDto, CancellationToken cancellationToken)
        {
            var result = await Sender.Send(new SendVerificationCodeCommand { SendVerificationCodeDto = sendVerificationCodeDto }, cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("delete-account")]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var results = await Sender.Send(new DeleteAccountCommand { Email = email }, cancellationToken);
            return Ok(results);
        }
    }
}
