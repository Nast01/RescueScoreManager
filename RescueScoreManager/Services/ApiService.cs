using System.Buffers.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using DocumentFormat.OpenXml.Office2010.Excel;

using System.Text.Json;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;
using RescueScoreManager.Helpers;

using static System.Reflection.Metadata.BlobBuilder;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace RescueScoreManager.Services;

public class ApiService : IApiService
{
    #region Attributes
    private HttpClient _httpClient;
    private string _baseAdress = "https://ffss.fr/api/v1.0";
    public ApiToken? Token { get; set; }
    public bool IsLoaded { get; set; }
    public Competition? Competition { get; set; }
    public List<Category> Categories { get; set; }
    public List<Licensee> Licensees { get; set; }
    public List<Club> Clubs { get; set; }
    public List<Race> Races { get; set; }
    public List<Team> Teams { get; set; }
    #endregion Attributes

    public ApiService()
    {
        if (Debugger.IsAttached)
        {
            _baseAdress = "https://qual.ffss.fr/api/v1.0";
        }

        // Initialize the HttpClient with base API URL, headers, or other configuration
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseAdress);

        Competition = null;
        Categories = new List<Category>();
        Licensees = new List<Licensee>();
        Clubs = new List<Club>();
        Races = new List<Race>();
        Teams = new List<Team>();
        IsLoaded = false;
    }

    #region Accessors
    #region Token
    public async Task<TokenResponse> RequestTokenAsync(string login, string password)
    {
        string endpoint = "/requestToken";
        var queryParameters = new Dictionary<string, string>
            {
                { "login", login },
                { "password", password }
            };

        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));

        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        try
        {
            // Make a POST request
            HttpResponseMessage response = await _httpClient.PostAsync($"{_baseAdress}{endpoint}?{queryString}", requestContent);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(jsonResponse);

            //if (response.IsSuccessStatusCode)
            //{
            //    string responseBody = await response.Content.ReadAsStringAsync();
            //    // Deserialize the JSON response into a class
            //    Token = JsonConvert.DeserializeObject<ApiToken>(responseBody);
            //    //DataAccessService.Instance.IsLoggedIn = true;
            //}
            //else
            //{
            //    Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("TokenError")}" + response.StatusCode);
            //}
        }
        catch (HttpRequestException ex)
        {
            // En cas d'erreur HTTP, on retourne un TokenResponse avec l'erreur
            return new TokenResponse
            {
                Success = false,
                Message = $"{ResourceManagerLocalizationService.Instance.GetString("HttpError")}: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new TokenResponse
            {
                Success = false,
                Message = $"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}"
            };
        }
    }
    public async Task<UserInfo> GetUserInfoAsync(string token)
    {
        string endpoint = "/me";
        try
        {
            // Ajouter le token à l'en-tête pour l'authentification
            //_httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var queryParameters = new Dictionary<string, string>
            {
                { "token", token },
            };

            // Define the data to be sent in the request body
            string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));

            var response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            JToken? jResponse = JsonConvert.DeserializeObject(jsonResponse) as JToken;
            UserInfo userInfo = new UserInfo();
            if (jResponse != null)
            {
                userInfo.Success = jResponse["success"].Value<bool>();
                userInfo.Message = jResponse["message"].Value<string>();
                userInfo.Label = jResponse["label"].Value<string>();
                userInfo.Type = jResponse["type"].Value<string>();
                userInfo.Role = jResponse["data"]["role"].Value<string>();
            }

            return userInfo;
        }
        catch (HttpRequestException ex)
        {
            return new UserInfo
            {
                Success = false,
                Message = $"{ResourceManagerLocalizationService.Instance.GetString("HttpError")}: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            return new UserInfo
            {
                Success = false,
                Message = $"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}"
            };
        }
    }
    #endregion Token
    public bool GetIsLoaded()
    {
        return IsLoaded;
    }

    public Competition GetCompetition()
    {
        return Competition;
    }
    public void SetCompetition(Competition competition)
    {
        Competition = competition;
    }

    public List<Category> GetCategories()
    {
        return Categories;
    }
    public List<Licensee> GetLicensees()
    {
        return Licensees;
    }
    public List<Club> GetClubs()
    {
        return Clubs;
    }
    public List<Race> GetRaces()
    {
        return Races;
    }
    public List<Team> GetTeams()
    {
        return Teams;
    }

    #endregion Accessors

    #region Public Methods
    public async Task<List<Competition>> GetCompetitions(DateTime startDate, AuthenticationInfo authenticationInfo)
    {
        string endpoint = "/competition/evenement";
        var queryParameters = new Dictionary<string, string>
            {
                { "debut", startDate.ToString("yyyy-MM-dd") },
                { "length", "500" }
            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        List<Competition> competitions = new List<Competition>();
        try
        {
            // Set the Authorization header with the bearer token
            if (authenticationInfo != null)
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authenticationInfo.Token);
            }

            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response into a class
                JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                if (jResponse != null)
                {
                    bool jStatus = jResponse["success"].Value<bool>();
                    int jRecordsFiltered = jResponse["recordsFiltered"].Value<int>();
                    int jAlreadyShown = jResponse["alreadyShown"].Value<int>();
                    int jRemains = jResponse["remains"].Value<int>();
                    int jNext = jResponse["remains"].Value<int>();

                    JArray? jData = jResponse["data"] as JArray;
                    foreach (var data in jData.Children())
                    {
                        Competition competition = new Competition(data);

                        competitions.Add(competition);
                    }
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieved the Competitions. Status Code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return competitions;
    }

    public async Task Load(Competition competition, AuthenticationInfo authenticationInfo)
    {
        try
        {
            SetCompetition(competition);
            await LoadCategories(competition, authenticationInfo);
            await LoadClubs(competition, authenticationInfo);
            await LoadReferees(competition, authenticationInfo);
            //await LoadReferees(competition, authenticationInfo);
            //await LoadRaces(competition);
            //await LoadTeams(competition);
            IsLoaded = true;
        }
        catch (HttpRequestException ex)
        {
            IsLoaded = false;
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    #endregion Public Methods

    #region Private Methods
    private async Task LoadCategories(Competition competition, AuthenticationInfo authenticationInfo)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/categories";
        var queryParameters = new Dictionary<string, string>();
        if (authenticationInfo != null && authenticationInfo.IsTokenValid == true)

        {
            queryParameters.Add("token", authenticationInfo.Token);
        }
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        try
        {
            // Make a GET request
            string apiUrl = queryString != "" ? $"{_baseAdress}{endpoint}?{queryString}" : $"{_baseAdress}{endpoint}";
            HttpResponseMessage response = await _httpClient.GetAsync($"{apiUrl}");
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON response into a class
            JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

            if (jResponse != null)
            {
                JArray? jDatas = jResponse["data"] as JArray;
                foreach (var jData in jDatas.Children())
                {
                    Category category = new Category(jData);
                    Categories.Add(category);
                }
            }
            else
            {
                Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")} the {ResourceManagerLocalizationService.Instance.GetString("Categories")}. {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}");
        }
    }
    private async Task LoadClubs(Competition competition, AuthenticationInfo authenticationInfo)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/organismes";
        var queryParameters = new Dictionary<string, string>();
        if (authenticationInfo != null && authenticationInfo.IsTokenValid == true)
        {
            queryParameters.Add("token", authenticationInfo.Token);
        }
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        try
        {
            // Make a GET request
            string apiUrl = queryString != "" ? $"{_baseAdress}{endpoint}?{queryString}" : $"{_baseAdress}{endpoint}";
            HttpResponseMessage response = await _httpClient.GetAsync($"{apiUrl}");
            response.EnsureSuccessStatusCode();


            string responseBody = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON response into a class
            JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

            if (jResponse != null)
            {
                JArray? jData = jResponse["data"] as JArray;
                Club? club = null;
                foreach (var data in jData.Children())
                {
                    int idClub = data["Id"].Value<int>();
                    bool isForeignClub = (idClub == 245);

                    club = new Club(data, isForeignClub);
                    Clubs.Add(club);
                }
            }
            else
            {
                Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")} the {ResourceManagerLocalizationService.Instance.GetString("Clubs")}. {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}");
        }
    }
    private async Task LoadReferees(Competition competition, AuthenticationInfo authenticationInfo)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/officiels";
        var queryParameters = new Dictionary<string, string>
            {
                { "token", authenticationInfo.Token }
            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        List<Referee> referees = new List<Referee>();
        try
        {
            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");
            response.EnsureSuccessStatusCode();


            string responseBody = await response.Content.ReadAsStringAsync();
            // Deserialize the JSON response into a class
            JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

            if (jResponse != null)
            {
                JArray? jReferees = jResponse["data"] as JArray;
                foreach (var jReferee in jReferees)
                {
                    Referee referee = new Referee(jReferee, competition.BeginDate);
                }

            }
            else
            {
                Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")} the {ResourceManagerLocalizationService.Instance.GetString("Referees")}. {response.StatusCode}");
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("HttpError")}: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"{ResourceManagerLocalizationService.Instance.GetString("UnexpectedError")}: {ex.Message}");
        }
    }
    private async Task<List<Race>> LoadRaces(Competition competition)
    {
        string endpoint = $"/competition/epreuve";
        var queryParameters = new Dictionary<string, string>
            {
                { "token", Token.Token },
                { "evenement", competition.Id.ToString() }
            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        List<Race> races = new List<Race>();
        try
        {
            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response into a class
                //JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                //if (jResponse != null && jResponse["success"].Value<bool>() == true)
                //{
                //    JArray? jDatas = jResponse["data"] as JArray;
                //    foreach (var jData in jDatas.Children())
                //    {
                //        Race race = new Race(jData, Categories);
                //        if (race != null)
                //        {
                //            Races.Add(race);
                //            race.Competition = competition;
                //            competition.Races.Add(race);
                //        }
                //    }
                //}
                //else
                //{
                //    Console.WriteLine("Failed to retrieved the Races. Status Code: " + response.StatusCode);
                //}
            }
            else
            {
                Console.WriteLine("Failed to retrieved the Races. Status Code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return races;
    }
    private async Task LoadTeams(Competition competition)
    {
        foreach (Race race in competition.Races)
        {
            string endpoint = $"/competition/engagement";
            var queryParameters = new Dictionary<string, string>
                {
                    { "token", Token.Token },
                    { "epreuve", race.Id.ToString() }
                };
            // Define the data to be sent in the request body
            string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
            // Create a StringContent object with the request body and content type
            var requestContent = new StringContent("", Encoding.UTF8, "application/json");

            List<Race> races = new List<Race>();
            try
            {
                // Make a GET request
                HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON response into a class
                    //JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                    //if (jResponse != null && jResponse["success"].Value<bool>() == true)
                    //{
                    //    List<Team> teams = new List<Team>();
                    //    JArray? jDatas = jResponse["data"] as JArray;
                    //    foreach (var jData in jDatas.Children())
                    //    {
                    //        Team team = null;
                    //        if (race.NumberByTeam == 1)
                    //        {
                    //            team = new IndividualTeam(jData, race, Licensees, Categories);
                    //        }
                    //        else
                    //        {
                    //            team = new RelayTeam(jData, race, Licensees);
                    //        }

                    //        if (team != null)
                    //        {
                    //            team.Race = race;
                    //            race.Teams.Add(team);
                    //        }
                    //    }
                    //}
                    //else
                    //{
                    //    Console.WriteLine("Failed to retrieved the Teams. Status Code: " + response.StatusCode);
                    //}
                }
                else
                {
                    Console.WriteLine("Failed to retrieved the Teams. Status Code: " + response.StatusCode);
                }
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }
    }

    #endregion Private Methods
}
