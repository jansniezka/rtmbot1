namespace RealTimeMediaBot.Services;

public interface IAuthenticationService
{
    Task<string> GetAccessTokenAsync(bool forceRefresh = false);
    Task<bool> HasValidTokenAsync();
}
