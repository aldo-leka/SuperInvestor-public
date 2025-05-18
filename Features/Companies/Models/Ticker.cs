using System.Text.Json.Serialization;

namespace SuperInvestor.Features.Companies.Models;

public class Ticker
{
    [JsonPropertyName("cik_str")]
    public int Cik { get; set; }

    [JsonPropertyName("ticker")]
    public string Symbol { get; set; }

    [JsonPropertyName("title")]
    public string Name { get; set; }
}
