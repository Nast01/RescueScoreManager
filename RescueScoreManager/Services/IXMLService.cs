using System.IO;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public interface IXMLService
{
    // File operations
    bool Save();
    void Load();
    void LoadFromFile(string filePath);
    void SetPath(string name);
    void SetPath(FileInfo file);
    string GetFilePath();
    string GetDirPath();
    bool IsLoaded();

    // Data management
    void Initialize(Competition competition, IEnumerable<Category> categories, IEnumerable<Club> clubs,
                   IEnumerable<Licensee> licensees, IEnumerable<Race> races, IEnumerable<Team> teams, IEnumerable<RaceFormatConfiguration> raceFormatConfigurations);
    void Reset();

    // Data access - using IReadOnlyList to fix CA1002
    Competition? GetCompetition();
    IReadOnlyList<Category> GetCategories();
    IReadOnlyList<Club> GetClubs();
    IReadOnlyList<Licensee> GetLicensees();
    IReadOnlyList<Athlete> GetAthletes();
    IReadOnlyList<Referee> GetReferees();
    IReadOnlyList<Race> GetRaces();
    IReadOnlyList<Team> GetTeams();
    IReadOnlyList<RaceFormatConfiguration> GetRaceFormatConfigurations();
    IReadOnlyList<ProgramMeeting> GetProgramMeetings();
    AppSetting? GetSetting();

    // Update methods
    void UpdateRaceFormatConfigurations(IEnumerable<RaceFormatConfiguration> raceFormatConfigurations);
}
