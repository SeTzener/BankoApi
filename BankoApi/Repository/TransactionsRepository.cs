using BankoApi.Data;
using BankoApi.Services.Model;
using PendingDto = BankoApi.Services.Model.Pending;

namespace BankoApi.Repository;

public class TransactionsRepository
{
    public void StoreTransactions(BankoDbContext ctx, Transactions transactions)
    {
        UpdatePendingTransactions(ctx, transactions.BankTransactions.Pending);

        var transactionsToStore = DiscardDuplicates(ctx, transactions).ToTransactionDao(ctx);

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
        var storedTransactions = dbContext.Transactions.Select(it => it.Id).ToList();
        var transactionsToStore = new Transactions()
        {
            BankTransactions = new BankTransactions()
            {
                Booked = new List<Booked>(),
                Pending = new List<PendingDto>(),
            }
        };
        foreach (var newTransaction in transactions.BankTransactions.Booked)
        {
            var prova = storedTransactions.Select(it => it).FirstOrDefault(it => it == newTransaction.TransactionId);
            Console.WriteLine($"Stored ID: {prova}");
            Console.WriteLine($"New    ID: {newTransaction.TransactionId}");
            Console.WriteLine("");
            if (!storedTransactions.Contains(newTransaction.TransactionId))
            {
                transactionsToStore.BankTransactions.Booked.Add(newTransaction);
            }
        }

        return transactionsToStore;
    }
}