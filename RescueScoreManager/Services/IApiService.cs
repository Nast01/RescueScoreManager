using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IApiService
{
    public Task<TokenResponse> RequestTokenAsync(string login, string password);
    public Task<UserInfo> GetUserInfoAsync(string token);
    //public Task<bool> RequestToken(string login, string password);
    //public ApiToken? GetToken();
    //public bool HasToken();
    public bool GetIsLoaded();
    public List<Category> GetCategories();
    public List<Licensee> GetLicensees();
    public List<Club> GetClubs();
    public List<Race> GetRaces();
    public List<Team> GetTeams();
    public Competition GetCompetition();
    public void SetCompetition(Competition competition);
    public Task<List<Competition>> GetCompetitions(DateTime startDate,AuthenticationInfo authenticationInfo);
    public Task Load(Competition competition,AuthenticationInfo authenticationInfo);
}
