using BankoApi.Data.Dao;

namespace BankoApi.Controllers.Settings.Responses
{
    public class GetBankAuthorizationsResponse
    {
        public List<BankAuth> BankAuthorizations { get; set; }
    }

    public class BankAuth
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public String? RequisitionId { get; set; }
        public String? InstitutionId { get; set; }
        public String? ReferenceId { get; set; }
        public String? AgreementId { get; set; }
        public BankAuthorizationStaus Status { get; set; }
        public String? InstitutionName { get; set; }
        public String? InstitutionLogoUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BankAccountSummary> Accounts { get; set; } = new();
    }

    public class BankAccountSummary
    {
        public String BankAccountId { get; set; } = string.Empty;
        public String? Iban { get; set; }
        public String? Currency { get; set; }
        public String? OwnerName { get; set; }
        public String? AccountName { get; set; }
        public String? Product { get; set; }
    }
}
