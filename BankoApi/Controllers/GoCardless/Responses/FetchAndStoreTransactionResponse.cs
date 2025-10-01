namespace BankoApi.Controllers.GoCardless.Responses
{
    enum FetchAndStoreTransactionResponse
    {
       TransactionsStoredSuccessfully,
       SomethingWentWrong,
       EndUserAgreementExpired,
       AgreementIdNotFound,
       NoTransactionsFound
    }
}
