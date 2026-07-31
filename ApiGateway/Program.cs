using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: false);
builder.Configuration.AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: false);
builder.Services.AddOcelot();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "https://pastebin-frontend.onrender.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("Frontend");

// TODO: endpoint debug tạm thời để chẩn đoán lỗi 502 khi gọi downstream trên Render - xóa sau khi xong.
app.MapGet("/debug/ping-auth", async () =>
{
    try
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(15) };
        var resp = await client.GetAsync("https://pastebin-authservice.onrender.com/api/Auth/me");
        var body = await resp.Content.ReadAsStringAsync();
        return Results.Ok(new { status = (int)resp.StatusCode, body });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            errorType = ex.GetType().FullName,
            message = ex.Message,
            inner = ex.InnerException?.Message
        });
    }
});

await app.UseOcelot();

app.Run();