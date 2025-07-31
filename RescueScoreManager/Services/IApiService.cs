using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IApiService
{
    // Authentication methods
    Task<TokenResponse> RequestTokenAsync(string login, string password);
    Task<UserInfo> GetUserInfoAsync(string token);

    // Data loading methods
    Task LoadAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Competition>> GetCompetitionsAsync(DateTime startDate, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default);

    // Data access methods
    bool GetIsLoaded();
    IReadOnlyList<Category> GetCategories();
    IReadOnlyList<Licensee> GetLicensees();
    IReadOnlyList<Athlete> GetAthletes();
    IReadOnlyList<Referee> GetReferees();
    IReadOnlyList<Club> GetClubs();
    IReadOnlyList<Race> GetRaces();
    IReadOnlyList<Team> GetTeams();
    Competition? GetCompetition();
    void SetCompetition(Competition competition);

    // State management
    void Reset();
}
