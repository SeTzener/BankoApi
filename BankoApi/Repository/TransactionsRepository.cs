using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;

namespace BankoApi.Repository;

public class TransactionsRepository
{
    public async Task StoreTransactions(BankoDbContext ctx, Guid userId, Guid bankAccountId, Transactions transactions)
    {
        var transactionsToStore = (await DiscardDuplicates(dbContext:ctx, transactions: transactions))
                .ToTransactionDao(dbContext: ctx, userId: userId, bankAccountId: bankAccountId)
                .OrderByDescending(it => it.BookingDate);

        ctx.Transactions.AddRange(transactionsToStore);
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

    private async Task<Transactions> DiscardDuplicates(BankoDbContext dbContext, Transactions transactions)
    {
        var candidateIds = transactions.BankTransactions.Booked
            .SelectMany(t => new[] { t.InternalTransactionId, t.TransactionId })
            .Where(id => !string.IsNullOrEmpty(id))
            .Distinct()
            .ToList();

        var existingInternalIds = new HashSet<string>(await dbContext.Transactions
            .AsNoTracking()
            .Where(t => candidateIds.Contains(t.InternalTransactionId))
            .Select(t => t.InternalTransactionId)
            .ToListAsync());

        var existingDbIds = new HashSet<string>(await dbContext.Transactions
            .AsNoTracking()
            .Where(t => candidateIds.Contains(t.Id))
            .Select(t => t.Id)
            .ToListAsync());

        existingInternalIds.UnionWith(existingDbIds);

        var transactionsToStore = new Transactions
        {
            BankTransactions = new BankTransactions
            {
                Booked = new List<Booked>()
            }
        };
        foreach (var newTransaction in transactions.BankTransactions.Booked)
            
            if (!existingInternalIds.Contains(newTransaction.InternalTransactionId) && !existingInternalIds.Contains(newTransaction.TransactionId))
            {
                if (string.IsNullOrEmpty(newTransaction.TransactionId))
                {
                    newTransaction.TransactionId = Guid.NewGuid().ToString();
                }
                transactionsToStore.BankTransactions.Booked.Add(newTransaction);
            }   

        return transactionsToStore;
    }
}