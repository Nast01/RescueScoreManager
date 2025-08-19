using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Security.Claims;
using System.Xml.Linq;

using CommunityToolkit.Mvvm.Messaging;

using Microsoft.Extensions.Logging;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;
using RescueScoreManager.Helpers;
using RescueScoreManager.Properties;

using static System.Reflection.Metadata.BlobBuilder;

namespace RescueScoreManager.Services;

public class XMLService : IXMLService
{
    private readonly ILogger<XMLService> _logger;

    // Internal storage using List<T> for better performance
    private Competition? _competition;
    private readonly List<Category> _categories = new();
    private readonly List<Club> _clubs = new();
    private readonly List<Licensee> _licensees = new();
    private readonly List<Athlete> _athletes = new();
    private readonly List<Referee> _referees = new();
    private readonly List<Race> _races = new();
    private readonly List<Team> _teams = new();
    private readonly List<RaceFormatConfiguration> _raceFormatConfigurations = new();
    private AppSetting _setting = new();

    public bool IsLoaded { get; private set; }

    public XMLService(ILogger<XMLService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #region Data Access Methods (Public API returns IReadOnlyList)
    public Competition? GetCompetition() => _competition;

    public IReadOnlyList<Category> GetCategories()
    {
        var sortedCategories = _categories.ToList();
        sortedCategories.Sort(new CategoryComparer());
        return sortedCategories.AsReadOnly();
    }

    public IReadOnlyList<Club> GetClubs()
    {
        var sortedClubs = _clubs.ToList();
        sortedClubs.Sort(new ClubNameComparer());
        return sortedClubs.AsReadOnly();
    }

    public IReadOnlyList<Licensee> GetLicensees()
    {
        var sortedLicensees = _licensees.ToList();
        sortedLicensees.Sort(new LicenseeClubFullNameComparer());
        return sortedLicensees.AsReadOnly();
    }

    public IReadOnlyList<Athlete> GetAthletes() => _athletes.AsReadOnly();
    public IReadOnlyList<Referee> GetReferees() => _referees.AsReadOnly();
    public IReadOnlyList<Race> GetRaces() => _races.AsReadOnly();
    public IReadOnlyList<Team> GetTeams() => _teams.AsReadOnly();
    public IReadOnlyList<RaceFormatConfiguration> GetRaceFormatConfigurations() => _raceFormatConfigurations.AsReadOnly();
    public AppSetting? GetSetting() => _setting;
    #endregion

    #region Path Management
    public void SetPath(string name)
    {
        try
        {
            string cleanedName = TextHelper.CleanedText(name);
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string filePath = Path.Combine(documentsPath, "RescueScore", cleanedName, cleanedName + ".ffss");
            var fileInfo = new FileInfo(filePath);

            Properties.Settings.Default.FilePath = fileInfo.FullName;
            Properties.Settings.Default.DirPath = fileInfo.DirectoryName;
            Properties.Settings.Default.Save();

            _logger.LogInformation("XML path set to: {FilePath}", fileInfo.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting XML path for name: {Name}", name);
            throw;
        }
    }

    public void SetPath(FileInfo file)
    {
        if (file == null)
        {
            throw new ArgumentNullException(nameof(file));
        }

        try
        {
            Properties.Settings.Default.FilePath = file.FullName;
            Properties.Settings.Default.DirPath = file.DirectoryName;
            Properties.Settings.Default.Save();

            _logger.LogInformation("XML path set to: {FilePath}", file.FullName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting XML path for file: {FileName}", file.FullName);
            throw;
        }
    }

    public string GetFilePath() => Properties.Settings.Default.FilePath ?? string.Empty;
    public string GetDirPath() => Properties.Settings.Default.DirPath ?? string.Empty;
    #endregion

    #region Data Initialization
    public void Initialize(Competition competition, IEnumerable<Category> categories, IEnumerable<Club> clubs,
                          IEnumerable<Licensee> licensees, IEnumerable<Race> races, IEnumerable<Team> teams, IEnumerable<RaceFormatConfiguration> raceFormatConfigurations)
    {
        if (competition == null)
        {
            throw new ArgumentNullException(nameof(competition));
        }

        try
        {
            _logger.LogInformation("Initializing XML service with competition: {CompetitionName}", competition.Name);

            // Reset existing data
            Reset();

            // Set competition
            _competition = competition;

            // Initialize collections
            if (categories != null)
            {
                _categories.AddRange(categories);
            }

            if (clubs != null)
            {
                _clubs.AddRange(clubs);
                _clubs.Sort(new ClubNameComparer());
            }

            if (licensees != null)
            {
                _licensees.AddRange(licensees);
                _licensees.Sort(new LicenseeClubFullNameComparer());

                // Separate athletes and referees with order numbers
                int orderNumber = 1;
                foreach (var licensee in _licensees)
                {
                    switch (licensee)
                    {
                        case Athlete athlete:
                            athlete.OrderNumber = orderNumber++;
                            _athletes.Add(athlete);
                            break;
                        case Referee referee:
                            _referees.Add(referee);
                            break;
                    }
                }
            }

            if (races != null)
            {
                _races.AddRange(races);
            }

            if (teams != null)
            {
                _teams.AddRange(teams);
            }

            if(raceFormatConfigurations != null)
            {
                _raceFormatConfigurations.AddRange(raceFormatConfigurations);
            }

                _logger.LogInformation("XML service initialized successfully with {CategoriesCount} categories, {ClubsCount} clubs, {AthletesCount} athletes, {RefereesCount} referees, {RacesCount} races, {TeamsCount} teams, {RaceFormatConfiguration} raceFormatConfiguration",
                _categories.Count, _clubs.Count, _athletes.Count, _referees.Count, _races.Count, _teams.Count,_raceFormatConfigurations.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error initializing XML service");
            throw;
        }
    }

    public void Reset()
    {
        _logger.LogDebug("Resetting XML service data");

        _competition = null;
        _categories.Clear();
        _clubs.Clear();
        _licensees.Clear();
        _athletes.Clear();
        _referees.Clear();
        _races.Clear();
        _teams.Clear();
        _raceFormatConfigurations.Clear();
        IsLoaded = false;
    }
    #endregion

    #region File Operations
    public void Load()
    {
        string filePath = GetFilePath();
        if (string.IsNullOrEmpty(filePath))
        {
            throw new InvalidOperationException("No file path set. Call SetPath first.");
        }

        LoadFromFile(filePath);
    }

    public void LoadFromFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"XML file not found: {filePath}");
        }

        try
        {
            _logger.LogInformation("Loading XML from file: {FilePath}", filePath);

            // Reset existing data
            Reset();

            // Load XML document
            var xDoc = XDocument.Load(filePath);
            var rootElement = xDoc.Element(Properties.Resources.Root_XMI);

            if (rootElement == null)
            {
                throw new InvalidOperationException("Invalid XML format: Root element not found");
            }

            // Load competition
            LoadCompetition(rootElement);

            // Load categories
            LoadCategories(rootElement);

            // Load clubs
            LoadClubs(rootElement);

            // Load licensees (athletes and referees)
            LoadLicensees(rootElement);

            // Load races
            LoadRaces(rootElement);

            // Load teams
            LoadTeams(rootElement);

            // Load race format configurations and details
            LoadRaceFormatConfigurations(rootElement);

            // Load settings
            LoadSetting(rootElement);

            IsLoaded = true;
            _logger.LogInformation("XML loaded successfully from: {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading XML from file: {FilePath}", filePath);
            Reset();
            throw;
        }
    }

    public bool Save()
    {
        string xmlFile = GetFilePath();
        if (string.IsNullOrEmpty(xmlFile))
        {
            _logger.LogError("No file path set for saving");
            return false;
        }

        if (_competition == null)
        {
            _logger.LogError("No competition data to save");
            return false;
        }

        try
        {
            _logger.LogInformation("Saving XML to file: {FilePath}", xmlFile);

            var xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "true"));
            var rootElement = new XElement(Properties.Resources.Root_XMI);

            // Save competition
            var competitionElement = _competition.WriteXml();
            if (competitionElement != null)
            {
                rootElement.Add(competitionElement);
            }
            // Save settings
            var settingElement = _setting.WriteXml();
            if (settingElement != null)
            {
                rootElement.Add(settingElement);
            }

            // Save categories
            var categoriesElement = new XElement(Properties.Resources.Categories_XMI);
            foreach (var category in _categories)
            {
                var categoryElement = category.WriteXml();
                if (categoryElement != null)
                {
                    categoriesElement.Add(categoryElement);
                }
            }
            rootElement.Add(categoriesElement);

            // Save clubs & licensees
            var clubsElement = new XElement(Properties.Resources.Clubs_XMI);
            foreach (var club in _clubs)
            {
                var clubElement = club.WriteXml();
                if (clubElement != null)
                {
                    clubsElement.Add(clubElement);
                }
            }
            rootElement.Add(clubsElement);

            // Save races & teams
            var racesElement = new XElement(Properties.Resources.Races_XMI);
            foreach (var race in _races)
            {
                var raceElement = race.WriteXml();
                if (raceElement != null)
                {
                    racesElement.Add(raceElement);
                }
            }
            rootElement.Add(racesElement);

            // Save race format configuration & race format details
            var racesFormatConfigurationElement = new XElement(Properties.Resources.RaceFormatConfigurations_XMI);
            foreach (var raceFormatConfiguration in _raceFormatConfigurations)
            {
                var raceFormatConfigurationElement = raceFormatConfiguration.WriteXml();
                if (raceFormatConfigurationElement != null)
                {
                    racesFormatConfigurationElement.Add(raceFormatConfigurationElement);
                }
            }
            rootElement.Add(racesFormatConfigurationElement);


            xDoc.Add(rootElement);

            // Ensure directory exists
            var fileInfo = new FileInfo(xmlFile);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            xDoc.Save(fileInfo.FullName);
            _logger.LogInformation("XML saved successfully to: {FilePath}", xmlFile);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving XML to file: {FilePath}", xmlFile);
            return false;
        }
    }

    bool IXMLService.IsLoaded() => IsLoaded;
    #endregion

    #region Private Loading Methods
    private void LoadCompetition(XElement rootElement)
    {
        var competitionElement = rootElement.Element(Properties.Resources.Competition_XMI);
        if (competitionElement != null)
        {
            _competition = new Competition(xElement: competitionElement);
            _logger.LogDebug("Competition loaded: {CompetitionName}", _competition.Name);
        }
    }

    private void LoadCategories(XElement rootElement)
    {
        var categoryElements = rootElement.Descendants(Properties.Resources.Category_XMI);
        foreach (var categoryElement in categoryElements)
        {
            try
            {
                var category = new Category(categoryElement);
                if (!_categories.Contains(category))
                {
                    _categories.Add(category);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load category from XML element");
            }
        }

        _categories.Sort(new CategoryComparer());
        _logger.LogDebug("Loaded {Count} categories", _categories.Count);
    }

    private void LoadClubs(XElement rootElement)
    {
        var clubElements = rootElement.Descendants(Properties.Resources.Club_XMI);
        foreach (var clubElement in clubElements)
        {
            try
            {
                var club = new Club(clubElement);
                if (!_clubs.Contains(club))
                {
                    _clubs.Add(club);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load club from XML element");
            }
        }

        _clubs.Sort(new ClubNameComparer());
        _logger.LogDebug("Loaded {Count} clubs", _clubs.Count);
    }

    private void LoadLicensees(XElement rootElement)
    {
        var athleteElements = rootElement.Descendants(Properties.Resources.Athlete_XMI);
        foreach (XElement athleteElement in athleteElements)
        {
            try
            {
                Athlete athlete = new Athlete(athleteElement);

                if (!_licensees.Contains(athlete))
                {
                    int clubId = int.Parse(athleteElement.Parent.Attribute(Properties.Resources.Id_XMI).Value);
                    Club club = GetClubs().ToList().Find(c => c.Id == clubId);
                    if (club != null)
                    {
                        club.AddLicensee(athlete);
                        athlete.Club = club;
                        athlete.ClubId = clubId;
                        _licensees.Add(athlete);
                        _athletes.Add(athlete);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load athletes from XML element");
            }
        }

        var refereeElements = rootElement.Descendants(Properties.Resources.Referee_XMI);
        foreach (XElement refereeElement in refereeElements)
        {
            try
            {
                Referee referee = new Referee(refereeElement);

                if (!_licensees.Contains(referee))
                {
                    int clubId = int.Parse(refereeElement.Parent.Attribute(Properties.Resources.Id_XMI).Value);
                    Club? club = GetClubs().ToList().Find(c => c.Id == clubId);
                    if (club != null)
                    {
                        club.AddLicensee(referee);
                        referee.Club = club;
                        referee.ClubId = clubId;
                        _licensees.Add(referee);
                        _referees.Add(referee);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load referees from XML element");
            }
        }

        _licensees.Sort(new LicenseeClubFullNameComparer());
        _logger.LogDebug("Loaded {LicenseeCount} licensees ({AthleteCount} athletes, {RefereeCount} referees)",
            _licensees.Count, _athletes.Count, _referees.Count);
    }

    private void LoadRaces(XElement rootElement)
    {
        var raceElements = rootElement.Descendants(Properties.Resources.Race_XMI);
        foreach (var raceElement in raceElements)
        {
            try
            {
                var race = new Race(raceElement, _categories);
                _races.Add(race);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load race from XML element");
            }
        }

        _logger.LogDebug("Loaded {Count} races", _races.Count);
    }

    private void LoadTeams(XElement rootElement)
    {
        var teamElements = rootElement.Descendants(Properties.Resources.IndividualTeam_XMI);
        foreach (var teamElement in teamElements)
        {
            try
            {
                var team = new IndividualTeam(teamElement,_athletes,_races,_categories);
                _teams.Add(team);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load team from XML element");
            }
        }
        teamElements = null;
        teamElements = rootElement.Descendants(Properties.Resources.RelayTeam_XMI);
        foreach (var teamElement in teamElements)
        {
            try
            {
                var team = new RelayTeam(teamElement, _athletes,_races,_categories);
                _teams.Add(team);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load team from XML element");
            }
        }

        _logger.LogDebug("Loaded {Count} teams", _teams.Count);
    }

    private void LoadRaceFormatConfigurations(XElement rootElement)
    {
        var raceFormatConfigurationElements = rootElement.Descendants(Properties.Resources.RaceFormatConfiguration_XMI);
        foreach (var raceFormatConfigurationElement in raceFormatConfigurationElements)
        {
            try
            {
                var raceFormatConfiguration = new RaceFormatConfiguration(raceFormatConfigurationElement,_categories);

                var raceFormatDetailElements = raceFormatConfigurationElement.Descendants(Properties.Resources.RaceFormatDetail_XMI);

                raceFormatConfiguration.RaceFormatDetails.Clear();
                foreach (var raceFormatDetailElement in raceFormatDetailElements)
                {
                    var raceFormatDetail = new RaceFormatDetail(raceFormatDetailElement);

                    raceFormatDetail.RaceFormatConfiguration = raceFormatConfiguration;
                    raceFormatConfiguration.RaceFormatDetails.Add(raceFormatDetail);
                }

                    _raceFormatConfigurations.Add(raceFormatConfiguration);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to load RaceFormatConfiguration from XML element");
            }
        }
        

        _logger.LogDebug("Loaded {Count} teams", _teams.Count);
    }
    private void LoadSetting(XElement rootElement)
    {
        var settingElement = rootElement.Element(Properties.Resources.AppSetting_XMI);
        if (settingElement != null)
        {
            _setting = new AppSetting(settingElement);
            _logger.LogDebug("Setting loaded");
        }
    }

    #endregion

}
