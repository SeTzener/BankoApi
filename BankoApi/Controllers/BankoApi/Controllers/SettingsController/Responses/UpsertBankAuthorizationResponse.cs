using BankoApi.Data.Dao;
using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses
{
    public class UpsertBankAuthorizationResponse
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public String? RequisitionId { get; set; }
        public String? InstitutionId { get; set; }
        public BankAuthorizationStaus Status { get; set; }
        public String? AgreementId { get; set; }
        public String? ReferenceId { get; set; }
        public String? InstitutionName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
