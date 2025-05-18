namespace SuperInvestor.Features.Companies.Models;

public class Quote
{
    public decimal RegularMarketPrice { get; set; }
    public decimal RegularMarketChange { get; set; }
    public decimal RegularMarketChangePercent { get; set; }
    public string MarketState { get; set; }
    public string Currency { get; set; }
    public string ExchangeName { get; set; }
    public string FullExchangeName { get; set; }
    public string Symbol { get; set; }
    public string ShortName { get; set; }
    public string LongName { get; set; }
}