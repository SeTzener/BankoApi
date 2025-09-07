using System.Text.Json.Serialization;
using BankoApi.Data;
using BankoApi.Data.Dao;

namespace BankoApi.Services.Model;

using TransactionDao = Transaction;
using DebtorAccountDao = Data.Dao.DebtorAccount;
using CreditorAccountDao = Data.Dao.CreditorAccount;
using PendingDao = Data.Dao.Pending;

public class Transactions
{
    [JsonPropertyName("transactions")] public required BankTransactions BankTransactions { get; set; }
}

public class BankTransactions
{
    public required List<Booked> Booked { get; set; }
    public required List<Pending> Pending { get; set; }
}

public class Booked
{
    public required string TransactionId { get; set; }
    public required string BookingDate { get; set; }
    public required string ValueDate { get; set; }
    public required TransactionAmount TransactionAmount { get; set; }
    public DebtorAccount? DebtorAccount { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
    public required string BankTransactionCode { get; set; }
    public required string InternalTransactionId { get; set; }
    public string? CreditorName { get; set; }
    public CreditorAccount? CreditorAccount { get; set; }
    public string? DebtorName { get; set; }
    public List<string>? RemittanceInformationStructuredArray { get; set; }
}

public class TransactionAmount
{
    public required string Amount { get; set; }
    public required string Currency { get; set; }
}

public class DebtorAccount
{
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}

public class CreditorAccount
{
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}

public class Pending
{
    public required string BookingDate { get; set; }
    public required TransactionAmount TransactionAmount { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
}

public static class TransactionsExtensions
{
    public static List<TransactionDao> ToTransactionDao(this Transactions transactions, BankoDbContext dbContext, Guid userId)
    {
        if (transactions.BankTransactions.Booked.Count == 0)
            // Handle the case where no booked transactions exist
            return new List<TransactionDao>();

        return transactions.BankTransactions.Booked.ConvertAll(bookedTransaction =>
            new TransactionDao
            {
                Id = bookedTransaction.TransactionId,
                UserId = userId,
                BookingDate = DateTime.Parse(bookedTransaction.BookingDate),
                ValueDate = DateTime.Parse(bookedTransaction.ValueDate),
                Amount = bookedTransaction.TransactionAmount.Amount,
                Currency = bookedTransaction.TransactionAmount.Currency,
                DebtorAccount = bookedTransaction.DebtorAccount != null
                    ? GetDebtorAccountId(bookedTransaction.DebtorAccount, dbContext)
                    : null,
                RemittanceInformationUnstructured = bookedTransaction.RemittanceInformationUnstructured,
                RemittanceInformationUnstructuredArray = bookedTransaction.RemittanceInformationUnstructuredArray,
                BankTransactionCode = bookedTransaction.BankTransactionCode,
                InternalTransactionId = bookedTransaction.InternalTransactionId,
                CreditorName = bookedTransaction.CreditorName,
                CreditorAccount = bookedTransaction.CreditorAccount != null
                    ? new CreditorAccountDao
                    {
                        Bban = bookedTransaction.CreditorAccount.Bban,
                        Iban = bookedTransaction.CreditorAccount.Iban
                    }
                    : null,
                RemittanceInformationStructuredArray = bookedTransaction.RemittanceInformationStructuredArray
            });
    }

    public static List<PendingDao> ToPendingDao(this List<Pending> pendings)
    {
        if (!pendings.Any())
            return new List<PendingDao>();

        return pendings.ConvertAll(pendingTransaction =>
            new PendingDao
            {
                BookingDate = DateTime.Parse(pendingTransaction.BookingDate),
                Amount = pendingTransaction.TransactionAmount.Amount,
                Currency = pendingTransaction.TransactionAmount.Currency,
                RemittanceInformationUnstructured = pendingTransaction.RemittanceInformationUnstructured,
                RemittanceInformationUnstructuredArray = pendingTransaction.RemittanceInformationUnstructuredArray
            }
        );
    }

    private static DebtorAccountDao? GetDebtorAccountId(this DebtorAccount debtorAccount, BankoDbContext ctx)
    {
        var existingAccount = ctx.DebtorAccounts.FirstOrDefault(it => it.Iban == debtorAccount.Iban);
        if (existingAccount != null) return existingAccount;

        var newAccount = new DebtorAccountDao
        {
            Bban = debtorAccount.Bban,
            Iban = debtorAccount.Iban
        };

        ctx.DebtorAccounts.Add(newAccount);
        ctx.SaveChanges();
        return newAccount;
    }
}