using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses
{
    public class GetBankAccountsResponse
    {
        public List<BankAccountResponse> Accounts {  get; set; }
    }

    public class BankAccountResponse
    {
        public Guid BankAuthorizationId { get; set; }
        public String BankAccountId { get; set; }
        public String? Iban { get; set; }
        public String? Bban { get; set; }
        public String? Currency { get; set; }
        public String? OwnerName { get; set; }
        public String? Product { get; set; }
        public String? AccountName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    internal enum GetBankAccountErrorMessages
    {
        AccountNotFound
    }
}
