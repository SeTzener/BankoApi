using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services.Model;
using BankoApi.Tests.Utilities;
using Microsoft.EntityFrameworkCore;
using CreditorAccountDao = BankoApi.Data.Dao.CreditorAccount;
using DebtorAccountDao = BankoApi.Data.Dao.DebtorAccount;
using ServiceCreditorAccount = BankoApi.Services.Model.CreditorAccount;
using ServiceDebtorAccount = BankoApi.Services.Model.DebtorAccount;

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
        Assert.All(ctx.Transactions, t => Assert.Equal(userId, t.UserId));
        Assert.DoesNotContain(ctx.Users, u => u.Email == "default@example.com");
    }

    [Fact]
    public async Task StoreTransactions_DuplicateInternalTransactionId_SkipsDuplicates()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        ctx.Transactions.Add(new Transaction
        {
            Id = "existing-tx",
            UserId = userId,
            BankAccountId = bankAccountId,
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
        var transactions = CreateSampleTransactions();

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        // Only tx-2 should be added (tx-1 has duplicate InternalTransactionId)
        Assert.Equal(2, ctx.Transactions.Count());
        Assert.All(ctx.Transactions, t => Assert.Equal(userId, t.UserId));
    }

    [Fact]
    public async Task StoreTransactions_DuplicateInternalTransactionId_UpdatesRowAndPreservesUserAnnotations()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        const string expenseTagId = "tag-1";

        ctx.Transactions.Add(new Transaction
        {
            Id = "existing-tx",
            UserId = userId,
            BankAccountId = bankAccountId,
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "50.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Existing",
            RemittanceInformationUnstructuredArray = new List<string> { "Existing" },
            InternalTransactionId = "internal-1",
            ExpenseTagId = expenseTagId,
            Note = "User note"
        });
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = CreateSampleTransactions();

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var updated = ctx.Transactions.First(t => t.InternalTransactionId == "internal-1");
        Assert.Equal("100.00", updated.Amount);
        Assert.Equal(expenseTagId, updated.ExpenseTagId);
        Assert.Equal("User note", updated.Note);
        Assert.False(updated.isDeleted);
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

    private static Booked CreateBooked(
        string? transactionId,
        string internalTransactionId,
        string amount = "100.00",
        string? creditorIban = null,
        string? creditorBban = null,
        string? debtorIban = null,
        string? debtorBban = null)
    {
        return new Booked
        {
            TransactionId = transactionId,
            BookingDate = "2024-01-15",
            ValueDate = "2024-01-15",
            TransactionAmount = new TransactionAmount
            {
                Amount = amount,
                Currency = "EUR"
            },
            RemittanceInformationUnstructured = "Payment",
            RemittanceInformationUnstructuredArray = new List<string> { "Payment" },
            InternalTransactionId = internalTransactionId,
            BankTransactionCode = "PMNT",
            CreditorAccount = creditorIban != null
                ? new ServiceCreditorAccount { Iban = creditorIban, Bban = creditorBban ?? "default-bban" }
                : null,
            DebtorAccount = debtorIban != null
                ? new ServiceDebtorAccount { Iban = debtorIban, Bban = debtorBban ?? "default-bban" }
                : null
        };
    }

    private static Transaction CreateSeedTransaction(
        Guid userId,
        Guid bankAccountId,
        string id,
        string internalTransactionId,
        DateTime bookingDate,
        CreditorAccountDao? creditorAccount = null,
        DebtorAccountDao? debtorAccount = null)
    {
        return new Transaction
        {
            Id = id,
            UserId = userId,
            BankAccountId = bankAccountId,
            BookingDate = bookingDate,
            ValueDate = bookingDate,
            Amount = "50.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Existing",
            RemittanceInformationUnstructuredArray = new List<string> { "Existing" },
            InternalTransactionId = internalTransactionId,
            CreditorAccount = creditorAccount,
            DebtorAccount = debtorAccount,
            isDeleted = false
        };
    }

    [Fact]
    public async Task StoreTransactions_NewCreditorAccount_CreatesNewRow()
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
                    CreateBooked(
                        transactionId: "tx-1",
                        internalTransactionId: "internal-1",
                        creditorIban: "NO9386011117947",
                        creditorBban: "93860111179")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Single(ctx.CreditorAccounts);
        var stored = ctx.Transactions.Single();
        AccountAssertions.AssertEqual("NO9386011117947", "93860111179", stored.CreditorAccount);
    }

    [Fact]
    public async Task StoreTransactions_ExistingCreditorAccount_DiscardsChangeAndReusesRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var existingCreditor = new CreditorAccountDao
        {
            Iban = "NO9386011117947",
            Bban = "original-bban"
        };
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-1", "internal-1",
            DateTime.Parse("2024-01-15"), creditorAccount: existingCreditor));
        await ctx.SaveChangesAsync();
        var seededCreditorId = existingCreditor.Id;

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(
                        transactionId: "tx-1",
                        internalTransactionId: "internal-1",
                        creditorIban: "NO9386011117947",
                        creditorBban: "changed-bban")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var storedCreditor = ctx.CreditorAccounts.Single();
        Assert.Equal(seededCreditorId, storedCreditor.Id);
        Assert.Equal("original-bban", storedCreditor.Bban);
        var stored = ctx.Transactions.Single();
        Assert.Equal(seededCreditorId, stored.CreditorAccount!.Id);
    }

    [Fact]
    public async Task StoreTransactions_ExistingTransactionWithNewCreditorIban_CreatesNewRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-1", "internal-1",
            DateTime.Parse("2024-01-15"),
            creditorAccount: new CreditorAccountDao { Iban = "NO9386011117947", Bban = "original-bban" }));
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(
                        transactionId: "tx-1",
                        internalTransactionId: "internal-1",
                        creditorIban: "NO1234567890123",
                        creditorBban: "12345678901")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Equal(2, ctx.CreditorAccounts.Count());
        var stored = ctx.Transactions.Single();
        AccountAssertions.AssertEqual("NO1234567890123", "12345678901", stored.CreditorAccount);
    }

    [Fact]
    public async Task StoreTransactions_NewDebtorAccount_CreatesNewRow()
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
                    CreateBooked(
                        transactionId: "tx-1",
                        internalTransactionId: "internal-1",
                        debtorIban: "DE89370400440532013000",
                        debtorBban: "370400440532013000")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Single(ctx.DebtorAccounts);
        var stored = ctx.Transactions.Single();
        AccountAssertions.AssertEqual("DE89370400440532013000", "370400440532013000", stored.DebtorAccount);
    }

    [Fact]
    public async Task StoreTransactions_ExistingDebtorAccount_DiscardsChangeAndReusesRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        var existingDebtor = new DebtorAccountDao
        {
            Iban = "DE89370400440532013000",
            Bban = "original-bban"
        };
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-1", "internal-1",
            DateTime.Parse("2024-01-15"), debtorAccount: existingDebtor));
        await ctx.SaveChangesAsync();
        var seededDebtorId = existingDebtor.Id;

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(
                        transactionId: "tx-1",
                        internalTransactionId: "internal-1",
                        debtorIban: "DE89370400440532013000",
                        debtorBban: "changed-bban")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var storedDebtor = ctx.DebtorAccounts.Single();
        Assert.Equal(seededDebtorId, storedDebtor.Id);
        Assert.Equal("original-bban", storedDebtor.Bban);
        var stored = ctx.Transactions.Single();
        Assert.Equal(seededDebtorId, stored.DebtorAccount!.Id);
    }

    [Fact]
    public async Task StoreTransactions_OnlyTransactionIdWithoutInternalId_StoresRow()
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
                    CreateBooked(transactionId: "tx-9", internalTransactionId: "")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var stored = Assert.Single(ctx.Transactions);
        Assert.Equal("tx-9", stored.Id);
    }

    [Fact]
    public async Task StoreTransactions_MissingTransactionIdAndInternalId_DiscardsTransaction()
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
                    CreateBooked(transactionId: null, internalTransactionId: "")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Empty(ctx.Transactions);
    }

    [Fact]
    public async Task StoreTransactions_ExistingTransactionMatchedByTransactionId_UpdatesRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-9", "",
            DateTime.Parse("2024-01-15")));
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(transactionId: "tx-9", internalTransactionId: "", amount: "200.00")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var stored = Assert.Single(ctx.Transactions);
        Assert.Equal("200.00", stored.Amount);
    }

    [Fact]
    public async Task StoreTransactions_ExistingTransactionWithEmptyInternalIdAndIncomingInternalId_UpdatesRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-1", "",
            DateTime.Parse("2024-01-15")));
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(transactionId: "tx-1", internalTransactionId: "internal-1", amount: "200.00")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var stored = Assert.Single(ctx.Transactions);
        Assert.Equal("200.00", stored.Amount);
        Assert.Equal("internal-1", stored.InternalTransactionId);
    }

    [Fact]
    public async Task StoreTransactions_ExistingTransactionOutsideBookingDateWindow_UpdatesRow()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var bankAccountId = Guid.NewGuid();
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, bankAccountId, "tx-old", "",
            DateTime.Parse("2020-01-01")));
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(transactionId: "tx-old", internalTransactionId: "internal-new", amount: "200.00")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        var stored = Assert.Single(ctx.Transactions);
        Assert.Equal("200.00", stored.Amount);
        Assert.Equal("internal-new", stored.InternalTransactionId);
    }

    [Fact]
    public async Task StoreTransactions_ExistingTransactionIdInAnotherAccount_SkipsInsert()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var accountA = Guid.NewGuid();
        var accountB = Guid.NewGuid();
        ctx.Transactions.Add(CreateSeedTransaction(
            userId, accountA, "tx-1", "internal-1",
            DateTime.Parse("2024-01-15")));
        await ctx.SaveChangesAsync();

        var repo = new TransactionsRepository();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    CreateBooked(transactionId: "tx-1", internalTransactionId: "internal-2", amount: "200.00")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, accountB, transactions);
        await ctx.SaveChangesAsync();

        var stored = Assert.Single(ctx.Transactions);
        Assert.Equal(accountA, stored.BankAccountId);
        Assert.Equal("50.00", stored.Amount);
    }

    [Fact]
    public async Task StoreTransactions_DuplicateTransactionIdWithinBatch_DifferentInternalIds_StoresSingleRow()
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
                    CreateBooked(transactionId: "tx-1", internalTransactionId: "internal-a"),
                    CreateBooked(transactionId: "tx-1", internalTransactionId: "internal-b")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Single(ctx.Transactions);
    }

    [Fact]
    public async Task StoreTransactions_DuplicateInternalTransactionIdWithinBatch_StoresSingleRow()
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
                    CreateBooked(transactionId: "tx-1", internalTransactionId: "duplicate-1"),
                    CreateBooked(transactionId: "tx-2", internalTransactionId: "duplicate-1")
                }
            }
        };

        await repo.StoreTransactions(ctx, userId, bankAccountId, transactions);
        await ctx.SaveChangesAsync();

        Assert.Single(ctx.Transactions);
    }
}
