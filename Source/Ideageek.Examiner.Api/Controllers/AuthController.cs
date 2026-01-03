using Ideageek.Examiner.Api.Helpers;
using Ideageek.Examiner.Api.Models;
using Ideageek.Examiner.Core.Dtos;
using Ideageek.Examiner.Core.Options;
using Ideageek.Examiner.Core.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Ideageek.Examiner.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AutoAuthOptions _autoAuthOptions;

    public AuthController(IUserService userService, IJwtTokenGenerator jwtTokenGenerator, IOptions<AutoAuthOptions> autoAuthOptions)
    {
        _userService = userService;
        _jwtTokenGenerator = jwtTokenGenerator;
        _autoAuthOptions = autoAuthOptions.Value;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> Login([FromBody] LoginRequestDto request)
    {
        var response = await AuthenticateAsync(request.Username, request.Password);
        if (response is null)
        {
            return this.ApiUnauthorized<AuthResponseDto>("Invalid credentials.");
        }

        AppendAuthCookie(response.Token, response.ExpiresAt);
        return this.ApiOk(response);
    }

    [AllowAnonymous]
    [HttpGet("refresh-token")]
    public async Task<ActionResult<ApiResponse<AuthResponseDto>>> RefreshToken()
    {
        if (string.IsNullOrWhiteSpace(_autoAuthOptions.Username) || string.IsNullOrWhiteSpace(_autoAuthOptions.Password))
        {
            return this.ApiBadRequest<AuthResponseDto>("Auto-login credentials are not configured.");
        }

        var response = await AuthenticateAsync(_autoAuthOptions.Username, _autoAuthOptions.Password);
        if (response is null)
        {
            return this.ApiUnauthorized<AuthResponseDto>("Invalid credentials.");
        }

        AppendAuthCookie(response.Token, response.ExpiresAt);
        return this.ApiOk(response);
    }

    private async Task<AuthResponseDto?> AuthenticateAsync(string username, string password)
    {
        var user = await _userService.ValidateCredentialsAsync(username, password);
        if (user is null)
        {
            return null;
        }

        return _jwtTokenGenerator.GenerateToken(user);
    }

    private void AppendAuthCookie(string token, DateTime expiresAt)
    {
        if (!_autoAuthOptions.SetCookie || string.IsNullOrWhiteSpace(_autoAuthOptions.CookieName))
        {
            return;
        }

        Response.Cookies.Append(_autoAuthOptions.CookieName, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = expiresAt
        });
    }
}
