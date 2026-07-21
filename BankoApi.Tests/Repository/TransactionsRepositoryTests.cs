using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Tests.Repository;

public class TransactionsRepositoryTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private Transactions CreateSampleTransactions()
    {
        return new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    new()
                    {
                        TransactionId = "tx-1",
                        BookingDate = "2024-01-15",
                        ValueDate = "2024-01-15",
                        TransactionAmount = new TransactionAmount
                        {
                            Amount = "100.00",
                            Currency = "EUR"
                        },
                        RemittanceInformationUnstructured = "Payment 1",
                        RemittanceInformationUnstructuredArray = new List<string> { "Payment 1" },
                        InternalTransactionId = "internal-1",
                        BankTransactionCode = "PMNT",
                        CreditorName = "Creditor A",
                        DebtorName = "Debtor A"
                    },
                    new()
                    {
                        TransactionId = "tx-2",
                        BookingDate = "2024-01-16",
                        ValueDate = "2024-01-16",
                        TransactionAmount = new TransactionAmount
                        {
                            Amount = "200.00",
                            Currency = "EUR"
                        },
                        RemittanceInformationUnstructured = "Payment 2",
                        RemittanceInformationUnstructuredArray = new List<string> { "Payment 2" },
                        InternalTransactionId = "internal-2",
                        BankTransactionCode = "PMNT"
                    }
                }
            }
        };
    }

    [Fact]
    public async Task StoreTransactions_NewTransactions_StoresAll()
    {
        using var ctx = CreateContext();
        var repo = new TransactionsRepository();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var transactions = CreateSampleTransactions();

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Equal(2, ctx.Transactions.Count());
    }

    [Fact]
    public async Task StoreTransactions_DuplicateInternalTransactionId_SkipsDuplicates()
    {
        using var ctx = CreateContext();
        ctx.Transactions.Add(new Transaction
        {
            Id = "existing-tx",
            UserId = Guid.NewGuid(),
            BankAccountId = Guid.NewGuid(),
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "50.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Existing",
            RemittanceInformationUnstructuredArray = new List<string> { "Existing" },
            InternalTransactionId = "internal-1"
        });
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var transactions = CreateSampleTransactions();

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        // Only tx-2 should be added (tx-1 has duplicate InternalTransactionId)
        Assert.Equal(2, ctx.Transactions.Count());
    }

    [Fact]
    public async Task StoreTransactions_EmptyTransactionId_GeneratesNewGuid()
    {
        using var ctx = CreateContext();
        var repo = new TransactionsRepository();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    new()
                    {
                        TransactionId = null,
                        BookingDate = "2024-01-15",
                        ValueDate = "2024-01-15",
                        TransactionAmount = new TransactionAmount
                        {
                            Amount = "100.00",
                            Currency = "EUR"
                        },
                        RemittanceInformationUnstructured = "No ID",
                        RemittanceInformationUnstructuredArray = new List<string> { "No ID" },
                        InternalTransactionId = "internal-new"
                    }
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var stored = ctx.Transactions.First();
        Assert.NotNull(stored.Id);
        Assert.NotEqual("", stored.Id);
    }

    [Fact]
    public void SetEuaExpirationStatus_ValidAgreementId_UpdatesStatus()
    {
        using var ctx = CreateContext();
        var agreementId = Guid.NewGuid().ToString();
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            AgreementId = agreementId,
            Status = BankAuthorizationStaus.Linked,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        ctx.SaveChanges();

        var repo = new TransactionsRepository();
        repo.SetEuaExpirationStatus(ctx, $"EUA was valid for 90 days and it expired {agreementId}");

        var auth = ctx.BankAuthorizations.First();
        Assert.Equal(BankAuthorizationStaus.Expired, auth.Status);
    }

    [Fact]
    public void SetEuaExpirationStatus_MessageWithoutGuid_ThrowsEndUserAgreementException()
    {
        using var ctx = CreateContext();
        var repo = new TransactionsRepository();

        Assert.Throws<BankoApi.Exceptions.GoCardless.Transactions.EndUserAgreementException>(
            () => repo.SetEuaExpirationStatus(ctx, "No GUID in this message"));
    }
}
