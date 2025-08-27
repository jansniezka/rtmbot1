namespace RealTimeMediaBot.Services;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync();
}
