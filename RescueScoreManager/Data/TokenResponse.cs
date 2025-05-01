using System.Text.Json.Serialization;

namespace RescueScoreManager.Data;

public class TokenResponse
{
    public string? Token { get; set; }

    public DateTime Expiration { get; set; }

    public bool Success { get; set; }

    public string? Message { get; set; }
}