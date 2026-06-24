using System.ComponentModel.DataAnnotations;

namespace BankoApi.Controllers.Users.Requests;

public class AcceptConsentRequest
{
    [Required] public Guid PolicyVersionId { get; set; }
}
