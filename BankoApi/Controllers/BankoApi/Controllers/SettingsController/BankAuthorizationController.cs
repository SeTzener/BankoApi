using BankoApi.Controllers.BankoApi.Controllers.Responses;
using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Requests;
using BankoApi.Controllers.BankoApi.Controllers.SettingsController.Responses;
using BankoApi.Controllers.GoCardless.Requests;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.Settings;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.BankoApi.Controllers.SettingsController.SettingsController
{
    public partial class SettingsController
    {
        [HttpGet("BankAuthorization")]
        public async Task<IActionResult> GetBankAuthorization(Guid userId)
        {
            try
            {
                var result = _dbContext.BankAuthorizations.Where(ba => ba.UserId == userId).ToList();
                if (result.Count == 0) throw new NoBankAuthorizationFoundException($"The user {userId} doesn't have any bank authorization process started yet.");
                return Ok(new GetBankAuthorizationsResponse
                {
                    BankAuthorizations = result.ConvertAll(it => new BankAuth()
                    {
                        Id = it.Id,
                        UserId = it.UserId,
                        RequisitionId = it.RequisitionId,
                        InstitutionId = it.InstitutionId,
                        ReferenceId = it.ReferenceId,
                        AgreementId = it.AgreementId,
                        Status = it.Status,
                        InstitutionName = it.InstitutionName,
                        CreatedAt = it.CreatedAt,
                        UpdatedAt = it.UpdatedAt
                    })
                });
            }
            catch (NoBankAuthorizationFoundException ex)
            {
                Console.WriteLine(ex.Message);
                return NotFound(new ErrorResponse
                {
                    Message = BankAuthorizationErrorMessages.NoAuthorizationFound.ToString(),
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

        [HttpPut("BankAuthorization")]
        public async Task<IActionResult> UpsertBankAuthorization([FromBody] UpsertBankAuthorizationRequest request)
        {
            try
            {
                if (!_dbContext.Users.Where(x => x.UserId == request.UserId).Any())
                {
                    return Unauthorized();
                }
                // Find or create the authorization
                var authorization = await _dbContext.BankAuthorizations
                    .FirstOrDefaultAsync(ba => ba.AgreementId == request.AgreementId)
                    ?? new BankAuthorization { AgreementId = request.AgreementId };

                authorization.Status = request.Status;

                // Update fields if they're provided in the request
                if (request.InstitutionId != null)
                    authorization.InstitutionId = request.InstitutionId;

                if (request.RequisitionId != null)
                    authorization.RequisitionId = request.RequisitionId;

                if (request.ReferenceId != null)
                    authorization.ReferenceId = request.ReferenceId;

                if (request.InstitutionName != null)
                    authorization.InstitutionName = request.InstitutionName;


                if (authorization.Id == Guid.Empty) // It's new
                {
                    authorization.UserId = request.UserId; // TODO(): Take it from the token
                    _dbContext.BankAuthorizations.Add(authorization);
                }
                else
                {
                    authorization.UpdatedAt = DateTime.UtcNow;
                }


                await _dbContext.SaveChangesAsync();

                return Ok(new UpsertBankAuthorizationResponse()
                {
                    Id = authorization.Id,
                    UserId = authorization.UserId,
                    RequisitionId = authorization.RequisitionId,
                    InstitutionId = authorization.InstitutionId,
                    Status = authorization.Status,
                    AgreementId = authorization.AgreementId,
                    ReferenceId = authorization.ReferenceId,
                    InstitutionName = authorization.InstitutionName,
                    CreatedAt = authorization.CreatedAt,
                    UpdatedAt = authorization.UpdatedAt,
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

        // TODO: Cambiare in PUT
        [HttpPost("Enduseragreement")]
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
