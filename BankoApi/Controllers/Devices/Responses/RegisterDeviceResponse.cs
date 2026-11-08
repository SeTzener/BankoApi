using BankoApi.Data.Dao;

namespace BankoApi.Controllers.Devices.Responses;

public class RegisterDeviceResponse
{
    public Guid Id { get; set; }
    public PushPlatform Platform { get; set; }
    public bool IsActive { get; set; }
}
