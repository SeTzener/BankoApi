namespace BankoApi;

public class Utils
{
    public static String SelectDatabase(WebApplicationBuilder builder)
    {
        string environment =
#if DEBUG
            "Development";
#else
            "Production";
#endif
        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            // Use Prod database connection string from configuration
            return builder.Configuration["GoogleCloud:Prod"] ??
                   throw new Exception("Missing environment variable: GoogleCloud:Prod");
        }
        else
        {
            // Use Dev database connection string (can be from configuration or a default)
            return builder.Configuration["GoogleCloud:DevDb"] ??
                   throw new Exception("Missing environment variable: GoogleCloud:DevDb");
        }
    }
}