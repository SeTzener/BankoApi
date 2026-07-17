using BankoApi.Data.Dao;

namespace BankoApi.Controllers.Settings.Responses
{
    public class BankAuthCallbackResponse
    {
        public Guid BankAuthorizationId { get; set; }
        public BankAuthorizationStaus Status { get; set; }
        public List<LinkedBankAccount> LinkedAccounts { get; set; } = new();
    }

    public class LinkedBankAccount
    {
        public String BankAccountId { get; set; } = string.Empty;
        public String? Iban { get; set; }
        public String? Currency { get; set; }
        public String? OwnerName { get; set; }
        public String? AccountName { get; set; }
        public String? Product { get; set; }
    }
}
