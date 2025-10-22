using BankoApi.Data.Dao;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Requests
{
    public class UpsertBankAuthorizationRequest
    {
        public Guid UserId { get; set; }
        public BankAuthorizationStaus Status {  get; set; }
        public String? RequisitionId { get; set; }
        public String? InstitutionId { get; set; }
        public String? ReferenceId { get; set; }
        public String? InstitutionName { get; set; }
        public String? AgreementId { get; set; }
    }
}
