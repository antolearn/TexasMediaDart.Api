var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

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
