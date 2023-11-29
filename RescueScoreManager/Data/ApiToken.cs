namespace RescueScoreManager.Data;

public class ApiToken 
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
    public string Message { get; set; }

    public bool IsValid()
    {
        return Expiration <= DateTime.Now && String.IsNullOrEmpty(Token) == false;
    }
}
