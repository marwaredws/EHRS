using EHRS.Api.Localization;
using EHRS.Api.Services;
using EHRS.Core.Abstractions.Queries;
using EHRS.Core.Common;
using EHRS.Core.DTOs.Auth;
using EHRS.Core.Requests.DoctorAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting; // 👈 مهم

namespace EHRS.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DoctorAuthController : ControllerBase
{
    private readonly IDoctorAuthQueries _queries;
    private readonly JwtTokenService _jwt;
    private readonly IAppLocalizer _loc;

    public DoctorAuthController(IDoctorAuthQueries queries, JwtTokenService jwt, IAppLocalizer loc)
    {
        _queries = queries;
        _jwt = jwt;
        _loc = loc;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] DoctorRegisterRequest request)
    {
        var (success, error) = await _queries.RegisterAsync(request);

        if (!success)
            return BadRequest(new { message = MapRegisterError(error) });

        return Ok(new { message = _loc["Auth_RegisterSuccess"] });
    }

    [HttpPost("login")]
    [EnableRateLimiting("LoginPolicy")] // 👈 إضافة الريت ليميت هنا
    public async Task<IActionResult> Login([FromBody] DoctorLoginRequest request)
    {
        var (success, error, user) = await _queries.LoginAsync(request);

        if (!success)
        {
            return error switch
            {
                DoctorAuthError.InvalidCredentials =>
                    Unauthorized(new { message = _loc["Auth_InvalidCredentials"] }),

                DoctorAuthError.PendingApproval =>
                    StatusCode(403, new { message = _loc["Auth_DoctorPending"] }),

                DoctorAuthError.Rejected =>
                    StatusCode(403, new { message = _loc["Auth_DoctorRejected"] }),

                _ =>
                    BadRequest(new { message = _loc["Auth_RegisterFailed"] })
            };
        }

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

    private string MapRegisterError(DoctorAuthError error)
    {
        return error switch
        {
            DoctorAuthError.PasswordsDoNotMatch => _loc["Auth_PasswordsDoNotMatch"],
            DoctorAuthError.EmailAlreadyExists => _loc["Auth_EmailExists"],
            DoctorAuthError.MedicalLicenseAlreadyExists => _loc["Auth_MedicalLicenseExists"],
            _ => _loc["Auth_RegisterFailed"]
        };
    }
}
