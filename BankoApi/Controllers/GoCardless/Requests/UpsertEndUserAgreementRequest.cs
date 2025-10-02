namespace BankoApi.Controllers.GoCardless.Requests
{
    public class UpsertEndUserAgreementRequest
    {
        public String? InstitutionId { get; set; }
        public int? DaysOfAccess { get; set; }
    }
}