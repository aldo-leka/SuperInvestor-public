using System.Text.Json;
using System.Text.Json.Serialization;
using SuperInvestor.Features.Common.Services;

namespace SuperInvestor.Features.Identity.Services;

public class TurnstileService
{
    private readonly HttpClient _httpClient;
    
    public TurnstileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<bool> VerifyTokenAsync(string token, string? remoteIp = null)
    {
        var formData = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("secret", EnvironmentHelper.TurnstileSecretKey),
            new KeyValuePair<string, string>("response", token),
            new KeyValuePair<string, string>("remoteip", remoteIp ?? "")
        });
        
        var response = await _httpClient.PostAsync(
            "https://challenges.cloudflare.com/turnstile/v0/siteverify", 
            formData);
            
        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<TurnstileResponse>(json);
        
        return result?.Success == true;
    }
}

public class TurnstileResponse
{
    [JsonPropertyName("success")]
    public bool Success { get; set; }
    
    [JsonPropertyName("error-codes")]
    public string[]? ErrorCodes { get; set; }
}