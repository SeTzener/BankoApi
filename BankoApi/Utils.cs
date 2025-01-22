namespace BankoApi;

public class Utils
{
    public static string SelectDatabase(WebApplicationBuilder builder)
    {
        var environment =
#if DEBUG
            "Development";
#else
            "Production";
#endif
        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
            // Use Prod database connection string from configuration
            return builder.Configuration["GoogleCloud:Prod"] ??
                   throw new Exception("Missing environment variable: GoogleCloud:Prod");

        // Use Dev database connection string (can be from configuration or a default)
        return builder.Configuration["GoogleCloud:DevDb"] ??
               throw new Exception("Missing environment variable: GoogleCloud:DevDb");
    }
}