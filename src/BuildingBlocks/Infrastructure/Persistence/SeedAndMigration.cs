using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Subscriptions;
using IndiamojoBackend.BuildingBlocks.Domain.Modules.Users;

namespace IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence;

public static class SeedAndMigration
{
    public static async Task ApplyMigrationsAndSeedAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Startup");
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        if (!await context.SubscriptionPlans.AnyAsync())
        {
            context.SubscriptionPlans.AddRange(
                new SubscriptionPlan("Free", SubscriptionType.Free, 0, 3, false),
                new SubscriptionPlan("Premium", SubscriptionType.Premium, 1999, 0, true));
        }

        if (!await context.Users.AnyAsync(x => x.Email == "admin@indiamojo.com"))
        {
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            context.Users.Add(new User("System Admin", "admin@indiamojo.com", passwordHasher.Hash("Admin@123"), UserRole.Admin));
        }

        await context.SaveChangesAsync();
        logger.LogInformation("Database migrated and seed data ensured.");
    }
}
