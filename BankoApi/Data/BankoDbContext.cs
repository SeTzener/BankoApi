using BankoApi.Data.Dao;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Data;

public class BankoDbContext : DbContext
{
    public DbSet<Institution> Institutions { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<Transactions> Transactions { get; set; }

    public BankoDbContext(DbContextOptions<BankoDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            // Optional: If configuration via `AddDbContext` is not available, use fallback logic
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASS") ?? "";

            var connectionString =
                $"Server=localhost,1433;Database=BankoDb;User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}