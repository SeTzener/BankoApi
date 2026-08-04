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
    public async Task StoreTransactions(BankoDbContext ctx, Guid userId, Guid bankAccountId, Transactions transactions)
    {
        await UpdateExistingTransactions(ctx, userId, transactions, bankAccountId);
        await ctx.SaveChangesAsync();
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

    private async Task UpdateExistingTransactions(BankoDbContext ctx, Guid userId, Transactions transactions, Guid bankAccountId)
    {
        foreach (var newTransaction in transactions.BankTransactions.Booked)
        {
            if (string.IsNullOrEmpty(newTransaction.TransactionId))
            {
                newTransaction.TransactionId = Guid.NewGuid().ToString();
            }

            if (!string.IsNullOrEmpty(newTransaction.InternalTransactionId))
            {
                var existingTransaction = await ctx.Transactions
                    .FirstOrDefaultAsync(t => t.InternalTransactionId == newTransaction.InternalTransactionId);
                
                if (existingTransaction != null)
                {
                    UpdateTransactionData(ctx, existingTransaction, newTransaction);
                }
                else
                {
                    CreateNewTransaction(ctx, userId, newTransaction, bankAccountId);
                }
            }
        }
    }

    private void UpdateTransactionData(BankoDbContext ctx, Transaction existingTransaction, Booked newTransaction)
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
            existingTransaction.DebtorAccount = GetOrCreateDebtorAccount(ctx, newTransaction.DebtorAccount);
        }

        if (newTransaction.CreditorAccount != null)
        {
            existingTransaction.CreditorAccount = new CreditorAccountDao
            {
                Bban = newTransaction.CreditorAccount.Bban,
                Iban = newTransaction.CreditorAccount.Iban
            };
        }
    }

    private void CreateNewTransaction(BankoDbContext ctx, Guid userId, Booked newTransaction, Guid bankAccountId)
    {
        var transaction = new Transaction
        {
            Id = newTransaction.TransactionId!,
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
            transaction.DebtorAccount = GetOrCreateDebtorAccount(ctx, newTransaction.DebtorAccount);
        }

        if (newTransaction.CreditorAccount != null)
        {
            transaction.CreditorAccount = new CreditorAccountDao
            {
                Bban = newTransaction.CreditorAccount.Bban,
                Iban = newTransaction.CreditorAccount.Iban
            };
        }

        ctx.Transactions.Add(transaction);
    }

    private BankoApi.Data.Dao.DebtorAccount GetOrCreateDebtorAccount(BankoDbContext ctx, BankoApi.Services.Model.DebtorAccount debtorAccount)
    {
        var existingAccount = ctx.DebtorAccounts.FirstOrDefault(it => it.Iban == debtorAccount.Iban);
        if (existingAccount != null) return existingAccount;

        var newAccount = new BankoApi.Data.Dao.DebtorAccount
        {
            Bban = debtorAccount.Bban,
            Iban = debtorAccount.Iban
        };

        ctx.DebtorAccounts.Add(newAccount);
        return newAccount;
    }
}