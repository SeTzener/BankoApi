using BankoApi.Data.Dao;
using Xunit;

namespace BankoApi.Tests.Utilities;

public static class AccountAssertions
{
    public static void AssertEqual(string iban, string bban, CreditorAccount? actual)
        => AssertCore(iban, bban, actual?.Iban, actual?.Bban);

    public static void AssertEqual(string iban, string bban, DebtorAccount? actual)
        => AssertCore(iban, bban, actual?.Iban, actual?.Bban);

    private static void AssertCore(string iban, string bban, string? actualIban, string? actualBban)
    {
        Assert.Equal(iban, actualIban);
        Assert.Equal(bban, actualBban);
    }
}
