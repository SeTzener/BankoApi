using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Repository;

public class BankAuthorizationRepository
{
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