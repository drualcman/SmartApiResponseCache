using SmartApiResponseCache.Middleware.Extensions;
using SmartApiResponseCache.Middleware.Options;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi2_0;
});
builder.Services.AddSmartResponseMemoryCache(
    options => builder.Configuration.GetSection(SmartCacheOptions.SectionKey).Bind(options)
    );
//Or also can do
//builder.Services.AddSmartResponseMemoryCache();


var app = builder.Build();

// Configure the HTTP request pipeline.
if(app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/openapi/v1.json", "SmartApiResponseCache API demo v1.0");
    });
}

app.UseSmartApiResponseCache();
app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async () =>
{
    await Task.Delay(2000);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithTags("WithSmartCacheDefaultsValues");

app.MapPost("/weatherforecast", async () =>
{
    await Task.Delay(2000);
    return Results.NoContent();
})
.WithTags("WithSmartCacheNoContentResult");

app.MapPut("/weatherforecast", async () =>
{
    await Task.Delay(2000);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithTags("WithSmartCacheChangeSecondsTo10")
.WithSmartCacheSeconds(10);

app.MapPatch("/weatherforecast", async () =>
{
    await Task.Delay(2000);
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithTags("WithoutSmartCache")
.WithoutSmartCache();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
