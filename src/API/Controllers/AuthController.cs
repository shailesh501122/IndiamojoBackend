using MediatR;
using Microsoft.AspNetCore.Mvc;
using IndiamojoBackend.BuildingBlocks.Application.Modules.Users;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new RegisterUserCommand(request.FullName, request.Email, request.Password, request.Role), cancellationToken));

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginUserRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new LoginUserCommand(request.Email, request.Password), cancellationToken));

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request, CancellationToken cancellationToken)
        => Ok(await sender.Send(new RefreshAccessTokenCommand(request.RefreshToken), cancellationToken));

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
        => Ok(new { message = await sender.Send(new GoogleLoginPlaceholderCommand(request.IdToken), cancellationToken) });

    public sealed record RegisterUserRequest(string FullName, string Email, string Password, UserRole Role);
    public sealed record LoginUserRequest(string Email, string Password);
    public sealed record RefreshTokenRequest(string RefreshToken);
    public sealed record GoogleLoginRequest(string IdToken);
}
