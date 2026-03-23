using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Serilog;
using IndiamojoBackend.BuildingBlocks.Application.Common;
using IndiamojoBackend.BuildingBlocks.Infrastructure;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Auth;
using IndiamojoBackend.BuildingBlocks.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

var port = builder.Configuration["PORT"];
if (!string.IsNullOrWhiteSpace(port))
{
    builder.WebHost.UseUrls($"http://+:{port}");
}

builder.Host.UseSerilog((context, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Indiamojo Real Estate API", Version = "v1" });
});

var app = builder.Build();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<IExceptionHandlerFeature>();
        var payload = new
        {
            error = feature?.Error.Message,
            details = feature?.Error is ValidationException validationException
                ? validationException.Errors.Select(x => new { x.PropertyName, x.ErrorMessage })
                : null
        };
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    });
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", utc = DateTime.UtcNow }));

await app.Services.ApplyMigrationsAndSeedAsync();

app.Run();
