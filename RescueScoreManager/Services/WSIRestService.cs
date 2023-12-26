using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RescueScoreManager.Comparer;
using RescueScoreManager.Data;
using RescueScoreManager.Helpers;

using static System.Reflection.Metadata.BlobBuilder;

namespace RescueScoreManager.Services;

public class WSIRestService : IWSIRestService
{
    #region Attributes
    private HttpClient _httpClient;
    private string _baseAdress = "https://ffss.fr/api/v1.0";
    public ApiToken? Token { get; set; }
    public bool IsLoaded { get; set; }

    public Competition Competition { get; set; }
    public List<Category> Categories { get; set; }
    public List<Licensee> Licensees { get; set; }
    public List<Club> Clubs { get; set; }
    public List<Race> Races { get; set; }
    public List<Team> Teams { get; set; }
    #endregion Attributes

    public WSIRestService()
    {
        // Initialize the HttpClient with base API URL, headers, or other configuration
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseAdress);

        Categories = new List<Category>();
        Licensees = new List<Licensee>();
        Clubs = new List<Club>();
        Races = new List<Race>();
        Teams = new List<Team>();
        IsLoaded = false;
    }

    #region Accessors
    #region Token
    public async Task<bool> RequestToken(string login, string password)
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
            if (Token == null || Token.IsValid() == false)
            {
                // Make a POST request
                HttpResponseMessage response = await _httpClient.PostAsync($"{_baseAdress}{endpoint}?{queryString}", requestContent);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    // Deserialize the JSON response into a class
                    Token = JsonConvert.DeserializeObject<ApiToken>(responseBody);
                    //DataAccessService.Instance.IsLoggedIn = true;
                }
                else
                {
                    Console.WriteLine("Failed to retrieved the Token. Status Code: " + response.StatusCode);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }

        return Token.IsValid();
    }

    public ApiToken? GetToken()
    {
        return Token;
    }

    public bool HasToken()
    {
        return Token != null;
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

    public async Task<List<Competition>> GetCompetitions(DateTime startDate)
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
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", Token.Token);

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

    public async Task Load(Competition competition)
    {
        try
        {
            SetCompetition(competition);
            await GetCategories(competition);
            await GetClubsAndLicensees(competition);
            await GetRaces(competition);
            await GetTeams(competition);
            IsLoaded = true;
        }
        catch (HttpRequestException ex)
        {
            IsLoaded = false;
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    public async Task SetLicenseeNationality(Licensee licensee)
    {
        string endpoint = $"/licencie";
        var queryParameters = new Dictionary<string, string>
            {
                { "token", Token.Token },
                { "mode", "api-cpt" },
                { "licence", licensee.Id }

            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        try
        {
            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response into a class
                JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                if (jResponse != null && jResponse["success"].Value<bool>() == true)
                {
                    JArray? jDatas = jResponse["data"] as JArray;
                    string nationality = jDatas[0]["nationalite"].Value<string>();
                    licensee.Nationality = TextHelper.RemoveDiacritics(nationality);
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }

    #region Private Methods
    private async Task GetCategories(Competition competition)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/categories";
        var queryParameters = new Dictionary<string, string>
            {
                { "token", Token.Token }
            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        try
        {
            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response into a class
                JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                if (jResponse != null && jResponse["success"].Value<bool>() == true)
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
                    Console.WriteLine("Failed to retrieved the Categories. Status Code: " + response.StatusCode);
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieved the Categories. Status Code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    private async Task GetClubsAndLicensees(Competition competition)
    {
        string endpoint = $"/competition/evenement/{competition.Id}/organismes";
        var queryParameters = new Dictionary<string, string>
            {
                { "token", Token.Token }
            };
        // Define the data to be sent in the request body
        string queryString = string.Join("&", queryParameters.Select(p => $"{p.Key}={p.Value}"));
        // Create a StringContent object with the request body and content type
        var requestContent = new StringContent("", Encoding.UTF8, "application/json");

        List<Club> clubs = new List<Club>();
        List<Competition> competitions = new List<Competition>();
        try
        {
            // Make a GET request
            HttpResponseMessage response = await _httpClient.GetAsync($"{_baseAdress}{endpoint}?{queryString}");

            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                // Deserialize the JSON response into a class
                JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                if (jResponse != null && jResponse["success"].Value<bool>() == true)
                {
                    JArray? jData = jResponse["data"] as JArray;
                    #region Clubs & Licensees
                    Club club = null;
                    foreach (var data in jData.Children())
                    {
                        club = new Club(data);
                        if (club != null)
                        {
                            JArray? jReferees = data["officiels"] as JArray;
                            foreach (var jReferee in jReferees.Children())
                            {
                                Referee referee = new Referee(jReferee, competition.BeginDate);
                                await SetLicenseeNationality(referee);
                                if (referee != null)
                                {
                                    referee.Club = club;
                                    club.Licensees.Add(referee);// AddLicensee(referee);
                                }
                            }

                            JArray? jAthletes = data["athletes"] as JArray;
                            foreach (var jAthlete in jAthletes.Children())
                            {
                                Athlete athlete = new Athlete(jAthlete);
                                await SetLicenseeNationality(athlete);
                                if (athlete != null)
                                {
                                    athlete.Club = club;
                                    club.Licensees.Add(athlete); //club.AddLicensee(athlete);
                                }
                            }
                            club.Licensees.ToList<Licensee>().Sort(new LicenseeFullNameComparer());

                            club.Competition = competition;
                            competition.Clubs.Add(club);

                            Licensees.AddRange(club.Licensees);
                            Clubs.Add(club);
                        }
                    }

                    //OrderNumber for each licensee by Club
                    //clubs.Sort(new ClubNameComparer());
                    //int orderNumber = 1;
                    //foreach (Club club1 in clubs)
                    //{
                    //    foreach (Athlete athlete in club1.GetAthletes())
                    //    {
                    //        athlete.OrderNumber = orderNumber++;
                    //    }
                    //}

                    #endregion Clubs & Licensees
                }
                else
                {
                    Console.WriteLine("Failed to retrieved the participating Clubs. Status Code: " + response.StatusCode);
                }
            }
            else
            {
                Console.WriteLine("Failed to retrieved the participating Clubs. Status Code: " + response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine("Error: " + ex.Message);
        }
    }
    private async Task<List<Race>> GetRaces(Competition competition)
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
                JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                if (jResponse != null && jResponse["success"].Value<bool>() == true)
                {
                    JArray? jDatas = jResponse["data"] as JArray;
                    foreach (var jData in jDatas.Children())
                    {
                        Race race = new Race(jData, Categories);
                        if (race != null)
                        {
                            Races.Add(race);
                            race.Competition = competition;
                            competition.Races.Add(race);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieved the Races. Status Code: " + response.StatusCode);
                }
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
    private async Task GetTeams(Competition competition)
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
                    JToken? jResponse = JsonConvert.DeserializeObject(responseBody) as JToken;

                    if (jResponse != null && jResponse["success"].Value<bool>() == true)
                    {
                        List<Team> teams = new List<Team>();
                        JArray? jDatas = jResponse["data"] as JArray;
                        foreach (var jData in jDatas.Children())
                        {
                            Team team = null;
                            if (race.NumberByTeam == 1)
                            {
                                team = new IndividualTeam(jData, race, Licensees,Categories);
                            }
                            else
                            {
                                team = new RelayTeam(jData, race, Licensees);
                            }

                            if (team != null)
                            {
                                team.Race = race;
                                race.Teams.Add(team);
                            }
                        }
                    }
                    else
                    {
                        Console.WriteLine("Failed to retrieved the Teams. Status Code: " + response.StatusCode);
                    }
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
