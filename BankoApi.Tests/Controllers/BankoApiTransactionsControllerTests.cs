using System.Security.Claims;
using BankoApi.Controllers.Settings.Requests;
using BankoApi.Controllers.Transactions;
using BankoApi.Controllers.Transactions.Requests;
using BankoApi.Data;
using BankoApi.Data.Dao;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Tests.Controllers;

public class BankoApiTransactionsControllerTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    private BankoDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<BankoDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new BankoDbContext(options);
    }

    private static TransactionsController CreateControllerWithUser(BankoDbContext ctx)
    {
        var controller = new TransactionsController(ctx)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, TestUserId.ToString())
                    }))
                }
            }
        };
        return controller;
    }

    [Fact]
    public async Task GetTransactions_NoFilters_ReturnsOk()
    {
        using var ctx = CreateContext();
        SeedTransaction(ctx);
        var controller = CreateControllerWithUser(ctx);

        var result = await controller.GetTransactions();

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetTransactions_WithDateRange_ReturnsFilteredResults()
    {
        using var ctx = CreateContext();
        SeedTransaction(ctx);
        var controller = CreateControllerWithUser(ctx);

        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);

        var result = await controller.GetTransactions(fromDate: fromDate, toDate: toDate);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(okResult.Value);
    }

    [Fact]
    public async Task GetTransactions_InvalidPageNumber_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);

        var result = await controller.GetTransactions(pageNumber: 0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetTransactions_InvalidPageSize_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);

        var result = await controller.GetTransactions(pageSize: 0);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetTransactions_FromDateAfterToDate_ReturnsBadRequest()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);

        var result = await controller.GetTransactions(
            fromDate: new DateTime(2024, 12, 31),
            toDate: new DateTime(2024, 1, 1));

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeleteTransaction_ExistingTransaction_MarksAsDeleted()
    {
        using var ctx = CreateContext();
        var tx = new Transaction
        {
            Id = "tx-to-delete",
            UserId = TestUserId,
            BankAccountId = Guid.NewGuid(),
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "100.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Test",
            RemittanceInformationUnstructuredArray = new List<string> { "Test" },
            InternalTransactionId = "internal-del"
        };
        ctx.Transactions.Add(tx);
        ctx.SaveChanges();

        var controller = CreateControllerWithUser(ctx);
        var result = await controller.DeleteTransaction("tx-to-delete");

        Assert.IsType<OkObjectResult>(result);
        Assert.True(ctx.Transactions.Find("tx-to-delete")!.isDeleted);
    }

    [Fact]
    public async Task DeleteTransaction_NonExistingTransaction_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);

        var result = await controller.DeleteTransaction("non-existent");

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateExpenseTags_ExistingTransaction_UpdatesExpenseTag()
    {
        using var ctx = CreateContext();
        var tagId = "tag-1";
        ctx.ExpenseTag.Add(new ExpenseTag
        {
            Id = tagId,
            Name = "Test Tag",
            Color = 123456,
            isEarning = false
        });
        var tx = new Transaction
        {
            Id = "tx-tag-update",
            UserId = TestUserId,
            BankAccountId = Guid.NewGuid(),
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "100.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Test",
            RemittanceInformationUnstructuredArray = new List<string> { "Test" },
            InternalTransactionId = "internal-tag"
        };
        ctx.Transactions.Add(tx);
        ctx.SaveChanges();

        var controller = CreateControllerWithUser(ctx);
        var request = new UpdateExpenseTagRequest
        {
            TransactionId = "tx-tag-update",
            ExpenseTagId = tagId
        };

        var result = await controller.UpdateExpenseTags(request);

        Assert.IsType<OkObjectResult>(result);
        var updated = ctx.Transactions.Find("tx-tag-update");
        Assert.Equal(tagId, updated!.ExpenseTagId);
    }

    [Fact]
    public async Task UpdateExpenseTags_NullExpenseTagId_ClearsExpenseTag()
    {
        using var ctx = CreateContext();
        var tx = new Transaction
        {
            Id = "tx-clear-tag",
            UserId = TestUserId,
            BankAccountId = Guid.NewGuid(),
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "100.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Test",
            RemittanceInformationUnstructuredArray = new List<string> { "Test" },
            InternalTransactionId = "internal-clear",
            ExpenseTagId = "old-tag"
        };
        ctx.Transactions.Add(tx);
        ctx.SaveChanges();

        var controller = CreateControllerWithUser(ctx);
        var request = new UpdateExpenseTagRequest
        {
            TransactionId = "tx-clear-tag",
            ExpenseTagId = null
        };

        var result = await controller.UpdateExpenseTags(request);

        Assert.IsType<OkObjectResult>(result);
        var updated = ctx.Transactions.Find("tx-clear-tag");
        Assert.Null(updated!.ExpenseTagId);
    }

    [Fact]
    public async Task UpdateExpenseTags_NonExistingTransaction_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);
        var request = new UpdateExpenseTagRequest
        {
            TransactionId = "non-existent",
            ExpenseTagId = "tag-1"
        };

        var result = await controller.UpdateExpenseTags(request);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateNote_ExistingTransaction_UpdatesNote()
    {
        using var ctx = CreateContext();
        var tx = new Transaction
        {
            Id = "tx-note",
            UserId = TestUserId,
            BankAccountId = Guid.NewGuid(),
            BookingDate = DateTime.UtcNow,
            ValueDate = DateTime.UtcNow,
            Amount = "100.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Test",
            RemittanceInformationUnstructuredArray = new List<string> { "Test" },
            InternalTransactionId = "internal-note"
        };
        ctx.Transactions.Add(tx);
        ctx.SaveChanges();

        var controller = CreateControllerWithUser(ctx);
        var request = new UpdateNoteRequest { Note = "Updated note" };

        var result = await controller.UpdateNote("tx-note", request);

        Assert.IsType<OkObjectResult>(result);
        var updated = ctx.Transactions.Find("tx-note");
        Assert.Equal("Updated note", updated!.Note);
    }

    [Fact]
    public async Task UpdateNote_NonExistingTransaction_ReturnsNotFound()
    {
        using var ctx = CreateContext();
        var controller = CreateControllerWithUser(ctx);
        var request = new UpdateNoteRequest { Note = "Note" };

        var result = await controller.UpdateNote("non-existent", request);

        Assert.IsType<NotFoundResult>(result);
    }

    private void SeedTransaction(BankoDbContext ctx)
    {
        ctx.Transactions.Add(new Transaction
        {
            Id = "tx-1",
            UserId = TestUserId,
            BankAccountId = Guid.NewGuid(),
            BookingDate = new DateTime(2024, 6, 15),
            ValueDate = new DateTime(2024, 6, 15),
            Amount = "150.00",
            Currency = "EUR",
            RemittanceInformationUnstructured = "Seed transaction",
            RemittanceInformationUnstructuredArray = new List<string> { "Seed transaction" },
            InternalTransactionId = "internal-seed"
        });
        ctx.SaveChanges();
    }
}
