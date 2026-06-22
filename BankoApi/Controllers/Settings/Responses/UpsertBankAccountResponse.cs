using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.Settings.Responses
{
    public class UpsertBankAccountResponse
    {
        public Guid BankAuthorizationId { get; set; }
        public String BankAccountId { get; set; }
        public String? Iban { get; set; }
        public String? Bban { get; set; }
        public String? Currency { get; set; }
        public String? OwnerName { get; set; }
        public String? AccountName { get; set; }
        public String? Product { get; set; }
    }
}
