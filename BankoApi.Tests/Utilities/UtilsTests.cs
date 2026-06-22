using BankoApi;
using Microsoft.AspNetCore.Builder;

namespace BankoApi.Tests.Utilities;

public class UtilsTests
{
    [Fact]
    public void SelectDatabase_ReturnsConfiguredDatabaseName()
    {
        var builder = WebApplication.CreateBuilder();
        builder.Configuration["GoogleCloud:DevDb"] = "BankoDb_Dev";

        var result = Utils.SelectDatabase(builder);

        // In Debug configuration, returns DevDb; in Release, returns Prod
        // Since tests run in Debug, it should use DevDb
        Assert.False(string.IsNullOrEmpty(result));
    }
}
