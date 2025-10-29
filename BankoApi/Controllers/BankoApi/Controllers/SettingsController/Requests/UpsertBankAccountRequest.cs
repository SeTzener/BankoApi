namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Requests
{
    public class UpsertBankAccountRequest
    {
        public Guid BankAuthorizationId { get; set; }
        public String AccountId { get; set; }
        public String? Iban { get; set; }
        public String? Bban { get; set; }
        public String? Currency { get; set; }
        public String? OwnerName { get; set; }
        public String? AccountName { get; set; }
        public String? Product { get; set; }
    }
}
