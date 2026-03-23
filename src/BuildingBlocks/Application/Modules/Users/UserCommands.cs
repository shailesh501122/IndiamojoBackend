using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.BuildingBlocks.Application.Modules.Users;

public sealed record RegisterUserCommand(string FullName, string Email, string Password, UserRole Role) : IRequest<UserResponse>;
public sealed record LoginUserCommand(string Email, string Password) : IRequest<AuthResponse>;
public sealed record RefreshAccessTokenCommand(string RefreshToken) : IRequest<AuthResponse>;
public sealed record GoogleLoginPlaceholderCommand(string IdToken) : IRequest<string>;

public sealed class RegisterUserValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}

public sealed class RegisterUserHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IMapper mapper, IDateTimeProvider dateTimeProvider)
    : IRequestHandler<RegisterUserCommand, UserResponse>
{
    public async Task<UserResponse> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        if (await context.Users.AnyAsync(x => x.Email == request.Email.ToLower(), cancellationToken))
        {
            throw new InvalidOperationException("Email already exists.");
        }

        var user = new User(request.FullName, request.Email, passwordHasher.Hash(request.Password), request.Role);
        var freePlan = await context.SubscriptionPlans.FirstAsync(x => x.Type == SubscriptionType.Free, cancellationToken);
        context.Users.Add(user);
        context.UserSubscriptions.Add(new UserSubscription(user.Id, freePlan.Id, dateTimeProvider.UtcNow, dateTimeProvider.UtcNow.AddYears(100)));
        await context.SaveChangesAsync(cancellationToken);
        return mapper.Map<UserResponse>(user);
    }
}

public sealed class LoginUserHandler(IApplicationDbContext context, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    : IRequestHandler<LoginUserCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(LoginUserCommand request, CancellationToken cancellationToken)
    {
        var user = await context.Users.Include(x => x.RefreshTokens).FirstOrDefaultAsync(x => x.Email == request.Email.ToLower(), cancellationToken)
            ?? throw new InvalidOperationException("Invalid credentials.");

        if (!passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new InvalidOperationException("Invalid credentials.");
        }

        var response = jwtTokenService.Generate(user);
        context.RefreshTokens.Add(new RefreshToken(user.Id, response.RefreshToken, response.ExpiresAtUtc.AddDays(7)));
        await context.SaveChangesAsync(cancellationToken);
        return response;
    }
}

public sealed class RefreshAccessTokenHandler(IApplicationDbContext context, IJwtTokenService jwtTokenService)
    : IRequestHandler<RefreshAccessTokenCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(RefreshAccessTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await context.RefreshTokens.Include(x => x.User).FirstOrDefaultAsync(x => x.Token == request.RefreshToken && !x.IsRevoked, cancellationToken)
            ?? throw new InvalidOperationException("Refresh token is invalid.");

        if (refreshToken.ExpiresAtUtc <= DateTime.UtcNow || refreshToken.User is null)
        {
            throw new InvalidOperationException("Refresh token expired.");
        }

        refreshToken.Revoke();
        var response = jwtTokenService.Generate(refreshToken.User);
        context.RefreshTokens.Add(new RefreshToken(refreshToken.UserId, response.RefreshToken, response.ExpiresAtUtc.AddDays(7)));
        await context.SaveChangesAsync(cancellationToken);
        return response;
    }
}

public sealed class GoogleLoginPlaceholderHandler : IRequestHandler<GoogleLoginPlaceholderCommand, string>
{
    public Task<string> Handle(GoogleLoginPlaceholderCommand request, CancellationToken cancellationToken)
        => Task.FromResult("Google login placeholder endpoint is wired. Replace with actual OAuth flow.");
}
