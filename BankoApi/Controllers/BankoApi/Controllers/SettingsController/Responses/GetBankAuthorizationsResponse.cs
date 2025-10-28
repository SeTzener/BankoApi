using BankoApi.Data.Dao;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses
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
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
