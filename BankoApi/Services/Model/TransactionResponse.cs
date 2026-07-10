using System.Text.Json.Serialization;

namespace BankoApi.Services.Model;

public class TransactionResponse
{
    public required string Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BankAccountId { get; set; }
    public required DateTime BookingDate { get; set; }
    public required DateTime ValueDate { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BankoApi.Data.Dao.DebtorAccount? DebtorAccount { get; set; }

    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
    public string? BankTransactionCode { get; set; }
    public required string InternalTransactionId { get; set; }
    public string? CreditorName { get; set; }
    public BankoApi.Data.Dao.CreditorAccount? CreditorAccount { get; set; }
    public string? DebtorName { get; set; }
    public List<string>? RemittanceInformationStructuredArray { get; set; }
    public string? ExpenseTagId { get; set; }
    public string? Note { get; set; }
    public bool isDeleted { get; set; }
    public BankoApi.Data.Dao.ExpenseTag? ExpenseTag { get; set; }

    // Bank info
    public string? BankName { get; set; }
    public string? BankLogoUrl { get; set; }
}
