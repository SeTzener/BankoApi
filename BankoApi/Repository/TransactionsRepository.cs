using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace BankoApi.Repository;

using CreditorAccountDao = BankoApi.Data.Dao.CreditorAccount;
using DebtorAccountDao = BankoApi.Data.Dao.DebtorAccount;

public class TransactionsRepository
{
    private const int SaveBatchSize = 200;

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

        var existingByKey = new Dictionary<string, Transaction>();
        foreach (var transaction in existingTransactions)
        {
            existingByKey.TryAdd(GetMatchKey(transaction), transaction);
        }

        var creditorByIban = new Dictionary<string, CreditorAccountDao>();
        var debtorByIban = new Dictionary<string, DebtorAccountDao>();
        var seenKeys = new HashSet<string>();
        var pendingTransactions = new List<Transaction>();

        foreach (var newTransaction in booked)
        {
            string? id = newTransaction.TransactionId;
            if (string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(newTransaction.InternalTransactionId))
                id = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(id) && string.IsNullOrEmpty(newTransaction.InternalTransactionId))
                continue;

            var key = GetMatchKey(newTransaction, id!);
            if (!seenKeys.Add(key)) continue;

            if (existingByKey.TryGetValue(key, out var existingTransaction))
            {
                UpdateTransactionData(ctx, existingTransaction, newTransaction, creditorByIban, debtorByIban);
            }
            else
            {
                pendingTransactions.Add(
                    CreateNewTransaction(ctx, userId, newTransaction, bankAccountId, id!, creditorByIban, debtorByIban));
            }
        }

        await SaveInBatches(ctx, pendingTransactions);
    }

    private static string GetMatchKey(Transaction transaction)
    {
        return string.IsNullOrEmpty(transaction.InternalTransactionId)
            ? transaction.Id
            : transaction.InternalTransactionId;
    }

    private static string GetMatchKey(Booked booked, string id)
    {
        return string.IsNullOrEmpty(booked.InternalTransactionId)
            ? id
            : booked.InternalTransactionId;
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
        existingTransaction.isDeleted = false;

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