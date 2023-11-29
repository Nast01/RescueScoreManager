using System.Net.Http;
using System.Text;

using Newtonsoft.Json;

using RescueScoreManager.Data;

namespace RescueScoreManager.Services;

public class WSIRestService : IWSIRestService
{
    #region Attributes
    private HttpClient _httpClient;
    private string _baseAdress = "https://ffss.fr/api/v1.0";
    public ApiToken? Token { get; set; }

    public WSIRestService()
    {
        // Initialize the HttpClient with base API URL, headers, or other configuration
        _httpClient = new HttpClient();
        _httpClient.BaseAddress = new Uri(_baseAdress);
    }

    #endregion Attributes
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
}
