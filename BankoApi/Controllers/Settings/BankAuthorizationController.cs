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
                var result = await _dbContext.BankAuthorizations
                    .Where(ba => ba.UserId == userId)
                    .AsNoTracking()
                    .Include(ba => ba.BankAccounts)
                    .Include(ba => ba.Institution)
                    .ToListAsync();

                if (result.Count == 0) throw new NoBankAuthorizationFoundException($"The user {userId} doesn't have any bank authorization process started yet.");

                var missingLogos = result
                    .Where(ba => ba.Institution != null && string.IsNullOrEmpty(ba.Institution.LogoUrl) && ba.InstitutionId != null)
                    .Select(ba => ba.Institution!)
                    .Distinct()
                    .ToList();

                foreach (var institution in missingLogos)
                {
                    try
                    {
                        var gcInstitution = await _goCardlessService.GetInstitutionAsync(institution.Id);
                        if (gcInstitution?.Logo != null)
                        {
                            institution.LogoUrl = gcInstitution.Logo;
                            institution.UpdatedAt = DateTime.UtcNow;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to fetch logo for institution {InstitutionId}", institution.Id);
                    }
                }

                if (missingLogos.Count > 0)
                    await _dbContext.SaveChangesAsync();

                var now = DateTime.UtcNow;
                return Ok(new GetBankAuthorizationsResponse
                {
                    BankAuthorizations = result.ConvertAll(it =>
                    {
                        var state = GetAuthorizationState(it, now);
                        return new BankAuth()
                        {
                            Id = it.Id,
                            UserId = it.UserId,
                            RequisitionId = it.RequisitionId,
                            InstitutionId = it.InstitutionId,
                            ReferenceId = it.ReferenceId,
                            AgreementId = it.AgreementId,
                            Status = it.Status,
                            InstitutionName = it.InstitutionName,
                            InstitutionLogoUrl = it.Institution?.LogoUrl,
                            CreatedAt = it.CreatedAt,
                            UpdatedAt = it.UpdatedAt,
                            ExpiresAt = it.AgreementExpiresAt,
                            DaysUntilExpiry = it.AgreementExpiresAt.HasValue
                                ? (int)Math.Ceiling((it.AgreementExpiresAt.Value - now).TotalDays)
                                : null,
                            AuthorizationState = state,
                            RequiresReauthorization = state is BankAuthorizationState.ExpiringSoon
                                or BankAuthorizationState.Expired
                                or BankAuthorizationState.Error,
                            Accounts = it.BankAccounts?.Where(acc => acc.IsActive).ToList().ConvertAll(acc => new BankAccountSummary()
                            {
                                BankAccountId = acc.BankAccountId,
                                Iban = acc.Iban,
                                Currency = acc.Currency,
                                OwnerName = acc.OwnerName,
                                AccountName = acc.AccountName,
                                Product = acc.Product
                            }) ?? new List<BankAccountSummary>()
                        };
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
            var userId = User.GetUserId();

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
                authorization.UserId = userId;
                _dbContext.BankAuthorizations.Add(authorization);
            }
            else
            {
                if (authorization.UserId != userId) return Forbid();
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
            if (request.InstitutionId == null || request.DaysOfAccess == null)
            {
                return BadRequest("InstitutionId and DaysOfAccess are required");
            }

            var validEua = await _goCardlessService.CreateEndUserAgreement(
                institutionId: request.InstitutionId,
                daysOfAccess: request.DaysOfAccess.Value);

            var institutionId = validEua.InstitutionId;

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

            var userId = User.GetUserId();
            var authorization = await _dbContext.BankAuthorizations
                .FirstOrDefaultAsync(ba => ba.UserId == userId && ba.InstitutionId == institutionId)
                ?? new BankAuthorization { UserId = userId };

            // NOTE: A renewal attempt overwrites the active agreement/requisition before the user
            // completes the bank SCA flow. If the user abandons that flow, the previous agreement's
            // expiry is lost and this row stays Processing. This is a known limitation: if it becomes
            // a real problem, track renewal attempts in separate Pending* fields and promote them to
            // the active agreement only after the callback succeeds.
            authorization.RequisitionId = requisition.Id;
            authorization.AgreementId = requisition.Agreement;
            authorization.ReferenceId = requisition.Reference;
            authorization.InstitutionId = institutionId;
            authorization.InstitutionName = gcInstitution?.Name;
            authorization.Status = BankAuthorizationStaus.Processing;
            ApplyAgreementLifecycle(authorization, validEua);
            authorization.LastStatusCheckAt = DateTime.UtcNow;

            if (authorization.Id == Guid.Empty)
                _dbContext.BankAuthorizations.Add(authorization);
            else
                authorization.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            return Ok(new UpsertEndUserAgreementResponse()
            {
                AgreementId = requisition.Agreement,
                Link = requisition.Link,
                InstitutionId = requisition.InstitutionId,
                RequisitionId = requisition.Id,
                ReferenceId = requisition.Reference,
                BankAuthorizationId = authorization.Id
            });
        }

        [HttpPost("bank-auth-callback")]
        public async Task<IActionResult> BankAuthCallback([FromBody] BankAuthCallbackRequest request)
        {
            var userId = User.GetUserId();
            var authorization = await _dbContext.BankAuthorizations
                .Include(ba => ba.BankAccounts)
                .FirstOrDefaultAsync(ba => ba.RequisitionId == request.RequisitionId && ba.UserId == userId);

            if (authorization == null)
            {
                return NotFound(new ErrorResponse
                {
                    Message = BankAuthorizationErrorMessages.NoAuthorizationFound.ToString()
                });
            }

            // The callback can be replayed (e.g. after a lost response), so make it idempotent.
            // Every renewal sets the row back to Processing, so Linked here means this
            // requisition was already fully processed.
            if (authorization.Status == BankAuthorizationStaus.Linked)
            {
                // A replay may be the first chance to record the accepted EUA (e.g. the
                // original response was lost after the bank accepted it).
                if (authorization.AgreementAcceptedAt == null)
                {
                    await RefreshAgreementLifecycleAsync(authorization);
                    await _dbContext.SaveChangesAsync();
                }

                return Ok(BuildCallbackResponse(authorization));
            }

            var requisition = await _goCardlessService.GetRequisitionAsync(request.RequisitionId);

            if (requisition.Status != "LN")
            {
                authorization.Status = BankAuthorizationStaus.Error;
                authorization.UpdatedAt = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();

                return BadRequest(new ErrorResponse
                {
                    Message = $"Authorization not completed. Status: {requisition.Status}"
                });
            }

            var existingAccounts = (authorization.BankAccounts ?? new List<BankAccount>())
                .ToDictionary(account => account.BankAccountId, StringComparer.Ordinal);
            var returnedAccountIds = new HashSet<string>(StringComparer.Ordinal);

            foreach (var gcAccountId in requisition.Accounts)
            {
                Guid accountId = Guid.Parse(gcAccountId);

                var details = await _goCardlessService.GetAccountDetailsAsync(accountId);

                // Upsert by (BankAuthorizationId, BankAccountId): re-authorization returns the
                // same GoCardless account IDs, so blindly adding would violate the unique index.
                if (!existingAccounts.TryGetValue(gcAccountId, out var bankAccount))
                {
                    bankAccount = new BankAccount
                    {
                        BankAuthorizationId = authorization.Id,
                        BankAccountId = gcAccountId
                    };
                    _dbContext.BankAccounts.Add(bankAccount);
                    existingAccounts[gcAccountId] = bankAccount;
                }

                bankAccount.Iban = details?.Iban;
                bankAccount.Bban = details?.Bban;
                bankAccount.Currency = details?.Currency;
                bankAccount.OwnerName = details?.OwnerName;
                bankAccount.Product = details?.Product;
                bankAccount.AccountName = details?.DisplayName ?? details?.Name;
                bankAccount.IsActive = true;
                bankAccount.UpdatedAt = DateTime.UtcNow;

                returnedAccountIds.Add(gcAccountId);

                try
                {
                    var transactions = await _goCardlessService.GetTransactionsAsync(accountId);
                    if (transactions != null)
                    {
                        await _transactionsRepository.StoreTransactions(
                            ctx: _dbContext,
                            userId: userId,
                            bankAccountId: accountId,
                            transactions: transactions);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to fetch initial transactions for account {AccountId}", gcAccountId);
                }
            }

            // Accounts the bank no longer returns are kept for history but stop being fetched.
            foreach (var removedAccount in existingAccounts.Values.Where(account => !returnedAccountIds.Contains(account.BankAccountId)))
            {
                removedAccount.IsActive = false;
                removedAccount.UpdatedAt = DateTime.UtcNow;
            }

            // The bank has accepted the agreement by now, so this is the first reliable
            // point to record the accepted date and the expiry used for renewal warnings.
            await RefreshAgreementLifecycleAsync(authorization);

            authorization.Status = BankAuthorizationStaus.Linked;
            authorization.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            return Ok(BuildCallbackResponse(authorization));
        }

        private const int ReauthorizationWarningDays = 7;

        private static BankAuthorizationState GetAuthorizationState(BankAuthorization authorization, DateTime now)
        {
            if (authorization.Status == BankAuthorizationStaus.Error) return BankAuthorizationState.Error;
            if (authorization.Status == BankAuthorizationStaus.Expired) return BankAuthorizationState.Expired;

            // No known expiry yet (e.g. still processing its first link): nothing to renew.
            if (!authorization.AgreementExpiresAt.HasValue) return BankAuthorizationState.Valid;

            if (authorization.AgreementExpiresAt.Value <= now) return BankAuthorizationState.Expired;
            if (authorization.AgreementExpiresAt.Value <= now.AddDays(ReauthorizationWarningDays))
                return BankAuthorizationState.ExpiringSoon;

            return BankAuthorizationState.Valid;
        }

        private static void ApplyAgreementLifecycle(BankAuthorization authorization, EndUserAgreement agreement)
        {
            authorization.AgreementCreatedAt = agreement.Created;
            authorization.AgreementAcceptedAt = agreement.Accepted;
            authorization.AccessValidForDays = agreement.AccessValidForDays;

            // GoCardless measures access_valid_for_days from the agreement creation date.
            authorization.AgreementExpiresAt = agreement.Created.AddDays(agreement.AccessValidForDays);
        }

        /// <summary>
        /// Re-reads the agreement from GoCardless and stores its accepted/expiry dates.
        /// Failures are logged instead of thrown: linking a requisition must not fail
        /// because a secondary status lookup did.
        /// </summary>
        private async Task RefreshAgreementLifecycleAsync(BankAuthorization authorization)
        {
            authorization.LastStatusCheckAt = DateTime.UtcNow;

            if (string.IsNullOrEmpty(authorization.AgreementId)) return;

            try
            {
                var agreement = await _goCardlessService.GetEndUserAgreementAsync(authorization.AgreementId);
                if (agreement != null)
                {
                    ApplyAgreementLifecycle(authorization, agreement);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to refresh EUA lifecycle for agreement {AgreementId}", authorization.AgreementId);
            }
        }

        private static BankAuthCallbackResponse BuildCallbackResponse(BankAuthorization authorization)
        {
            return new BankAuthCallbackResponse
            {
                BankAuthorizationId = authorization.Id,
                Status = authorization.Status,
                LinkedAccounts = (authorization.BankAccounts ?? new List<BankAccount>())
                    .Where(account => account.IsActive)
                    .Select(account => new LinkedBankAccount
                    {
                        BankAccountId = account.BankAccountId,
                        Iban = account.Iban,
                        Currency = account.Currency,
                        OwnerName = account.OwnerName,
                        AccountName = account.AccountName,
                        Product = account.Product
                    })
                    .ToList()
            };
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
