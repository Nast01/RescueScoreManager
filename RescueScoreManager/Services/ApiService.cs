using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;

using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;

using Irony.Parsing;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RescueScoreManager.Data;

using static RescueScoreManager.Data.EnumRSM;

namespace RescueScoreManager.Services;

public class ApiService : IApiService, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ApiService> _logger;
    private readonly string _baseAddress;
    private readonly string _version;
    private bool _disposed = false;

    // Data storage
    private bool _isLoaded { get; set; }
    private Competition? _competition { get; set; }
    private List<Category> _categories { get; set; } = new();
    private List<Licensee> _licensees { get; set; } = new();
    private List<Athlete> _athletes { get; set; } = new();
    private List<Referee> _referees { get; set; } = new();
    private List<Club> _clubs { get; set; } = new();
    private List<Race> _races { get; set; } = new();
    private List<Team> _teams { get; set; } = new();
    private List<RaceFormatConfiguration> _raceFormatConfigurations { get; set; } = new();

    public ApiService(ILogger<ApiService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Set base address based on environment
        _baseAddress = Debugger.IsAttached ? "https://qual.ffss.fr/" : "https://ffss.fr/";
        _version = "api/v1.0";

        // Create and configure HttpClient
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseAddress);
        _httpClient.Timeout = TimeSpan.FromSeconds(30);
        _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

        Reset();
    }

    #region Authentication Methods

    public async Task<TokenResponse> RequestTokenAsync(string login, string password)
    {
        if (string.IsNullOrWhiteSpace(login))
            throw new ArgumentException("Login cannot be null or empty", nameof(login));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be null or empty", nameof(password));

        const string endpoint = "/requestToken";
        var queryParameters = new Dictionary<string, string>
        {
            { "login", login },
            { "password", password }
        };

        try
        {
            _logger.LogInformation("Requesting authentication token for user: {Login}", login);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");

            // Log the full URL being constructed for debugging
            string fullUrl = $"{_httpClient.BaseAddress}{endpoint}?{queryString}";
            _logger.LogDebug("Making request to: {FullUrl}", fullUrl);

            using var response = await _httpClient.PostAsync($"{_version}{endpoint}?{queryString}", requestContent);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse == null)
            {
                _logger.LogError("Failed to deserialize token response");
                return new TokenResponse { Success = false, Message = "Invalid response format" };
            }

            var tokenResponse = new TokenResponse
            {
                Success = jResponse["success"]?.Value<bool>() ?? false,
                Message = jResponse["message"]?.Value<string>() ?? "Unknown error",
                Token = jResponse["token"]?.Value<string>() ?? string.Empty
            };

            if (jResponse["expiration"]?.Value<string>() is string expirationStr &&
                DateTime.TryParse(expirationStr, out DateTime expiration))
            {
                tokenResponse.Expiration = expiration;
            }

            _logger.LogInformation("Token request completed successfully: {Success}", tokenResponse.Token);
            return tokenResponse;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during token request for user: {Login}", login);
            return new TokenResponse
            {
                Success = false,
                Message = $"Network error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token request for user: {Login}", login);
            return new TokenResponse
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}"
            };
        }
    }

    public async Task<UserInfo> GetUserInfoAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        }
        const string endpoint = "/me";
        var queryParameters = new Dictionary<string, string>
        {
            { "token", token }
        };

        try
        {
            _logger.LogInformation("Requesting user information");

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            using var response = await _httpClient.GetAsync($"{_version}{endpoint}?{queryString}");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse == null)
            {
                _logger.LogError("Failed to deserialize user info response");
                return new UserInfo { Success = false, Message = "Invalid response format" };
            }

            var userInfo = new UserInfo
            {
                Success = jResponse["success"]?.Value<bool>() ?? false,
                Message = jResponse["message"]?.Value<string>() ?? "Unknown error",
                Label = jResponse["label"]?.Value<string>() ?? string.Empty,
                Type = jResponse["type"]?.Value<string>() ?? string.Empty,
                Role = jResponse["data"]?["role"]?.Value<string>() ?? string.Empty
            };

            _logger.LogInformation("User info request completed successfully: {Success}", userInfo.Success);
            return userInfo;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during user info request");
            return new UserInfo
            {
                Success = false,
                Message = $"Network error: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user info request");
            return new UserInfo
            {
                Success = false,
                Message = $"Unexpected error: {ex.Message}"
            };
        }
    }

    #endregion

    #region Data Loading Methods

    public async Task LoadAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default)
    {
        if (competition == null)
        {
            throw new ArgumentNullException(nameof(competition));
        }
        if (authenticationInfo == null)
        {
            throw new ArgumentNullException(nameof(authenticationInfo));
        }
        try
        {
            _logger.LogInformation("Starting data load for competition: {CompetitionName} (ID: {CompetitionId})",
                competition.Name, competition.Id);

            // Reset previous data
            Reset();
            SetCompetition(competition);

            // Set authentication header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationInfo.Token);

            // Load dependent data

            await LoadCategoriesAsync(competition, authenticationInfo, cancellationToken);
            await LoadClubsAsync(competition, authenticationInfo, cancellationToken);
            await LoadLicenseesAsync(competition, authenticationInfo, cancellationToken);
            await LoadRacesAsync(competition, authenticationInfo, cancellationToken);
            await LoadTeamsAsync(competition, authenticationInfo, cancellationToken);
            await LoadRaceFormatConfigurationsAsync(competition, authenticationInfo, cancellationToken);

            _isLoaded = true;
            _logger.LogInformation("Data load completed successfully for competition: {CompetitionName}", competition.Name);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Data load was cancelled for competition: {CompetitionName}", competition.Name);
            _isLoaded = false;
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading data for competition: {CompetitionName} (ID: {CompetitionId})",
                competition.Name, competition.Id);
            _isLoaded = false;
            throw new InvalidOperationException($"Failed to load competition data: {ex.Message}", ex);
        }
        finally
        {
            // Clear authentication header
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<IReadOnlyList<Competition>> GetCompetitionsAsync(DateTime startDate, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default)
    {
        if (authenticationInfo == null)
        {
            throw new ArgumentNullException(nameof(authenticationInfo));
        }

        const string endpoint = "/competition/evenement";
        var queryParameters = new Dictionary<string, string>
        {
            { "debut", startDate.ToString("yyyy-MM-dd") },
            { "length", "500" }
        };

        try
        {
            _logger.LogInformation("Requesting competitions starting from: {StartDate}", startDate);

            // Set authentication header
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationInfo.Token);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            using var response = await _httpClient.GetAsync($"{_version}{endpoint}?{queryString}", cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);
            var competitions = new List<Competition>();

            if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
            {
                foreach (var competitionData in jData.Children())
                {
                    try
                    {
                        var competition = new Competition(competitionData);
                        competitions.Add(competition);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse competition data: {CompetitionData}", competitionData);
                    }
                }
            }
            else
            {
                _logger.LogWarning("API returned unsuccessful response or invalid data format");
            }

            _logger.LogInformation("Retrieved {CompetitionCount} competitions", competitions.Count);
            return competitions;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Get competitions request was cancelled");
            throw;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error during competitions request");
            throw new InvalidOperationException($"Network error while retrieving competitions: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during competitions request");
            throw new InvalidOperationException($"Unexpected error while retrieving competitions: {ex.Message}", ex);
        }
        finally
        {
            // Clear authentication header
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }

    #endregion

    #region Private Data Loading Methods

    private async Task LoadCategoriesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/categories";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token!);
        }

        try
        {
            _logger.LogDebug("Loading categories for competition: {CompetitionId}", competition.Id);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

            using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
            {
                _categories.Clear();
                foreach (var categoryData in jData.Children())
                {
                    try
                    {
                        var category = new Category(categoryData);
                        _categories.Add(category);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse category data: {CategoryData}", categoryData);
                    }
                }
                _logger.LogDebug("Loaded {CategoryCount} categories", _categories.Count);
            }
            else
            {
                _logger.LogWarning("Categories API returned unsuccessful response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadClubsAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/organismes";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token!);
        }

        try
        {
            _logger.LogDebug("Loading clubs for competition: {CompetitionId}", competition.Id);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? $"{_version}{endpoint}" : $"{_version}{endpoint}?{queryString}";

            // Set authentication header
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationInfo.Token);
            using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
            {
                _clubs.Clear();
                foreach (var clubData in jData.Children())
                {
                    try
                    {
                        int idClub = clubData["Id"]?.Value<int>() ?? 0;
                        bool isForeignClub = (idClub == 245);
                        Club club = null;
                        if(idClub == 245) //we create the club FFSS
                        {
                            club = new Club(clubData, false);
                            club.Competition = competition;
                            _clubs.Add(club);
                        }

                        club = null;
                        club = new Club(clubData, isForeignClub);
                        club.Competition = competition;
                        _clubs.Add(club);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse club data: {ClubData}", clubData);
                    }
                }

                if (jData.Count == _clubs.Count)
                {
                    _logger.LogDebug("Loaded {ClubCount} clubs", _clubs.Count);
                }
                else
                {
                    int missingClubCount = jData.Count - _clubs.Count;
                    _logger.LogError("Not all clubs loaded missing { missingClubCount}", missingClubCount);
                    //throw new Exception($"Not all clubs loaded missing {missingClubCount}");
                }
            }
            else
            {
                _logger.LogWarning("Clubs API returned unsuccessful response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading clubs for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadLicenseesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Loading licensees for competition: {CompetitionId}", competition.Id);

            await LoadAthletesAsync(competition, authenticationInfo, cancellationToken);
            await LoadRefereesAsync(competition, authenticationInfo, cancellationToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading licensees for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadAthletesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/participants";
        var queryParameters = new Dictionary<string, string>();

        try
        {
            if (authenticationInfo.IsTokenValid)
            {
                queryParameters.Add("token", authenticationInfo.Token!);


                _logger.LogDebug("Loading Athletes for competition: {CompetitionId}", competition.Id);

                string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

                using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

                if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
                {
                    _licensees.Clear();
                    int count = 0;
                    _logger.LogDebug("expected athlete : {AhtleteNumber}", jData.Children().Count());
                    foreach (var licenseeData in jData.Children())
                    {
                        try
                        {
                            Athlete athlete = new Athlete(licenseeData);
                            Club? club = _clubs.Find(c => c.Id == licenseeData["idClub"]!.Value<int>());
                            if (club != null)
                            {
                                athlete.Club = club;
                                athlete.ClubId = club.Id;
                                club.AddLicensee(athlete);
                            }
                            else

                            {
                                // Handle the case where club is not found
                                _logger.LogInformation("No club for Athlete : {AthleteFullName}", athlete.FullName);
                                Club missingClub = new Club(licenseeData["idClub"]!.Value<int>(), licenseeData["clubLabel"].Value<string>());
                                if (_clubs.Exists(c => c.Id == missingClub.Id) == false)
                                {
                                    _clubs.Add(missingClub);

                                }
                                athlete.Club = missingClub;
                                athlete.ClubId = missingClub.Id;
                                missingClub.AddLicensee(athlete);
                            }


                            _licensees.Add(athlete);
                            _athletes.Add(athlete);
                            ++count;
                            _logger.LogDebug("current number : {AhtleteCount}", count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse athletes data");
                        }
                    }

                    if (jData.Count == _athletes.Count)
                    {
                        _logger.LogDebug("Loaded {LicenseeCount} athletes", _athletes.Count);
                    }
                    else
                    {
                        int missingAthletesCount = jData.Count - _athletes.Count;
                        _logger.LogError("Not all athletes loaded, missing { missingAthletesCount}", missingAthletesCount);
                        throw new Exception($"Not all athletes loaded, missing {missingAthletesCount}");
                    }
                }
                else
                {
                    _logger.LogWarning("Athletes API returned unsuccessful response");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading athletes for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }
    private async Task LoadRefereesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/officiels";
        var queryParameters = new Dictionary<string, string>();

        try
        {
            if (authenticationInfo.IsTokenValid)
            {
                queryParameters.Add("token", authenticationInfo.Token!);


                _logger.LogDebug("Loading Referees for competition: {CompetitionId}", competition.Id);

                string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

                using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

                if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
                {
                    _licensees.Clear();
                    foreach (var licenseeData in jData.Children())
                    {
                        try
                        {
                            Referee referee = new Referee(licenseeData, competition.BeginDate);
                            int idOrganisme = licenseeData["IdOrganisme"]!.Value<int>();
                            int idClub = licenseeData["idOfficielClub"]!.Value<int>();
                            Club? club = null;
                            if (idOrganisme != idClub)
                            {
                                club = _clubs.Find(c => c.Id == idOrganisme);
                            }
                            else
                            {
                                club = _clubs.Find(c => c.Id == idClub);
                            }

                            if (club != null)
                            {
                                referee.Club = club;
                                club.AddLicensee(referee);
                            }
                            else

                            {
                                // Handle the case where club is not found
                                _logger.LogDebug("No club for Referee : {AthleteFullName}", referee.FullName);
                                Club missingClub = new Club(licenseeData["idOfficielClub"]!.Value<int>(), licenseeData["officielClubLabel"].Value<string>());
                                if (_clubs.Exists(c => c.Id == missingClub.Id) == false)
                                {
                                    _clubs.Add(missingClub);
                                }
                                referee.Club = missingClub;
                                referee.ClubId = missingClub.Id;
                                missingClub.AddLicensee(referee);
                                throw new InvalidOperationException("Club not found");
                            }

                            _licensees.Add(referee);
                            _referees.Add(referee);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse referees data: {LicenseeData}", licenseeData["Id"]!.Value<int>());
                        }
                    }
                    _logger.LogDebug("Loaded {LicenseeCount} athletes", _referees.Count);
                }
                else
                {
                    _logger.LogWarning("Referees API returned unsuccessful response");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading athletes for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadRacesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/epreuve";
        var queryParameters = new Dictionary<string, string>();
        try
        {
            if (authenticationInfo.IsTokenValid)
            {
                queryParameters.Add("token", authenticationInfo.Token);
                queryParameters.Add("evenement", competition.Id.ToString());

                _logger.LogDebug("Loading races for competition: {CompetitionId}", competition.Id);

                string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
                string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

                using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
                response.EnsureSuccessStatusCode();

                string responseBody = await response.Content.ReadAsStringAsync();
                var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

                if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
                {
                    _races.Clear();
                    foreach (var raceData in jData.Children())
                    {
                        try
                        {
                            var race = new Race(raceData, _categories);
                            if (race != null)
                            {
                                _races.Add(race);
                                race.Competition = competition;
                                competition.Races.Add(race);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to parse race data: {RaceData}", raceData);
                        }
                    }

                    if (jData.Count == _races.Count)
                    {
                        _logger.LogDebug("Loaded {RaceCount} races", _races.Count);
                    }
                    else
                    {
                        int missingRacesCount = jData.Count - _races.Count;
                        _logger.LogError("Not all races loaded, missing { missingRacesCount}", missingRacesCount);
                        throw new Exception($"Not all races loaded, missing {missingRacesCount}");
                    }
                }
                else
                {
                    _logger.LogWarning("Races API returned unsuccessful response");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading races for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadTeamsAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogDebug("Loading teams for {RaceCount} races in competition: {CompetitionId}", _races.Count, competition.Id);

            var teamLoadTasks = _races.Select(race => LoadTeamsForRaceAsync(race, authenticationInfo, cancellationToken));
            await Task.WhenAll(teamLoadTasks);

            _logger.LogDebug("Loaded teams for all races in competition: {CompetitionId}", competition.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading teams for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadTeamsForRaceAsync(Race race, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = "/competition/engagement";
        var queryParameters = new Dictionary<string, string>
        {
            { "token", authenticationInfo.Token },
            { "epreuve", race.Id.ToString() }
        };

        try
        {
            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));

            using var response = await _httpClient.GetAsync($"{_version}{endpoint}?{queryString}", cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
            {
                Team? team = null;
                foreach (var teamData in jData.Children())
                {
                    try
                    {
                        if (race.NumberByTeam == 1)
                        {
                            team = new IndividualTeam(teamData, race, _athletes, _categories);
                        }
                        else
                        {
                            team = new RelayTeam(teamData, race, _athletes, _categories);
                        }
                        if (team != null)
                        {
                            _teams.Add(team);
                            race.Teams.Add(team);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse team data for race {RaceId}: {TeamData}", race.Id, teamData["Id"].Value<int>());
                    }
                }


                //if (jData.Count == _teams.Count)
                //{
                //    _logger.LogDebug("Loaded {TeamsCount} teams", _teams.Count);
                //}
                //else
                //{
                //    int missingTeamsCount = jData.Count - _teams.Count;
                //    _logger.LogError("Not all teams loaded, missing { missingTeamsCount}", missingTeamsCount);
                //    throw new Exception($"Not all teams loaded, missing {missingTeamsCount}");
                //}
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading teams for race: {RaceId}", race.Id);
            throw;
        }
    }

    private async Task LoadRaceFormatConfigurationsAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/{competition.Id}/deroulement";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token!);
        }

        try
        {
            _logger.LogDebug("Loading RaceFormatConfigurations for competition: {CompetitionId}", competition.Id);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

            using var response = await _httpClient.GetAsync(apiUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            if (jResponse?["success"]?.Value<bool>() == true && jResponse["data"] is JArray jData)
            {
                _raceFormatConfigurations.Clear();
                foreach (var raceFormatConfigurationData in jData.Children())
                {
                    try
                    {
                        // Create a new RaceFormatConfiguration instance and RaceFormatDetail instances
                        var raceFormatConfiguration = new RaceFormatConfiguration(raceFormatConfigurationData, _categories);
                        _raceFormatConfigurations.Add(raceFormatConfiguration);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse raceFormatConfiguration data: {RaceFormatConfigurationData}", raceFormatConfigurationData);
                    }
                }
                _logger.LogDebug("Loaded {raceFormatConfigurationCount} raceFormatConfigurations", _raceFormatConfigurations.Count);
            }
            else
            {
                _logger.LogWarning("Categories API returned unsuccessful response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading raceFormatConfigurations for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    #endregion

    #region Data Access Methods

    public bool GetIsLoaded() => _isLoaded;
    public IReadOnlyList<Category> GetCategories() => _categories.AsReadOnly();
    public IReadOnlyList<Licensee> GetLicensees() => _licensees.AsReadOnly();
    public IReadOnlyList<Athlete> GetAthletes() => _athletes.AsReadOnly();
    public IReadOnlyList<Referee> GetReferees() => _referees.AsReadOnly();
    public IReadOnlyList<Club> GetClubs() => _clubs.AsReadOnly();
    public IReadOnlyList<Race> GetRaces() => _races.AsReadOnly();
    public IReadOnlyList<Team> GetTeams() => _teams.AsReadOnly();
    public IReadOnlyList<RaceFormatConfiguration> GetRaceFormatConfigurations() => _raceFormatConfigurations.AsReadOnly();
    public Competition? GetCompetition() => _competition;
    public void SetCompetition(Competition competition) => _competition = competition ?? throw new ArgumentNullException(nameof(competition));

    public void Reset()
    {
        _isLoaded = false;
        _competition = null;
        _categories.Clear();
        _licensees.Clear();
        _clubs.Clear();
        _races.Clear();
        _teams.Clear();
        _logger.LogDebug("ApiService data has been reset");
    }

    public async Task SubmitRaceFormatConfigurationAsync(RaceFormatConfiguration raceFormatConfiguration, Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default)
    {
        if (authenticationInfo == null)
        {
            throw new ArgumentNullException(nameof(authenticationInfo));
        }
        if (competition == null)
        {
            throw new InvalidOperationException("Aucune compétition n'est chargée.");
        }
        string endpoint = $"/competition/{competition.Id}/deroulement/submit";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token!);
            if (raceFormatConfiguration.Id != 0)
            {
                queryParameters.Add("id", raceFormatConfiguration.Id.ToString());
            }
            else
            {
                queryParameters.Add("id", "");
            }
        }
        queryParameters.Add("discipline", raceFormatConfiguration.Discipline.ToString());
        queryParameters.Add("genre", EnumRSM.GetGenderLetter(raceFormatConfiguration.Gender));
        foreach (Category category in raceFormatConfiguration.Categories)
        {

            queryParameters.Add("categories[]", category.Id.ToString());
        }

        //token=gQEKgILl%24cXgCxswuM8yyol4Zo13jA_0s18Pf52D%21cZVRfnSYCG8RGxs_iXeLuu1&id=&discipline=9&genre=F&categories%5B%5D=13

        try
        {
            _logger.LogInformation("Submit RaceFormatConfiguration : {raceFormatConfiguration}", raceFormatConfiguration.FullLabel);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? $"{_version}{endpoint}" : $"{_version}{endpoint}?{queryString}";
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending POST request to: {ApiUrl}", apiUrl);

            using var response = await _httpClient.PostAsync(apiUrl, requestContent);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

            if (jResponse?["success"]?.Value<bool>() == true)
            {
                raceFormatConfiguration.Id = jResponse["id"]?.Value<int>() ?? raceFormatConfiguration.Id;
                foreach (RaceFormatDetail raceFormatDetail in raceFormatConfiguration.RaceFormatDetails)
                {
                    await SubmitRaceFormatDetailAsync(raceFormatDetail, raceFormatConfiguration, competition, authenticationInfo, cancellationToken);
                }
            }
            else
            {
                _logger.LogWarning("Submit RaceFormatConfiguration API returned unsuccessful response");
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(ex, "Request timed out after 10 seconds for RaceFormatConfiguration: {RaceFormatConfiguration}. The server may have processed the request successfully but failed to send a response.", raceFormatConfiguration.FullLabel);
            // Since the data appears on the website, we'll treat this as a successful operation
            return;
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Submit RaceFormatConfiguration request was cancelled: {RaceFormatConfiguration}", raceFormatConfiguration.FullLabel);
            throw new OperationCanceledException("The operation was cancelled.", ex, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while submitting RaceFormatConfiguration: {RaceFormatConfiguration}", raceFormatConfiguration.FullLabel);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Submit RaceFormatConfiguration: {RaceFormatConfiguration}", raceFormatConfiguration.FullLabel);
            throw;
        }


        // Implémentation à ajouter ici selon la logique métier.
        // Pour corriger l'erreur CS0161, retourner une tâche terminée.
        //return Task.CompletedTask;
    }

    public async Task SubmitRaceFormatDetailAsync(RaceFormatDetail raceFormatDetail, RaceFormatConfiguration raceFormatConfiguration, Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken = default)
    {
        if (authenticationInfo == null)
        {
            throw new ArgumentNullException(nameof(authenticationInfo));
        }
        if (competition == null)
        {
            throw new InvalidOperationException("Aucune compétition n'est chargée.");
        }
        string endpoint = $"/competition/deroulement/{raceFormatConfiguration.Id}/partie/submit";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token!);
            if (raceFormatDetail.Id != 0)
            {
                queryParameters.Add("id", raceFormatDetail.Id.ToString());
            }
            else
            {
                queryParameters.Add("id", "");
            }
        }
        queryParameters.Add("deroulement", raceFormatConfiguration.Id.ToString());
        queryParameters.Add("ordre", raceFormatDetail.Order.ToString());
        queryParameters.Add("niveau", EnumRSM.GetEnumDescription(raceFormatDetail.Level));
        queryParameters.Add("logiqueQualification", EnumRSM.GetEnumDescription(raceFormatDetail.QualificationMethod));
        queryParameters.Add("nbCourse", raceFormatDetail.NumberOfRun.ToString());
        queryParameters.Add("nbPlace", raceFormatDetail.SpotsPerRace.ToString());
        queryParameters.Add("nbPlaceQualificative", raceFormatDetail.QualifyingSpots.ToString());
        
        foreach (Category category in raceFormatConfiguration.Categories)
        {

            queryParameters.Add("categories[]", category.Id.ToString());
        }

        //token=gQEKgILl%24cXgCxswuM8yyol4Zo13jA_0s18Pf52D%21cZVRfnSYCG8RGxs_iXeLuu1&id=&discipline=9&genre=F&categories%5B%5D=13

        try
        {
            _logger.LogInformation("Submit RaceFormatDetail : {RaceFormatDetail}", raceFormatDetail.FullLabel);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? $"{_version}{endpoint}" : $"{_version}{endpoint}?{queryString}";
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");

            _logger.LogInformation("Sending POST request to: {ApiUrl}", apiUrl);
            //api/v1.0/competition/deroulement/33/partie/submit?token=yuzEl5zbfxI%24QVvPCRGYUVkbulLBHO3pSxOTPvb5RLpAyKrFhroeUwk4CFCOWnZp&id=&ordre=1&niveau=Heat&logiqueQualification=course&nbCourse=4&nbPlace=13&categories%5B%5D=11
            using var response = await _httpClient.PostAsync(apiUrl, requestContent);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            var jResponse = JsonConvert.DeserializeObject<JToken>(responseBody);

            _logger.LogInformation("Received response with status code: {StatusCode}", response.StatusCode);

            if (jResponse?["success"]?.Value<bool>() == true)
            {
                raceFormatDetail.Id = jResponse["id"].Value<int>();
            }
            else
            {
                _logger.LogWarning("Submit RaceFormatDetail API returned unsuccessful response");
            }
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException || !cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation(ex, "Request timed out after 10 seconds for RaceFormatDetail: {RaceFormatDetail}. The server may have processed the request successfully but failed to send a response.", raceFormatDetail.FullLabel);
            // Since the data appears on the website, we'll treat this as a successful operation
            return;
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Submit RaceFormatDetail request was cancelled: {RaceFormatDetail}", raceFormatDetail.FullLabel);
            throw new OperationCanceledException("The operation was cancelled.", ex, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while submitting RaceFormatDetail: {RaceFormatDetail}", raceFormatDetail.FullLabel);
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error Submit RaceFormatDetail: {RaceFormatDetail}", raceFormatDetail.FullLabel);
            throw;
        }


        // Implémentation à ajouter ici selon la logique métier.
        // Pour corriger l'erreur CS0161, retourner une tâche terminée.
        //return Task.CompletedTask;
    }
    #endregion Data Saving Methods

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _httpClient?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    #endregion
}
