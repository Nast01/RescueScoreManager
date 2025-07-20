using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RescueScoreManager.Data;

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
    private List<Club> _clubs { get; set; } = new();
    private List<Race> _races { get; set; } = new();
    private List<Team> _teams { get; set; } = new();

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

            // Load data in parallel where possible
            var loadTasks = new List<Task>
            {
                LoadCategoriesAsync(competition, authenticationInfo, cancellationToken),
                LoadClubsAsync(competition, authenticationInfo, cancellationToken),
                LoadLicenseesAsync(competition, authenticationInfo, cancellationToken)
            };

            // Wait for core data to load
            await Task.WhenAll(loadTasks);

            // Load dependent data
            await LoadRacesAsync(competition, authenticationInfo, cancellationToken);
            await LoadTeamsAsync(competition, authenticationInfo, cancellationToken);

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
            queryParameters.Add("token", authenticationInfo.Token);
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
        string endpoint = $"/competition/evenement/{competition.Id}/clubs";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token);
        }

        try
        {
            _logger.LogDebug("Loading clubs for competition: {CompetitionId}", competition.Id);

            string queryString = string.Join("&", queryParameters.Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value)}"));
            string apiUrl = string.IsNullOrEmpty(queryString) ? endpoint : $"{_version}{endpoint}?{queryString}";

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
                        int idClub = clubData["Id"].Value<int>();
                        bool isForeignClub = (idClub == 245);

                        var club = new Club(clubData, isForeignClub);
                        _clubs.Add(club);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse club data: {ClubData}", clubData);
                    }
                }
                _logger.LogDebug("Loaded {ClubCount} clubs", _clubs.Count);
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
        string endpoint = $"/competition/evenement/{competition.Id}/licensees";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token);
        }

        try
        {
            _logger.LogDebug("Loading licensees for competition: {CompetitionId}", competition.Id);

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
                        _licensees.Add(referee);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse licensee data: {LicenseeData}", licenseeData);
                    }
                }
                _logger.LogDebug("Loaded {LicenseeCount} licensees", _licensees.Count);
            }
            else
            {
                _logger.LogWarning("Licensees API returned unsuccessful response");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading licensees for competition: {CompetitionId}", competition.Id);
            throw;
        }
    }

    private async Task LoadRacesAsync(Competition competition, AuthenticationInfo authenticationInfo, CancellationToken cancellationToken)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/races";
        var queryParameters = new Dictionary<string, string>();

        if (authenticationInfo.IsTokenValid)
        {
            queryParameters.Add("token", authenticationInfo.Token);
        }

        try
        {
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
                _logger.LogDebug("Loaded {RaceCount} races", _races.Count);
            }
            else
            {
                _logger.LogWarning("Races API returned unsuccessful response");
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
                foreach (var teamData in jData.Children())
                {
                    try
                    {
                        //var team = new Team(teamData);
                        //Teams.Add(team);
                        //race.Teams.Add(team);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse team data for race {RaceId}: {TeamData}", race.Id, teamData);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading teams for race: {RaceId}", race.Id);
            throw;
        }
    }

    #endregion

    #region Data Access Methods

    public bool GetIsLoaded() => _isLoaded;
    public IReadOnlyList<Category> GetCategories() => _categories.AsReadOnly();
    public IReadOnlyList<Licensee> GetLicensees() => _licensees.AsReadOnly();
    public IReadOnlyList<Club> GetClubs() => _clubs.AsReadOnly();
    public IReadOnlyList<Race> GetRaces() => _races.AsReadOnly();
    public IReadOnlyList<Team> GetTeams() => _teams.AsReadOnly();
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

    #endregion

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
