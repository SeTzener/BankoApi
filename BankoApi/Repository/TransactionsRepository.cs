using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace BankoApi.Repository;

using CreditorAccountDao = BankoApi.Data.Dao.CreditorAccount;
using DebtorAccountDao = BankoApi.Data.Dao.DebtorAccount;

public class TransactionsRepository
{
    private const int SaveBatchSize = 200;

    private readonly ILogger<TransactionsRepository>? _logger;

    public TransactionsRepository(ILogger<TransactionsRepository>? logger = null)
    {
        _logger = logger;
    }

    public async Task StoreTransactions(BankoDbContext ctx, Guid userId, Guid bankAccountId, Transactions transactions)
    {
        var booked = transactions.BankTransactions.Booked;
        if (booked.Count == 0) return;

        var oldestBookingDate = booked.Min(t => DateTime.Parse(t.BookingDate));

        var existingTransactions = await ctx.Transactions
            .Where(t => t.UserId == userId
                && t.BankAccountId == bankAccountId
                && t.BookingDate >= oldestBookingDate.AddDays(-1))
            .ToListAsync();

        var existingById = new Dictionary<string, Transaction>();
        var existingByInternalId = new Dictionary<string, Transaction>();
        foreach (var transaction in existingTransactions)
        {
            existingById.TryAdd(transaction.Id, transaction);
            if (!string.IsNullOrEmpty(transaction.InternalTransactionId))
            {
                existingByInternalId.TryAdd(transaction.InternalTransactionId, transaction);
            }
        }

        var creditorByIban = new Dictionary<string, CreditorAccountDao>();
        var debtorByIban = new Dictionary<string, DebtorAccountDao>();
        var seenTransactionIds = new HashSet<string>();
        var seenInternalIds = new HashSet<string>();
        var plannedInserts = new List<(Booked Booked, string Id)>();
        var pendingTransactions = new List<Transaction>();

        foreach (var newTransaction in booked)
        {
            string? id = newTransaction.TransactionId;
            if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(newTransaction.InternalTransactionId))
                id = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(newTransaction.InternalTransactionId))
                continue;

            if (!seenTransactionIds.Add(id!)) continue;
            if (!string.IsNullOrEmpty(newTransaction.InternalTransactionId)
                && !seenInternalIds.Add(newTransaction.InternalTransactionId))
                continue;

