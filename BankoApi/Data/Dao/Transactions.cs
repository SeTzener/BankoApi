using System.Text.Json.Serialization;

namespace BankoApi.Data.Dao;

public class Transactions
{
    public int Id { get; set; }
    [JsonPropertyName("transactions")] public required BankTransactions BankTransactions { get; set; }
}

public class BankTransactions
{
    public int Id { get; set; }
    public required List<Booked> Booked { get; set; }
    public required List<Pending> Pending { get; set; }
}

public class Booked
{
    public int Id { get; set; }
    public required string TransactionId { get; set; }
    public required string BookingDate { get; set; }
    public required string ValueDate { get; set; }
    public required TransactionAmount TransactionAmount { get; set; }
    public DebtorAccount? DebtorAccount { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
    public required string BankTransactionCode { get; set; }
    public required string InternalTransactionId { get; set; }
    public string? CreditorName { get; set; }
    public CreditorAccount? CreditorAccount { get; set; }
    public string? DebtorName { get; set; }
    public List<string>? RemittanceInformationStructuredArray { get; set; }
}

public class TransactionAmount
{
    public int Id { get; set; }
    public required string Amount { get; set; }
    public required string Currency { get; set; }
}

public class DebtorAccount
{
    public int Id { get; set; }
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}

public class CreditorAccount
{
    public int Id { get; set; }
    public required string Iban { get; set; }
    public required string Bban { get; set; }
}

public class Pending
{
    public int Id { get; set; }
    public required string BookingDate { get; set; }
    public required TransactionAmount TransactionAmount { get; set; }
    public required string RemittanceInformationUnstructured { get; set; }
    public required List<string> RemittanceInformationUnstructuredArray { get; set; }
}