using BankoApi.Data;
using BankoApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using BankoApi;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup (assuming IConfiguration is injected)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddDbContext<BankoDbContext>(options =>
{
    Env.Load();
    String db = Utils.SelectDatabase(builder);
    String baseUrl = Environment.GetEnvironmentVariable("GOOGLE_CLOUD_IP") ?? throw new Exception("GoogleCloud BaseUrl is missing");
    String dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
    String dbPassword = Environment.GetEnvironmentVariable("DB_PASS") ?? "";
    String connectionString =
        $"Server={baseUrl},1433;Database={db};User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
    options.UseSqlServer(connectionString);
});

String baseUrl = builder.Configuration["NordigenAPI:BaseUrl"] ?? throw new Exception("GoCadless Base URL is missing");
String version = builder.Configuration["NordigenAPI:version"] ?? throw new Exception("GoCadless API version is missing");
    
builder.Services.AddHttpClient<NordigenTokenService>(client =>
{
    client.BaseAddress = new Uri(new Uri(baseUrl), version); ;
});
builder.Services.AddHttpClient<NordigenService>(client =>
{
    client.BaseAddress = new Uri(new Uri(baseUrl),  version);
    client.DefaultRequestHeaders.Accept.Add(
     new MediaTypeWithQualityHeaderValue("application/json"));
    client.DefaultRequestHeaders.UserAgent.ParseAdd("Banko/1.0");
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();