            var existingTransaction = FindExistingTransaction(existingById, existingByInternalId, id!, newTransaction.InternalTransactionId);
            if (existingTransaction != null)
            {
                BackfillInternalTransactionId(existingTransaction, newTransaction);
                UpdateTransactionData(ctx, existingTransaction, newTransaction, creditorByIban, debtorByIban);
            }
            else
            {
                plannedInserts.Add((newTransaction, id!));
            }
        }

        await ResolvePlannedInserts(ctx, plannedInserts, pendingTransactions, userId, bankAccountId, creditorByIban, debtorByIban);
        await SaveInBatches(ctx, pendingTransactions);
    }

    private static Transaction? FindExistingTransaction(
        Dictionary<string, Transaction> existingById,
        Dictionary<string, Transaction> existingByInternalId,
        string id,
        string internalTransactionId)
    {
        if (!string.IsNullOrEmpty(internalTransactionId)
            && existingByInternalId.TryGetValue(internalTransactionId, out var byInternalId))
        {
            return byInternalId;
        }

        return existingById.TryGetValue(id, out var byId) ? byId : null;
    }

    private static void BackfillInternalTransactionId(Transaction existingTransaction, Booked newTransaction)
    {
        if (string.IsNullOrEmpty(existingTransaction.InternalTransactionId)
            && !string.IsNullOrEmpty(newTransaction.InternalTransactionId))
        {
            existingTransaction.InternalTransactionId = newTransaction.InternalTransactionId;
        }
    }

    private async Task ResolvePlannedInserts(
        BankoDbContext ctx,
        List<(Booked Booked, string Id)> plannedInserts,
        List<Transaction> pendingTransactions,
        Guid userId,
        Guid bankAccountId,
        Dictionary<string, CreditorAccountDao> creditorByIban,
        Dictionary<string, DebtorAccountDao> debtorByIban)
    {
        if (plannedInserts.Count == 0) return;

        var insertIds = plannedInserts.Select(p => p.Id).ToList();
        var existingById = await ctx.Transactions
            .Where(t => insertIds.Contains(t.Id))
            .ToDictionaryAsync(t => t.Id);

        foreach (var planned in plannedInserts.ToList())
        {
            if (!existingById.TryGetValue(planned.Id, out var existingTransaction))
                continue;

            plannedInserts.Remove(planned);
            if (existingTransaction.UserId == userId && existingTransaction.BankAccountId == bankAccountId)
            {
                _logger?.LogWarning(
                    "Transaction {Id} already exists but was outside the booking-date lookup window; updating instead of inserting",
                    planned.Id);
                BackfillInternalTransactionId(existingTransaction, planned.Booked);
                UpdateTransactionData(ctx, existingTransaction, planned.Booked, creditorByIban, debtorByIban);
            }
            else
            {
                _logger?.LogWarning(
                    "Transaction {Id} already exists under a different bank account; skipping insert to avoid a duplicate primary key",
                    planned.Id);
            }
        }

        foreach (var planned in plannedInserts)
        {
            pendingTransactions.Add(
                CreateNewTransaction(ctx, userId, planned.Booked, bankAccountId, planned.Id, creditorByIban, debtorByIban));
        }
    }

    private async Task SaveInBatches(BankoDbContext ctx, List<Transaction> pendingTransactions)
    {
        for (var i = 0; i < pendingTransactions.Count; i += SaveBatchSize)
        {
            var batch = pendingTransactions.Skip(i).Take(SaveBatchSize);
            ctx.Transactions.AddRange(batch);
            await ctx.SaveChangesAsync();
        }
    }

    public void SetEuaExpirationStatus(BankoDbContext dbContext, String message)
    {
        String agreementId = FindAgreementId(message);
        var result = dbContext.BankAuthorizations.FirstOrDefault(r => r.AgreementId == agreementId);
        if (result == null) return;
        result.Status = Data.Dao.BankAuthorizationStaus.Expired;
        dbContext.SaveChanges();        
    }

    private string FindAgreementId(string input)
    {
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        Match match = Regex.Match(input, pattern);

        return match.Success ? match.Value : throw new EndUserAgreementException(FetchAndStoreTransactionResponse.AgreementIdNotFound.ToString());
    }

    private void UpdateTransactionData(
        BankoDbContext ctx,
        Transaction existingTransaction,
        Booked newTransaction,
        Dictionary<string, CreditorAccountDao> creditorByIban,
        Dictionary<string, DebtorAccountDao> debtorByIban)
    {
        existingTransaction.BookingDate = DateTime.Parse(newTransaction.BookingDate);
        existingTransaction.ValueDate = DateTime.Parse(newTransaction.ValueDate);
        existingTransaction.Amount = newTransaction.TransactionAmount.Amount;
        existingTransaction.Currency = newTransaction.TransactionAmount.Currency;
        existingTransaction.RemittanceInformationUnstructured = newTransaction.RemittanceInformationUnstructured ?? "Transaction description not available";
        existingTransaction.RemittanceInformationUnstructuredArray = newTransaction.RemittanceInformationUnstructuredArray ?? new List<string> { "Transaction description not available" };
        existingTransaction.BankTransactionCode = newTransaction.BankTransactionCode;
        existingTransaction.CreditorName = newTransaction.CreditorName;
        existingTransaction.DebtorName = newTransaction.DebtorName;
        existingTransaction.RemittanceInformationStructuredArray = newTransaction.RemittanceInformationStructuredArray;

        if (newTransaction.DebtorAccount != null)
        {
            existingTransaction.DebtorAccount = GetOrCreateDebtorAccount(ctx, newTransaction.DebtorAccount, debtorByIban);
        }

        if (newTransaction.CreditorAccount != null)
        {
            existingTransaction.CreditorAccount = GetOrCreateCreditorAccount(ctx, newTransaction.CreditorAccount, creditorByIban);
        }
    }

    private Transaction CreateNewTransaction(
        BankoDbContext ctx,
        Guid userId,
        Booked newTransaction,
        Guid bankAccountId,
        string id,
        Dictionary<string, CreditorAccountDao> creditorByIban,
        Dictionary<string, DebtorAccountDao> debtorByIban)
    {
        var transaction = new Transaction
        {
            Id = id,
            UserId = userId,
            BankAccountId = bankAccountId,
            BookingDate = DateTime.Parse(newTransaction.BookingDate),
            ValueDate = DateTime.Parse(newTransaction.ValueDate),
            Amount = newTransaction.TransactionAmount.Amount,
            Currency = newTransaction.TransactionAmount.Currency,
            RemittanceInformationUnstructured = newTransaction.RemittanceInformationUnstructured ?? "Transaction description not available",
            RemittanceInformationUnstructuredArray = newTransaction.RemittanceInformationUnstructuredArray ?? new List<string> { "Transaction description not available" },
            BankTransactionCode = newTransaction.BankTransactionCode,
            InternalTransactionId = newTransaction.InternalTransactionId,
            CreditorName = newTransaction.CreditorName,
            DebtorName = newTransaction.DebtorName,
            RemittanceInformationStructuredArray = newTransaction.RemittanceInformationStructuredArray,
            ExpenseTagId = null,
            Note = null,
            isDeleted = false
        };

        if (newTransaction.DebtorAccount != null)
        {
            transaction.DebtorAccount = GetOrCreateDebtorAccount(ctx, newTransaction.DebtorAccount, debtorByIban);
        }

        if (newTransaction.CreditorAccount != null)
        {
            transaction.CreditorAccount = GetOrCreateCreditorAccount(ctx, newTransaction.CreditorAccount, creditorByIban);
        }

        return transaction;
    }

    private BankoApi.Data.Dao.DebtorAccount GetOrCreateDebtorAccount(
        BankoDbContext ctx,
        BankoApi.Services.Model.DebtorAccount debtorAccount,
        Dictionary<string, BankoApi.Data.Dao.DebtorAccount> debtorByIban)
    {
        if (debtorByIban.TryGetValue(debtorAccount.Iban, out var cached)) return cached;

        var existingAccount = ctx.DebtorAccounts.FirstOrDefault(it => it.Iban == debtorAccount.Iban);
        if (existingAccount == null)
        {
            existingAccount = new BankoApi.Data.Dao.DebtorAccount
            {
                Bban = debtorAccount.Bban,
                Iban = debtorAccount.Iban
            };
            ctx.DebtorAccounts.Add(existingAccount);
        }

        debtorByIban[debtorAccount.Iban] = existingAccount;
        return existingAccount;
    }

    private BankoApi.Data.Dao.CreditorAccount GetOrCreateCreditorAccount(
        BankoDbContext ctx,
        BankoApi.Services.Model.CreditorAccount creditorAccount,
        Dictionary<string, BankoApi.Data.Dao.CreditorAccount> creditorByIban)
    {
        if (creditorByIban.TryGetValue(creditorAccount.Iban, out var cached)) return cached;

        var existingAccount = ctx.CreditorAccounts.FirstOrDefault(it => it.Iban == creditorAccount.Iban);
        if (existingAccount == null)
        {
            existingAccount = new BankoApi.Data.Dao.CreditorAccount
            {
                Bban = creditorAccount.Bban,
                Iban = creditorAccount.Iban
            };
            ctx.CreditorAccounts.Add(existingAccount);
        }

        creditorByIban[creditorAccount.Iban] = existingAccount;
        return existingAccount;
    }
}