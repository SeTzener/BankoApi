using BankoApi.Data.Dao;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Data;

public class BankoDbContext : DbContext
{
    public BankoDbContext(DbContextOptions<BankoDbContext> options) : base(options)
    {
    }

    public DbSet<Institutions> Institutions { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<DebtorAccount> DebtorAccounts { get; set; }
    public DbSet<Pending> Pendings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            Env.Load();
            // Optional: If configuration via `AddDbContext` is not available, use fallback logic
            var dbUser = Environment.GetEnvironmentVariable("DB_USER") ?? "";
            var dbPassword = Environment.GetEnvironmentVariable("DB_PASS") ?? "";

            var connectionString =
                $"Server=localhost,1433;Database=BankoDb;User Id={dbUser};Password={dbPassword};TrustServerCertificate=True;";
            optionsBuilder.UseSqlServer(connectionString);
        }
    }
}