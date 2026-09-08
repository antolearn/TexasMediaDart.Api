using System.Reflection;

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

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/health/api", () =>
{
    return Results.Ok(new
    {
        status = "Healthy",
        service = "TexasMediaDart.Api",
        timestampUtc = DateTime.UtcNow
    });
});
app.MapGet("/health/db", async (IConfiguration configuration) =>
{
    var connectionString =
        configuration.GetConnectionString("DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        return Results.Problem(
            title: "Database health check failed",
            detail: "DefaultConnection is not configured.",
            statusCode: StatusCodes.Status500InternalServerError);
    }

    try
    {
        await using var connection =
            new Microsoft.Data.SqlClient.SqlConnection(connectionString);

        await connection.OpenAsync();

        await using var command =
            new Microsoft.Data.SqlClient.SqlCommand(
                "SELECT DB_NAME()",
                connection);

        var databaseName = await command.ExecuteScalarAsync();

        return Results.Ok(new
        {
            status = "Healthy",
            database = databaseName,
            timestampUtc = DateTime.UtcNow
        });
    }
    catch (Exception)
    {
        return Results.Problem(
            title: "Database health check failed",
            detail: "Unable to connect to the database.",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});
app.MapGet("/health/version", (IHostEnvironment environment) =>
{
    var assembly = Assembly.GetExecutingAssembly();

    var version =
        assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? "unknown";

    return Results.Ok(new
    {
        application = "TexasMediaDart.Api",
        version,
        environment = environment.EnvironmentName,
        timestampUtc = DateTime.UtcNow
    });
});
app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
