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
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<ExpenseTag> ExpenseTag { get; set; }
    public DbSet<DebtorAccount> DebtorAccounts { get; set; }
    public DbSet<BankAccount> BankAccounts { get; set; }
    public DbSet<BankAuthorization> BankAuthorizations { get; set; }
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

        modelBuilder.Entity<BankAuthorization>(entity =>
        {
            // Set unique constraint on RequisitionId
            entity.HasIndex(e => e.RequisitionId)
                  .IsUnique();

            // Create an index on Status for querying connections that need action
            entity.HasIndex(e => e.Status)
                  .HasDatabaseName("idx_bank_auth_status");

            // Create an index on UserId for quickly finding a user's connections
            entity.HasIndex(e => e.UserId)
                  .HasDatabaseName("idx_bank_auth_user_id");

            // Define the relationship with User
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.Status)
            .HasConversion<string>()  // This converts enum to string
            .HasMaxLength(50);
        });

        // Configure BankAccount Entity
        modelBuilder.Entity<BankAccount>(entity =>
        {
            // Create a unique constraint on the combination of (ConnectionId, AccountId)
            entity.HasIndex(e => new { e.BankAuthorizationId, e.BankAccountId })
                  .IsUnique();

            // Create an index on ConnectionId for quickly fetching accounts for a connection
            entity.HasIndex(e => e.BankAuthorizationId)
                  .HasDatabaseName("idx_bank_accounts_connection_id");

            // Define the relationship with GoCardlessConnection
            entity.HasOne(e => e.BankAuthorization)
                  .WithMany(c => c.BankAccounts)
                  .HasForeignKey(e => e.BankAuthorizationId)
                  .OnDelete(DeleteBehavior.Cascade); // ON DELETE CASCADE
        });
    }
}