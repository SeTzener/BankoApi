namespace BankoApi.Controllers.GoCardless.Responses
{
    public class UpsertEndUserAgreementResponse
    {
        public String Link { get; set; }
        public String RequisitionId { get; set; }
        public String AgreementId { get; set; }
        public String ReferenceId { get; set; }
        public String InstitutionId { get; set; }
    }
}
