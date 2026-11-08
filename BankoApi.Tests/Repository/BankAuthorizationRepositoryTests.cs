using BankoApi.Data;
using BankoApi.Data.Dao;
using BankoApi.Repository;
using BankoApi.Services.Model;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Tests.Repository;

public class BankAuthorizationRepositoryTests
{
    private static BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

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

    [Fact]
    public async Task DeleteStaleProcessingAuthorizations_DeletesOnlyAbandonedRowsWithoutAccounts()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var staleId = Guid.NewGuid();
        var recentId = Guid.NewGuid();
        var linkedId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        ctx.BankAuthorizations.AddRange(
            new BankAuthorization
            {
                Id = staleId,
                UserId = userId,
                Status = BankAuthorizationStaus.Processing,
                CreatedAt = now.AddHours(-25),
                UpdatedAt = now.AddHours(-25)
            },
            new BankAuthorization
            {
                Id = recentId,
                UserId = userId,
                Status = BankAuthorizationStaus.Processing,
                CreatedAt = now,
                UpdatedAt = now
            },
            new BankAuthorization
            {
                Id = linkedId,
                UserId = userId,
                Status = BankAuthorizationStaus.Linked,
                CreatedAt = now.AddHours(-25),
                UpdatedAt = now.AddHours(-25)
            });
        await ctx.SaveChangesAsync();

        var repo = new BankAuthorizationRepository();
        var removed = await repo.DeleteStaleProcessingAuthorizations(ctx, now.AddHours(-24));

        Assert.Equal(1, removed);
        var remaining = ctx.BankAuthorizations.Select(ba => ba.Id).ToList();
        Assert.DoesNotContain(staleId, remaining);
        Assert.Contains(recentId, remaining);
        Assert.Contains(linkedId, remaining);
    }

    [Fact]
    public async Task DeleteStaleProcessingAuthorizations_UsesUpdatedAtNotCreatedAt()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var renewalId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        // A renewal on an old connection refreshes UpdatedAt, so it must not be treated as stale.
        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = renewalId,
            UserId = userId,
            Status = BankAuthorizationStaus.Processing,
            CreatedAt = now.AddDays(-90),
            UpdatedAt = now.AddMinutes(-5)
        });
        await ctx.SaveChangesAsync();

        var repo = new BankAuthorizationRepository();
        var removed = await repo.DeleteStaleProcessingAuthorizations(ctx, now.AddHours(-24));

        Assert.Equal(0, removed);
        Assert.Single(ctx.BankAuthorizations);
    }

    [Fact]
    public async Task DeleteStaleProcessingAuthorizations_KeepsRowsThatOwnAccounts()
    {
        using var ctx = CreateContext();
        var userId = Guid.NewGuid();
        var authId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        ctx.BankAuthorizations.Add(new BankAuthorization
        {
            Id = authId,
            UserId = userId,
            Status = BankAuthorizationStaus.Processing,
            CreatedAt = now.AddDays(-90),
            UpdatedAt = now.AddDays(-2)
        });
        ctx.BankAccounts.Add(new BankAccount
        {
            Id = Guid.NewGuid(),
            BankAuthorizationId = authId,
            BankAccountId = Guid.NewGuid().ToString(),
            CreatedAt = now.AddDays(-90),
            UpdatedAt = now.AddDays(-90)
        });
        await ctx.SaveChangesAsync();

        var repo = new BankAuthorizationRepository();
        var removed = await repo.DeleteStaleProcessingAuthorizations(ctx, now.AddHours(-24));

        Assert.Equal(0, removed);
        Assert.Single(ctx.BankAuthorizations);
    }
}
