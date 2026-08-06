using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using BankoApi.Exceptions.GoCardless.Transactions;
using BankoApi.Services;
using BankoApi.Services.Model;
using Moq;
using Moq.Protected;

namespace BankoApi.Tests.Services;

public class GoCardlessServiceTests
{
    [Fact]
    public async Task GetTransactionsAsync_Success_ReturnsTransactions()
    {
        var accountId = Guid.NewGuid();
        var transactionsResponse = new
        {
            transactions = new
            {
                booked = new[]
                {
                    new
                    {
                        transactionId = "tx-1",
                        bookingDate = "2024-01-15",
                        valueDate = "2024-01-15",
                        transactionAmount = new { amount = "100.00", currency = "EUR" },
                        remittanceInformationUnstructured = "Test payment",
                        remittanceInformationUnstructuredArray = new[] { "Test payment" },
                        internalTransactionId = "internal-1",
                        bankTransactionCode = "PMNT"
                    }
                },
                pending = Array.Empty<object>()
            }
        };

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(transactionsResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.GetTransactionsAsync(accountId);

        Assert.NotNull(result);
        Assert.Single(result.BankTransactions.Booked);
        Assert.Equal("tx-1", result.BankTransactions.Booked[0].TransactionId);
    }

    [Fact]
    public async Task GetTransactionsAsync_UnauthorizedWithEuaMessage_ThrowsEndUserAgreementException()
    {
        var accountId = Guid.NewGuid();
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.Unauthorized,
                Content = new StringContent($"EUA was valid for 90 days and it expired {Guid.NewGuid()}")
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);

        await Assert.ThrowsAsync<EndUserAgreementException>(() =>
            service.GetTransactionsAsync(accountId));
    }

    [Fact]
    public async Task GetTransactionsAsync_ApiError_ThrowsException()
    {
        var accountId = Guid.NewGuid();
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetTransactionsAsync(accountId));
    }

    [Fact]
    public async Task GetTransactionsAsync_TransientServiceUnavailableThenSuccess_ReturnsTransactions()
    {
        var accountId = Guid.NewGuid();
        var transactionsResponse = new
        {
            transactions = new
            {
                booked = new[]
                {
                    new
                    {
                        transactionId = "tx-1",
                        bookingDate = "2024-01-15",
                        valueDate = "2024-01-15",
                        transactionAmount = new { amount = "100.00", currency = "EUR" },
                        remittanceInformationUnstructured = "Test payment",
                        remittanceInformationUnstructuredArray = new[] { "Test payment" },
                        internalTransactionId = "internal-1",
                        bankTransactionCode = "PMNT"
                    }
                },
                pending = Array.Empty<object>()
            }
        };

        var callCount = 0;
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
        {
            callCount++;
            if (callCount < 3)
                return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(transactionsResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            };
        });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.GetTransactionsAsync(accountId);

        Assert.NotNull(result);
        Assert.Equal(3, callCount);
        Assert.Single(result.BankTransactions.Booked);
        Assert.Equal("tx-1", result.BankTransactions.Booked[0].TransactionId);
    }

    [Fact]
    public async Task GetTransactionsAsync_TransientServiceUnavailableExhausted_ThrowsException()
    {
        var accountId = Guid.NewGuid();
        var callCount = 0;
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
        });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetTransactionsAsync(accountId));
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task GetTransactionsAsync_TooManyRequestsThenSuccess_RetriesAndReturnsTransactions()
    {
        var accountId = Guid.NewGuid();
        var transactionsResponse = new
        {
            transactions = new
            {
                booked = new[]
                {
                    new
                    {
                        transactionId = "tx-1",
                        bookingDate = "2024-01-15",
                        valueDate = "2024-01-15",
                        transactionAmount = new { amount = "100.00", currency = "EUR" },
                        remittanceInformationUnstructured = "Test payment",
                        remittanceInformationUnstructuredArray = new[] { "Test payment" },
                        internalTransactionId = "internal-1",
                        bankTransactionCode = "PMNT"
                    }
                },
                pending = Array.Empty<object>()
            }
        };

        var callCount = 0;
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
        {
            callCount++;
            if (callCount == 1)
                return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
                {
                    Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(0)) }
                };

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(transactionsResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            };
        });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.GetTransactionsAsync(accountId);

        Assert.NotNull(result);
        Assert.Equal(2, callCount);
        Assert.Single(result.BankTransactions.Booked);
    }

    [Fact]
    public async Task GetTransactionsAsync_TooManyRequestsExhausted_ThrowsException()
    {
        var accountId = Guid.NewGuid();
        var callCount = 0;
        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
        {
            callCount++;
            return new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Headers = { RetryAfter = new RetryConditionHeaderValue(TimeSpan.FromSeconds(0)) }
            };
        });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            service.GetTransactionsAsync(accountId));
        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task GetEndUserAgreement_Success_ReturnsAgreements()
    {
        var agreementsResponse = new
        {
            count = 1,
            results = new[]
            {
                new
                {
                    id = "eua-1",
                    created = DateTime.UtcNow,
                    institution_id = "TEST_BANK",
                    max_historical_days = 90,
                    access_valid_for_days = 90,
                    access_scope = new[] { "balances", "transactions" },
                    accepted = (DateTime?)null,
                    reconfirmation = false
                }
            }
        };

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(agreementsResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.GetEndUserAgreement();

        Assert.NotNull(result);
        Assert.Single(result.Results);
        Assert.Equal("TEST_BANK", result.Results[0].InstitutionId);
    }

    [Fact]
    public async Task CreateEndUserAgreement_Success_ReturnsAgreement()
    {
        var agreementResponse = new
        {
            id = "new-eua-id",
            created = DateTime.UtcNow,
            institution_id = "TEST_BANK",
            max_historical_days = 30,
            access_valid_for_days = 30,
            access_scope = new[] { "balances", "transactions" },
            accepted = (DateTime?)null,
            reconfirmation = false
        };

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(agreementResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.CreateEndUserAgreement("TEST_BANK", 30);

        Assert.NotNull(result);
        Assert.Equal("new-eua-id", result.Id);
        Assert.Equal("TEST_BANK", result.InstitutionId);
    }

    [Fact]
    public async Task CreateRequisition_Success_ReturnsRequisition()
    {
        var requisitionResponse = new
        {
            id = "req-1",
            created = DateTime.UtcNow,
            redirect = "Banko://bank-auth-callback",
            status = "CR",
            institution_id = "TEST_BANK",
            agreement = "agreement-1",
            reference = "ref-1",
            accounts = new[] { "acc-1" },
            user_language = "EN",
            link = "https://bank.link",
            ssn = "",
            account_selection = false,
            redirect_immediate = false
        };

        var handlerMock = MockHelpers.CreateHandlerWithToken(_ =>
            new HttpResponseMessage(HttpStatusCode.Created)
            {
                Content = new StringContent(JsonSerializer.Serialize(requisitionResponse,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }))
            });

        var service = MockHelpers.CreateGoCardlessServiceWithHandler(handlerMock.Object);
        var result = await service.CreateRequisition("TEST_BANK", "agreement-1");

        Assert.NotNull(result);
        Assert.Equal("req-1", result.Id);
        Assert.Equal("TEST_BANK", result.InstitutionId);
        Assert.Equal("agreement-1", result.Agreement);
    }
}
