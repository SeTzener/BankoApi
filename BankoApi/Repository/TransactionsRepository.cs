using BankoApi.Data;
using BankoApi.Services.Model;
using Microsoft.IdentityModel.Tokens;
using PendingDto = BankoApi.Services.Model.Pending;

namespace BankoApi.Repository;

public class TransactionsRepository
{
    public void StoreTransactions(BankoDbContext ctx, Guid userId, Transactions transactions)
    {
        UpdatePendingTransactions(ctx, transactions.BankTransactions.Pending);

        var transactionsToStore = DiscardDuplicates(dbContext:ctx, transactions: transactions)
                .ToTransactionDao(dbContext: ctx, userId: userId)
                .OrderByDescending(it => it.BookingDate);

        ctx.Transactions.AddRange(transactionsToStore);
    }

    private void UpdatePendingTransactions(BankoDbContext dbContext, List<PendingDto> pendings)
    {
        if (pendings.Count == 0) return;

        dbContext.Pendings.RemoveRange();
        dbContext.Pendings.AddRange(pendings.ToPendingDao());
    }

    private Transactions DiscardDuplicates(BankoDbContext dbContext, Transactions transactions)
    {
        var storedTransactions = dbContext.Transactions.Select(it => it.InternalTransactionId).ToList();
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