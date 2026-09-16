
using TexasMediaDart.Api.Extensions;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var allowedOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        policy
            .SetIsOriginAllowed(origin =>
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    return false;
                }

                var isLocalhost =
                    uri.Host.Equals(
                        "localhost",
                        StringComparison.OrdinalIgnoreCase) ||
                    uri.Host.Equals(
                        "127.0.0.1",
                        StringComparison.OrdinalIgnoreCase);

                var isConfiguredOrigin =
                    allowedOrigins.Contains(
                        origin,
                        StringComparer.OrdinalIgnoreCase);

                return isLocalhost || isConfiguredOrigin;
            })
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("FrontendPolicy");
app.UseHttpsRedirection();

app.MapHealthEndpoints();
app.Run();