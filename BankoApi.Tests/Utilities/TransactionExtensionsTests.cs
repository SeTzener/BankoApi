using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;
using PendingDto = BankoApi.Services.Model.Pending;
using ServiceCreditorAccount = BankoApi.Services.Model.CreditorAccount;
using ServiceDebtorAccount = BankoApi.Services.Model.DebtorAccount;

namespace BankoApi.Tests.Utilities;

public class TransactionExtensionsTests
{
    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    [Fact]
    public void ToTransactionDao_BookedTransactions_ReturnsDaoList()
    {
        using var ctx = CreateContext();
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
                        TransactionId = "tx-1",
                        BookingDate = "2024-06-15",
                        ValueDate = "2024-06-15",
                        TransactionAmount = new TransactionAmount
                        {
                            Amount = "250.00",
                            Currency = "EUR"
                        },
                        RemittanceInformationUnstructured = "Salary",
                        RemittanceInformationUnstructuredArray = new List<string> { "Salary" },
                        InternalTransactionId = "int-1",
                        BankTransactionCode = "PMNT",
                        CreditorName = "Employer",
                        CreditorAccount = new ServiceCreditorAccount
                        {
                            Iban = "DE89370400440532013000",
                            Bban = "00000000000"
                        }
                    }
                },
                Pending = new List<PendingDto>()
            }
        };

        var result = transactions.ToTransactionDao(ctx, userId, bankAccountId);

        Assert.Single(result);
        var dao = result[0];
        Assert.Equal("tx-1", dao.Id);
        Assert.Equal(userId, dao.UserId);
        Assert.Equal(bankAccountId, dao.BankAccountId);
        Assert.Equal("250.00", dao.Amount);
        Assert.Equal("EUR", dao.Currency);
        Assert.Equal("Salary", dao.RemittanceInformationUnstructured);
        Assert.Equal("Employer", dao.CreditorName);
        Assert.NotNull(dao.CreditorAccount);
        Assert.Equal("DE89370400440532013000", dao.CreditorAccount.Iban);
    }

    [Fact]
    public void ToTransactionDao_EmptyBooked_ReturnsEmptyList()
    {
        using var ctx = CreateContext();
        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>(),
                Pending = new List<PendingDto>()
            }
        };

        var result = transactions.ToTransactionDao(ctx, Guid.NewGuid(), Guid.NewGuid());

        Assert.Empty(result);
    }

    [Fact]
    public void ToTransactionDao_DebtorAccount_FindsOrCreatesDebtorAccount()
    {
        using var ctx = CreateContext();
        var existingIban = "EXISTINGIBAN123";
        ctx.DebtorAccounts.Add(new BankoApi.Data.Dao.DebtorAccount
        {
            Id = Guid.NewGuid(),
            Iban = existingIban,
            Bban = "existing-bban"
        });
        ctx.SaveChanges();

        var transactions = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>
                {
                    new()
                    {
                        TransactionId = "tx-debtor",
                        BookingDate = "2024-06-15",
                        ValueDate = "2024-06-15",
                        TransactionAmount = new TransactionAmount
                        {
                            Amount = "100.00",
                            Currency = "EUR"
                        },
                        RemittanceInformationUnstructured = "Test",
                        RemittanceInformationUnstructuredArray = new List<string> { "Test" },
                        InternalTransactionId = "int-debtor",
                        DebtorAccount = new ServiceDebtorAccount
                        {
                            Iban = existingIban,
                            Bban = "existing-bban"
                        }
                    }
                },
                Pending = new List<PendingDto>()
            }
        };

        var result = transactions.ToTransactionDao(ctx, Guid.NewGuid(), Guid.NewGuid());

        Assert.Single(result);
        Assert.NotNull(result[0].DebtorAccount);
        Assert.Equal(existingIban, result[0].DebtorAccount.Iban);
    }

    [Fact]
    public void ToPendingDao_PendingTransactions_ReturnsDaoList()
    {
        var pendings = new List<PendingDto>
        {
            new()
            {
                BookingDate = "2024-06-20",
                TransactionAmount = new TransactionAmount
                {
                    Amount = "75.00",
                    Currency = "EUR"
                },
                RemittanceInformationUnstructured = "Pending tx",
                RemittanceInformationUnstructuredArray = new List<string> { "Pending tx" }
            }
        };

        var result = pendings.ToPendingDao();

        Assert.Single(result);
        Assert.Equal("75.00", result[0].Amount);
        Assert.Equal("EUR", result[0].Currency);
    }

    [Fact]
    public void ToPendingDao_EmptyList_ReturnsEmptyList()
    {
        var pendings = new List<PendingDto>();

        var result = pendings.ToPendingDao();

        Assert.Empty(result);
    }

    [Fact]
    public void ToPendingDao_NullableFields_NullCoalescingApplied()
    {
        var pendings = new List<PendingDto>
        {
            new()
            {
                BookingDate = "2024-06-20",
                TransactionAmount = new TransactionAmount
                {
                    Amount = "50.00",
                    Currency = "USD"
                },
                RemittanceInformationUnstructured = null,
                RemittanceInformationUnstructuredArray = null
            }
        };

        var result = pendings.ToPendingDao();

        Assert.Single(result);
        Assert.Equal("Transaction description not available", result[0].RemittanceInformationUnstructured);
        Assert.Single(result[0].RemittanceInformationUnstructuredArray);
    }
}
