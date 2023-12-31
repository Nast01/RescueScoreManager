using System.IO;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IXMLService
{
    public bool Save();
    public void Load();
    public void SetPath(string name);
    public void SetPath(FileInfo file);
    public string GetFilePath();
    public string GetDirPath();
    public bool IsLoaded();
    public void Initialize(Competition competition, List<Category> categories, List<Club> Clubs, List<Licensee> licensees, List<Race> races, List<Team> teams);
    public Competition GetCompetition();
    public List<Category> GetCategories();
    public List<Club> GetClubs();
    public List<Licensee> GetLicensees();
    public List<Athlete> GetAthletes();
    public List<Referee> GetReferees();
    public List<Race> GetRaces();
    public List<Team> GetTeams();
}
