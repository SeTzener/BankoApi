using BankoApi.Services.Model;

namespace BankoApi.Tests.Models;

public class DtoAndModelTests
{
    [Fact]
    public void DebtorAccount_NullValuesUseDefaults()
    {
        var account = new BankoApi.Services.Model.DebtorAccount
        {
            Iban = null,
            Bban = null
        };
        Assert.Equal("XX0000000000000", account.Iban);
        Assert.Equal("00000000000", account.Bban);
    }
}
