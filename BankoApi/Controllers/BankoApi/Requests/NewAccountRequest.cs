namespace BankoApi.Controllers.BankoApi.Requests;

public class NewAccountRequest
{
    public String Email { get; set; }
    public String Password { get; set; }
    public String? FullName { get; set; }
    public String? Address { get; set; }
    public String? PhoneNumber { get; set; }
    public Boolean ConsentGiven { get; set; }
}