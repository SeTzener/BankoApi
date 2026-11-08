using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Repository;

public class BankAuthorizationRepository
{
    /// <summary>
    /// Deletes authorizations that never completed their first link and have been
    /// abandoned for longer than the threshold.
    /// Rows that already own accounts are kept: they belong to an existing connection
    /// (or a partially processed callback) and must not lose their history.
    /// </summary>
    public async Task<int> DeleteStaleProcessingAuthorizations(BankoDbContext dbContext, DateTime threshold)
    {
        var staleAuthorizations = await dbContext.BankAuthorizations
            .Where(ba => ba.Status == BankAuthorizationStaus.Processing
                         && ba.UpdatedAt < threshold
                         && !dbContext.BankAccounts.Any(account => account.BankAuthorizationId == ba.Id))
            .ToListAsync();

        if (staleAuthorizations.Count == 0) return 0;

        dbContext.BankAuthorizations.RemoveRange(staleAuthorizations);
        await dbContext.SaveChangesAsync();
        return staleAuthorizations.Count;
    }

    public EndUserAgreement? HasValidNonAcceptedAgreement(PaginatedEndUserAgreements agreements)
    {
        if (agreements?.Results == null || agreements.Results.Count == 0) { return null; }

        var newestNonAccepted = agreements.Results
        .Where(eua => eua.Accepted == null)
        .OrderByDescending(eua => eua.Created)
        .FirstOrDefault();

        if (newestNonAccepted == null) { return null; }

        // Check if it has at least 1 week (7 days) of validity remaining
        var expirationDate = newestNonAccepted.Created.AddDays(newestNonAccepted.AccessValidForDays);
        var timeRemaining = expirationDate - DateTime.UtcNow;

        return timeRemaining.TotalDays >= 7 ? newestNonAccepted : null;
    }
}