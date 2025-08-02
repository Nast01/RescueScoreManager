namespace RescueScoreManager.Data;

public class ApiToken 
{
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string Message { get; set; } = string.Empty;

    public bool IsValid()
    {
        return Expiration >= DateTime.Now && String.IsNullOrEmpty(Token) == false;
    }
}
