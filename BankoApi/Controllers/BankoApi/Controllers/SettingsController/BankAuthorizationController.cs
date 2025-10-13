using BankoApi.Controllers.BankoApi.Controllers.Responses;
using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses;
using BankoApi.Controllers.GoCardless.Requests;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.SettingsController
{
    public partial class SettingsController
    {
        [HttpGet("BankAuthorization")]
        public async Task<IActionResult> GetBankInfoAsync()
        {
            try
            {
                return Ok(new GetBankAuthorizationResponse
                {
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse
                {
                    Message = BankAuthorizationErrorMessages.SomethingWentWrong.ToString()
                });
            }
        }

        [HttpPost("enduseragreement")]
        public async Task<IActionResult> UpsertEndUserAgreement([FromBody] UpsertEndUserAgreementRequest request)
        {
            try
            {
                // Check del GET Eua, se ce n'è una o più valide resitituire quella con più giorni prima della prossima expire date.
                // Se non ce n'è una disponibile, creare un nuovo EUA
                // Creare una nuova Requisition contentente Agreement, Reference e Institution
                // Salvare tutti i nuovi dati sul DB

                var eua = await _goCardlessService.GetEndUserAgreement();
                EndUserAgreement? validEua = _repository.HasValidNonAcceptedAgreement(eua);

                if (validEua == null)
                {
                    if (request.InstitutionId != null && request.DaysOfAccess != null)
                    {
                        validEua = await _goCardlessService.CreateEndUserAgreement(institutionId: request.InstitutionId, daysOfAccess: request.DaysOfAccess.Value);
                    }
                    else
                    {
                        String institutionId = "BIEN_SPAREBANK_BIENNOK1"; // TODO: prendere l'insitutionId dal DB
                        int daysOfAccess = eua.Results.Where(e => e.InstitutionId == institutionId).First().AccessValidForDays;

                        validEua = await _goCardlessService.CreateEndUserAgreement(institutionId: institutionId, daysOfAccess: daysOfAccess);
                    }
                }

                var requisition = await _goCardlessService.CreateRequisition(
                    institutionId: validEua.InstitutionId,
                    agreementId: validEua.Id
                );

                // TODO: Salvare sul DB tutte le info necessarie

                return Ok(new UpsertEndUserAgreementResponse()
                {
                    AgreementId = requisition.Agreement,
                    Link = requisition.Link,
                    InstitutionId = requisition.InstitutionId,
                    RequisitionId = requisition.Id,
                    ReferenceId = requisition.Reference
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest(new ErrorResponse() { Message = ex.Message });
            }
        }
    }
}
