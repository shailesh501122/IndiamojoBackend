using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Auth;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Services;

namespace IndiamojoBackend.BuildingBlocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Database connection string is missing.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseNpgsql(connectionString, sql =>
            sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? throw new InvalidOperationException("JWT options are missing.");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization();
        services.AddHttpContextAccessor();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<ISubscriptionPolicyService, SubscriptionPolicyService>();

        var redisConnection = configuration.GetValue<string>("Redis:ConnectionString");
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options => options.Configuration = redisConnection);
        }
        else
        {
            services.AddDistributedMemoryCache();
        }

        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
}
