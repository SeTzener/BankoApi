using BankoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

builder.Services.AddHttpClient<NordigenTokenService>(client =>
{
    client.BaseAddress = 
        new Uri(builder.Configuration["NordigenAPI:BaseUrl"] + builder.Configuration["NordigenAPI:version"]);
});

builder.Services.AddHttpClient<NordigenService>(client =>
{
    client.BaseAddress =
        new Uri(builder.Configuration["NordigenAPI:BaseUrl"] + builder.Configuration["NordigenAPI:version"]);
});

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();