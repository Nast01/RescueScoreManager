using System.Net.Http;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IWSIRestService
{
    public Task<bool> RequestToken(string login, string password);
    public ApiToken? GetToken();
    public bool HasToken();
}
