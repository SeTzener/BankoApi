namespace BankoApi.Data.Dao;

public class Transaction
{
    public required string Id { get; set; }
    public required string BookingDate { get; set; }
    public required string ValueDate { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }
    public Guid? DebtorAccount { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
    public required string BankTransactionCode { get; set; }
    public required string InternalTransactionId { get; set; }
    public string? CreditorName { get; set; }
    public CreditorAccount? CreditorAccount { get; set; }
    public string? DebtorName { get; set; }
    public List<string>? RemittanceInformationStructuredArray { get; set; }
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
    public required string BookingDate { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
}