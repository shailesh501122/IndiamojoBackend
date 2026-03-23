using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "notifications",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Channel = table.Column<int>(nullable: false),
                Subject = table.Column<string>(nullable: false),
                Message = table.Column<string>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_notifications", x => x.Id));

        migrationBuilder.CreateTable(
            name: "payments",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Amount = table.Column<decimal>(type: "numeric", nullable: false),
                Gateway = table.Column<int>(nullable: false),
                Status = table.Column<int>(nullable: false),
                Reference = table.Column<string>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_payments", x => x.Id));

        migrationBuilder.CreateTable(
            name: "subscription_plans",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(nullable: false),
                Type = table.Column<int>(nullable: false),
                Price = table.Column<decimal>(type: "numeric", nullable: false),
                ListingLimit = table.Column<int>(nullable: false),
                IsFeaturedEnabled = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_subscription_plans", x => x.Id));

        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                FullName = table.Column<string>(maxLength: 120, nullable: false),
                Email = table.Column<string>(maxLength: 255, nullable: false),
                PasswordHash = table.Column<string>(nullable: false),
                Role = table.Column<int>(nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "bookings",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                PropertyId = table.Column<Guid>(nullable: false),
                TenantId = table.Column<Guid>(nullable: false),
                VisitDateUtc = table.Column<DateTime>(nullable: false),
                Status = table.Column<int>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_bookings", x => x.Id));

        migrationBuilder.CreateTable(
            name: "properties",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                OwnerId = table.Column<Guid>(nullable: false),
                Title = table.Column<string>(nullable: false),
                Description = table.Column<string>(nullable: false),
                Type = table.Column<int>(nullable: false),
                BHK = table.Column<int>(nullable: false),
                Price = table.Column<decimal>(type: "numeric", nullable: false),
                city = table.Column<string>(maxLength: 100, nullable: false),
                latitude = table.Column<decimal>(type: "numeric", nullable: false),
                longitude = table.Column<decimal>(type: "numeric", nullable: false),
                Status = table.Column<int>(nullable: false),
                IsFeatured = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_properties", x => x.Id));

        migrationBuilder.CreateTable(
            name: "refresh_tokens",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Token = table.Column<string>(nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(nullable: false),
                IsRevoked = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_refresh_tokens", x => x.Id);
                table.ForeignKey("FK_refresh_tokens_users_UserId", x => x.UserId, "users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "reviews",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                PropertyId = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                Rating = table.Column<int>(nullable: false),
                Comment = table.Column<string>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table => table.PrimaryKey("PK_reviews", x => x.Id));

        migrationBuilder.CreateTable(
            name: "user_subscriptions",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                UserId = table.Column<Guid>(nullable: false),
                SubscriptionPlanId = table.Column<Guid>(nullable: false),
                StartDateUtc = table.Column<DateTime>(nullable: false),
                ExpiryDateUtc = table.Column<DateTime>(nullable: false),
                IsActive = table.Column<bool>(nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_user_subscriptions", x => x.Id);
                table.ForeignKey("FK_user_subscriptions_subscription_plans_SubscriptionPlanId", x => x.SubscriptionPlanId, "subscription_plans", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "property_amenities",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                PropertyId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 120, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_property_amenities", x => x.Id);
                table.ForeignKey("FK_property_amenities_properties_PropertyId", x => x.PropertyId, "properties", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "property_images",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                PropertyId = table.Column<Guid>(nullable: false),
                Url = table.Column<string>(maxLength: 1000, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                UpdatedAtUtc = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_property_images", x => x.Id);
                table.ForeignKey("FK_property_images_properties_PropertyId", x => x.PropertyId, "properties", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex("IX_users_Email", "users", "Email", unique: true);
        migrationBuilder.CreateIndex("IX_refresh_tokens_Token", "refresh_tokens", "Token", unique: true);
        migrationBuilder.CreateIndex("IX_refresh_tokens_UserId", "refresh_tokens", "UserId");
        migrationBuilder.CreateIndex("IX_properties_city", "properties", "city");
        migrationBuilder.CreateIndex("IX_properties_Price", "properties", "Price");
        migrationBuilder.CreateIndex("IX_properties_Type", "properties", "Type");
        migrationBuilder.CreateIndex("IX_property_amenities_PropertyId", "property_amenities", "PropertyId");
        migrationBuilder.CreateIndex("IX_property_images_PropertyId", "property_images", "PropertyId");
        migrationBuilder.CreateIndex("IX_user_subscriptions_SubscriptionPlanId", "user_subscriptions", "SubscriptionPlanId");
        migrationBuilder.CreateIndex("IX_user_subscriptions_UserId_IsActive", "user_subscriptions", new[] { "UserId", "IsActive" });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("bookings");
        migrationBuilder.DropTable("notifications");
        migrationBuilder.DropTable("payments");
        migrationBuilder.DropTable("property_amenities");
        migrationBuilder.DropTable("property_images");
        migrationBuilder.DropTable("refresh_tokens");
        migrationBuilder.DropTable("reviews");
        migrationBuilder.DropTable("user_subscriptions");
        migrationBuilder.DropTable("properties");
        migrationBuilder.DropTable("users");
        migrationBuilder.DropTable("subscription_plans");
    }
}
