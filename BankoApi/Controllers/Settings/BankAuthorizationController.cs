using BankoApi.Controllers.Shared;
using BankoApi.Controllers.Settings.Requests;
using BankoApi.Controllers.Settings.Responses;
using BankoApi.Controllers.GoCardless.Requests;
using BankoApi.Controllers.GoCardless.Responses;
using BankoApi.Data.Dao;
using BankoApi.Exceptions.Settings;
using BankoApi.Services.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankoApi.Controllers.Settings
{
    public partial class SettingsController
    {
        [HttpGet("BankAuthorization")]
        public async Task<IActionResult> GetBankAuthorization()
        {
            try
            {
                var userId = User.GetUserId();
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
                _logger.LogError(ex, "No bank authorization found");
                return NotFound(new ErrorResponse
                {
                    Message = BankAuthorizationErrorMessages.NoAuthorizationFound.ToString(),
                });
            }
        }

        [HttpPut("BankAuthorization")]
        public async Task<IActionResult> UpsertBankAuthorization([FromBody] UpsertBankAuthorizationRequest request)
        {
            if (!_dbContext.Users.Where(x => x.UserId == request.UserId).Any())
            {
                return Unauthorized();
            }

            var authorization = await _dbContext.BankAuthorizations
                .FirstOrDefaultAsync(ba => ba.AgreementId == request.AgreementId)
                ?? new BankAuthorization { AgreementId = request.AgreementId };

            authorization.Status = request.Status;

            if (request.InstitutionId != null)
                authorization.InstitutionId = request.InstitutionId;

            if (request.RequisitionId != null)
                authorization.RequisitionId = request.RequisitionId;

            if (request.ReferenceId != null)
                authorization.ReferenceId = request.ReferenceId;

            if (request.InstitutionName != null)
                authorization.InstitutionName = request.InstitutionName;

            if (authorization.Id == Guid.Empty)
            {
                authorization.UserId = request.UserId;
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

        [HttpPost("end-user-agreement")]
        public async Task<IActionResult> UpsertEndUserAgreement([FromBody] UpsertEndUserAgreementRequest request)
        {
            var eua = await _goCardlessService.GetEndUserAgreement();
            EndUserAgreement? validEua = _repository.HasValidNonAcceptedAgreement(eua);

            string institutionId;
            if (validEua == null)
            {
                if (request.InstitutionId != null && request.DaysOfAccess != null)
                {
                    institutionId = request.InstitutionId;
                    validEua = await _goCardlessService.CreateEndUserAgreement(institutionId: institutionId, daysOfAccess: request.DaysOfAccess.Value);
                }
                else
                {
                    return BadRequest("InstitutionId is required to create a new agreement");
                }
            }
            else
            {
                institutionId = validEua.InstitutionId;
            }

            // Fetch and store institution data
            var gcInstitution = await _goCardlessService.GetInstitutionAsync(institutionId);
            if (gcInstitution != null)
            {
                var existing = await _dbContext.Institutions.FindAsync(gcInstitution.Id);
                if (existing == null)
                {
                    _dbContext.Institutions.Add(new Institution
                    {
                        Id = gcInstitution.Id,
                        Name = gcInstitution.Name,
                        LogoUrl = gcInstitution.Logo
                    });
                }
                else
                {
                    existing.Name = gcInstitution.Name;
                    existing.LogoUrl = gcInstitution.Logo;
                    existing.UpdatedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
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

        [AllowAnonymous]
        [HttpGet("institutions")]
        public async Task<IActionResult> GetInstitutions([FromQuery] string? country = null)
        {
            var institutions = await _goCardlessService.GetInstitutionsAsync(country);
            return Ok(institutions);
        }

        [HttpPost("institutions/sync-logos")]
        public async Task<IActionResult> SyncMissingLogos()
        {
            var missing = await _dbContext.Institutions
                .Where(i => i.LogoUrl == null || i.LogoUrl == "")
                .ToListAsync();

            var updated = 0;
            foreach (var institution in missing)
            {
                var gcInstitution = await _goCardlessService.GetInstitutionAsync(institution.Id);
                if (gcInstitution?.Logo != null)
                {
                    institution.LogoUrl = gcInstitution.Logo;
                    institution.UpdatedAt = DateTime.UtcNow;
                    updated++;
                }
            }

            if (updated > 0)
                await _dbContext.SaveChangesAsync();

            return Ok(new { total = missing.Count, updated });
        }
    }
}
