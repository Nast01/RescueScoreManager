using System.ComponentModel;
using System.Net.Http;

using RescueScoreManager.Data;

using static System.Reflection.Metadata.BlobBuilder;

namespace RescueScoreManager.Services;

public interface IWSIRestService
{
    public Task<bool> RequestToken(string login, string password);
    public ApiToken? GetToken();
    public bool HasToken();
    public bool GetIsLoaded();
    public List<Category> GetCategories();
    public List<Licensee> GetLicensees();
    public List<Club> GetClubs();
    public List<Race> GetRaces();
    public List<Team> GetTeams();
    public Competition GetCompetition();
    public void SetCompetition(Competition competition);
    public Task<List<Competition>> GetCompetitions(DateTime startDate);
    public Task Load(Competition competition);
    public Task SetLicenseeNationality(Licensee licensee);

}
