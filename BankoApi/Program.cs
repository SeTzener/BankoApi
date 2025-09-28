using System.Net.Http.Headers;
using BankoApi;
using BankoApi.Controllers.GoCardless;
using BankoApi.Data;
using BankoApi.Services;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup (assuming IConfiguration is injected)
builder.Configuration.AddJsonFile("appsettings.json", false, true);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<BankoDbContext>(options =>
{
    Env.Load();
    var db = Utils.SelectDatabase(builder);
    var baseUrl = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_IP")
                  ?? throw new Exception("GoogleCloud BaseUrl is missing");
    var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
    var dbPassword = Environment.GetEnvironmentVariable("DB_PASS") ?? "";
    var connectionString =
        $"Server={baseUrl},1433;Database={db};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
    options.UseSqlServer(connectionString);
});

var baseUrl = builder.Configuration["GoCardlessAPI:BaseUrl"] ?? throw new Exception("GoCadless Base URL is missing");
var version = builder.Configuration["GoCardlessAPI:version"] ?? throw new Exception("GoCadless API version is missing");

builder.Services.AddHttpClient<GoCardlessTokenService>(client =>
{
    client.BaseAddress = new Uri(new Uri(baseUrl), version);
});
builder.Services.AddHttpClient<GoCardlessService>(client =>
{
    client.BaseAddress = new Uri(new Uri(baseUrl), version);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Banko/1.0");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the background service
builder.Services.AddHostedService<ScheduledTaskService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();