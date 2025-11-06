using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using PendingDto = BankoApi.Services.Model.Pending;

namespace BankoApi.Repository;

public class TransactionsRepository
{
    public async Task StoreTransactions(BankoDbContext ctx, Guid userId, Guid bankAccountId, Transactions transactions)
    {
        UpdatePendingTransactions(ctx, transactions.BankTransactions.Pending);

        var transactionsToStore = DiscardDuplicates(dbContext:ctx, transactions: transactions).Result
                .ToTransactionDao(dbContext: ctx, userId: userId, bankAccountId: bankAccountId)
                .OrderByDescending(it => it.BookingDate);

        ctx.Transactions.AddRange(transactionsToStore);
    }

    public void SetEuaExpirationStatus(BankoDbContext dbContext, String message)
    {
        String agreementId = FindAgreementId(message);
        var result = dbContext.BankAuthorizations.FirstOrDefault(r => r.AgreementId == agreementId);
        result.Status = Data.Dao.BankAuthorizationStaus.Expired;
        dbContext.SaveChanges();        
    }

    private string FindAgreementId(string input)
    {
        string pattern = @"[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}";
        Match match = Regex.Match(input, pattern);

        return match.Success ? match.Value : throw new EndUserAgreementException(FetchAndStoreTransactionResponse.AgreementIdNotFound.ToString());
    }

    private void UpdatePendingTransactions(BankoDbContext dbContext, List<PendingDto> pendings)
    {
        if (pendings.Count == 0) return;

        dbContext.Pendings.RemoveRange();
        dbContext.Pendings.AddRange(pendings.ToPendingDao());
    }

    private async Task<Transactions> DiscardDuplicates(BankoDbContext dbContext, Transactions transactions)
    {
        var storedTransactions = await dbContext.Transactions.Select(it => it.InternalTransactionId).ToListAsync();
        var transactionsToStore = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>(),
                Pending = new List<PendingDto>()
            }
        };
        foreach (var newTransaction in transactions.BankTransactions.Booked)
            
            if (!storedTransactions.Contains(newTransaction.InternalTransactionId) && !storedTransactions.Contains(newTransaction.TransactionId))
            {
                if (newTransaction.TransactionId.IsNullOrEmpty())
                {
                    newTransaction.TransactionId = Guid.NewGuid().ToString();
                }
                transactionsToStore.BankTransactions.Booked.Add(newTransaction);
            }   

        return transactionsToStore;
    }
}