using EHRS.Api.Localization;
using EHRS.Api.Services;
using EHRS.Core.Abstractions.Queries;
using EHRS.Core.Common;
using EHRS.Core.DTOs.Auth;
using EHRS.Core.Requests.PatientAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EHRS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PatientAuthController : ControllerBase
{
    private readonly IPatientAuthQueries _queries;
    private readonly JwtTokenService _jwt;
    private readonly IAppLocalizer _t;

    public PatientAuthController(
        IPatientAuthQueries queries,
        JwtTokenService jwt,
        IAppLocalizer t)
    {
        _queries = queries;
        _jwt = jwt;
        _t = t;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] PatientRegisterRequest request)
    {
        var (success, error, user) = await _queries.RegisterAsync(request);

        if (!success)
            return BadRequest(new { message = MapRegisterError(error) });

        try
        {
            var (token, exp) = _jwt.CreateToken(
                user!.UserId,
                user.Role,
                user.FullName,
                user.Email,
                rememberMe: false);

            return Ok(new AuthTokenDto
            {
                AccessToken = token,
                ExpiresInMinutes = exp,
                User = user
            });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500, new { message = _t["Common_UnexpectedError"] });
        }
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")] // 👈 Rate Limiting هنا
    public async Task<IActionResult> Login([FromBody] PatientLoginRequest request)
    {
        var (success, error, user) = await _queries.LoginAsync(request);

        if (!success)
        {
            return error switch
            {
                PatientAuthError.InvalidCredentials =>
                    Unauthorized(new { message = _t["Auth_InvalidCredentials"] }),
                _ =>
                    BadRequest(new { message = _t["Auth_InvalidCredentials"] })
            };
        }

        try
        {
            var (token, exp) = _jwt.CreateToken(
                user!.UserId,
                user.Role,
                user.FullName,
                user.Email,
                request.RememberMe);

            return Ok(new AuthTokenDto
            {
                AccessToken = token,
                ExpiresInMinutes = exp,
                User = user
            });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(500, new { message = _t["Common_UnexpectedError"] });
        }
    }

    private string MapRegisterError(PatientAuthError error)
    {
        return error switch
        {
            PatientAuthError.PasswordsDoNotMatch => _t["Auth_PasswordsDoNotMatch"],
            PatientAuthError.EmailAlreadyExists => _t["Auth_EmailExists"],
            PatientAuthError.NationalIdAlreadyExists => _t["Auth_NationalIdExists"],
            _ => _t["Auth_RegisterFailed"]
        };
    }
}