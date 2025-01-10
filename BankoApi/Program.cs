using BankoApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration setup (assuming IConfiguration is injected)
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddHttpClient<NordigenTokenService>(client =>
{
    client.BaseAddress =
        new Uri(builder.Configuration["NordigenAPI:BaseUrl"] + builder.Configuration["NordigenAPI:version"]);
});
builder.Services.AddHttpClient<NordigenService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["NordigenAPI:BaseUrl"] + builder.Configuration["NordigenAPI:version"]);
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