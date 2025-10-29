using System.Text.Json.Serialization;

namespace BankoApi.Data.Dao;

public class Transaction
{
    public required string Id { get; set; }
    public Guid UserId { get; set; }
    public Guid BankAccountId { get; set; }
    public required DateTime BookingDate { get; set; }
    public required DateTime ValueDate { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public virtual DebtorAccount? DebtorAccount { get; set; }

    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
    public string? BankTransactionCode { get; set; }
    public required string InternalTransactionId { get; set; }
    public string? CreditorName { get; set; }
    public virtual CreditorAccount? CreditorAccount { get; set; }
    public string? DebtorName { get; set; }
    public List<string>? RemittanceInformationStructuredArray { get; set; }
    public string ExpenseTagId { get; set; }
    public string? Note { get; set; }
    public virtual ExpenseTag? ExpenseTag { get; set; }
}

public class CreditorAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}

public class Pending
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required DateTime BookingDate { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
}