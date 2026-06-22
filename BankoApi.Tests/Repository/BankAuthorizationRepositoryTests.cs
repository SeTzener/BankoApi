using BankoApi.Repository;
using BankoApi.Services.Model;

namespace BankoApi.Tests.Repository;

public class BankAuthorizationRepositoryTests
{
    [Fact]
    public void HasValidNonAcceptedAgreement_NullResults_ReturnsNull()
    {
        var repo = new BankAuthorizationRepository();
        var result = repo.HasValidNonAcceptedAgreement(new PaginatedEndUserAgreements());
        Assert.Null(result);
    }

    [Fact]
    public void HasValidNonAcceptedAgreement_EmptyResults_ReturnsNull()
    {
        var repo = new BankAuthorizationRepository();
        var agreements = new PaginatedEndUserAgreements
        {
            Results = new List<EndUserAgreement>()
        };
        var result = repo.HasValidNonAcceptedAgreement(agreements);
        Assert.Null(result);
    }

    [Fact]
    public void HasValidNonAcceptedAgreement_AllAccepted_ReturnsNull()
    {
        var repo = new BankAuthorizationRepository();
        var agreements = new PaginatedEndUserAgreements
        {
            Results = new List<EndUserAgreement>
            {
                new()
                {
                    Id = "1",
                    Created = DateTime.UtcNow.AddDays(-10),
                    AccessValidForDays = 30,
                    Accepted = DateTime.UtcNow.AddDays(-5)
                }
            }
        };
        var result = repo.HasValidNonAcceptedAgreement(agreements);
        Assert.Null(result);
    }

    [Fact]
    public void HasValidNonAcceptedAgreement_NonAcceptedWithSufficientTime_ReturnsAgreement()
    {
        var repo = new BankAuthorizationRepository();
        var agreement = new EndUserAgreement
        {
            Id = "valid-id",
            Created = DateTime.UtcNow,
            AccessValidForDays = 30,
            Accepted = null
        };
        var agreements = new PaginatedEndUserAgreements
        {
            Results = new List<EndUserAgreement> { agreement }
        };

        var result = repo.HasValidNonAcceptedAgreement(agreements);
        Assert.NotNull(result);
        Assert.Equal("valid-id", result.Id);
    }

    [Fact]
    public void HasValidNonAcceptedAgreement_NonAcceptedExpiringSoon_ReturnsNull()
    {
        var repo = new BankAuthorizationRepository();
        var agreement = new EndUserAgreement
        {
            Id = "expiring-id",
            Created = DateTime.UtcNow.AddDays(-25),
            AccessValidForDays = 30,
            Accepted = null
        };
        var agreements = new PaginatedEndUserAgreements
        {
            Results = new List<EndUserAgreement> { agreement }
        };

        var result = repo.HasValidNonAcceptedAgreement(agreements);
        Assert.Null(result);
    }

    [Fact]
    public void HasValidNonAcceptedAgreement_MultipleNonAccepted_ReturnsNewestWithSufficientTime()
    {
        var repo = new BankAuthorizationRepository();
        var oldAgreement = new EndUserAgreement
        {
            Id = "old-id",
            Created = DateTime.UtcNow.AddDays(-20),
            AccessValidForDays = 30,
            Accepted = null
        };
        var newAgreement = new EndUserAgreement
        {
            Id = "new-id",
            Created = DateTime.UtcNow,
            AccessValidForDays = 30,
            Accepted = null
        };
        var agreements = new PaginatedEndUserAgreements
        {
            Results = new List<EndUserAgreement> { oldAgreement, newAgreement }
        };

        var result = repo.HasValidNonAcceptedAgreement(agreements);
        Assert.NotNull(result);
        Assert.Equal("new-id", result.Id);
    }
}
