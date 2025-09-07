using BankoApi.Data.Dao;
using DotNetEnv;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Data;

public class BankoDbContext : DbContext
{
    public BankoDbContext(DbContextOptions<BankoDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Balance> Balances { get; set; }
    public DbSet<Requisition> Requisitions { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ExpenseTag> ExpenseTag { get; set; }
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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey("UserId")
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.DebtorAccount) // Explicitly set the relationship
            .WithMany() // Assuming a one-to-many relationship
            .HasForeignKey("DebtorAccountId") // Define the FK explicitly
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascading deletes

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.CreditorAccount) // Explicitly set the relationship
            .WithMany() // Assuming a one-to-many relationship
            .HasForeignKey("CreditorAccountId") // Define the FK explicitly
            .OnDelete(DeleteBehavior.Restrict); // Avoid cascading deletes

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.ExpenseTag) // Explicitly set the relationship
            .WithMany()
            .HasForeignKey(t => t.ExpenseTagId) // Define the FK explicitly
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull); // Avoid cascading deletes

        modelBuilder.Entity<ExpenseTag>()
            .HasKey(et => et.Id);

        modelBuilder.Entity<ExpenseTag>()
            .HasIndex(et => et.Id)
            .IsUnique();

        modelBuilder.Entity<DebtorAccount>()
            .HasIndex(da => da.Iban) // Add a unique constraint for IBAN
            .IsUnique(); // Ensure uniqueness in the database

        modelBuilder.Entity<CreditorAccount>()
            .HasIndex(da => da.Iban); // Add a unique constraint for IBAN

        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
    }
}