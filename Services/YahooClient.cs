using Microsoft.Net.Http.Headers;

namespace SuperInvestor.Services;

public class YahooClient
{
    private static HttpClient cookieClient;
    private string crumb;

    private async Task Init()
    {
        string cookie;
        var cookieClientHandler = new HttpClientHandler();
        cookieClient = new HttpClient(cookieClientHandler)
        {
            Timeout = TimeSpan.FromSeconds(30),
        };

        cookieClientHandler.AllowAutoRedirect = true;
        cookieClient.DefaultRequestHeaders.Add(
            HeaderNames.UserAgent,
            "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_10_1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.95 Safari/537.36");

        var response = await cookieClient.GetAsync("https://fc.yahoo.com/");
        if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            cookie = cookies.FirstOrDefault();
        }

        response = await cookieClient.GetAsync("https://query1.finance.yahoo.com/v1/test/getcrumb");
        crumb = await response.Content.ReadAsStringAsync();
    }

    public async Task<Quote> GetAsync(string ticker)
    {
        if (cookieClient is null)
        {
            await Init();
        }

        /*
         *Inputs for the ?modules= query:
         [
   'assetProfile',
   'summaryProfile',
   'summaryDetail',
   'esgScores',
   'price',
   'incomeStatementHistory',
   'incomeStatementHistoryQuarterly',
   'balanceSheetHistory',
   'balanceSheetHistoryQuarterly',
   'cashflowStatementHistory',
   'cashflowStatementHistoryQuarterly',
   'defaultKeyStatistics',
   'financialData',
   'calendarEvents',
   'secFilings',
   'recommendationTrend',
   'upgradeDowngradeHistory',
   'institutionOwnership',
   'fundOwnership',
   'majorDirectHolders',
   'majorHoldersBreakdown',
   'insiderTransactions',
   'insiderHolders',
   'netSharePurchaseActivity',
   'earnings',
   'earningsHistory',
   'earningsTrend',
   'industryTrend',
   'indexTrend',
   'sectorTrend']

        // Pricing, etc: https://stackoverflow.com/questions/44030983/yahoo-finance-url-not-working
         */

        var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{ticker}?modules=summaryProfile,summaryDetail&corsDomain=finance.yahoo.com&formatted=false&symbol={ticker}&crumb={crumb}";
        var response = await cookieClient.GetAsync(url);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Quote>();
        }

        return default;
    } 
}

public class Quote
{
    public QuoteSummary QuoteSummary { get; set; }
}

public class QuoteSummary
{
    public List<Result> Result { get; set; }
    public string Error { get; set; }
}

public class Result
{
    public SummaryDetail SummaryDetail { get; set; }
    public SummaryProfile SummaryProfile { get; set; }
}

public class SummaryDetail
{
    public int MaxAge { get; set; }
    public int PriceHint { get; set; }
    public double PreviousClose { get; set; }
    public double Open { get; set; }
    public double DayLow { get; set; }
    public double DayHigh { get; set; }
    public double RegularMarketPreviousClose { get; set; }
    public double RegularMarketOpen { get; set; }
    public double RegularMarketDayLow { get; set; }
    public double RegularMarketDayHigh { get; set; }
    public double PayoutRatio { get; set; }
    public double Beta { get; set; }
    public double TrailingPE { get; set; }
    public double ForwardPE { get; set; }
    public int Volume { get; set; }
    public int RegularMarketVolume { get; set; }
    public int AverageVolume { get; set; }
    public int AverageVolume10days { get; set; }
    public int AverageDailyVolume10Day { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public int BidSize { get; set; }
    public int AskSize { get; set; }
    public long MarketCap { get; set; }
    public double FiftyTwoWeekLow { get; set; }
    public double FiftyTwoWeekHigh { get; set; }
    public double PriceToSalesTrailing12Months { get; set; }
    public double FiftyDayAverage { get; set; }
    public double TwoHundredDayAverage { get; set; }
    public double TrailingAnnualDividendRate { get; set; }
    public double TrailingAnnualDividendYield { get; set; }
    public string Currency { get; set; }
    public object FromCurrency { get; set; }
    public object ToCurrency { get; set; }
    public object LastMarket { get; set; }
    public object CoinMarketCapLink { get; set; }
    public object Algorithm { get; set; }
    public bool Tradeable { get; set; }
}

public class SummaryProfile
{
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string Zip { get; set; }
    public string Country { get; set; }
    public string Phone { get; set; }
    public string Website { get; set; }
    public string Industry { get; set; }
    public string IndustryKey { get; set; }
    public string IndustryDisp { get; set; }
    public string Sector { get; set; }
    public string SectorKey { get; set; }
    public string SectorDisp { get; set; }
    public string LongBusinessSummary { get; set; }
    public int FullTimeEmployees { get; set; }
    public List<object> CompanyOfficers { get; set; }
    public int MaxAge { get; set; }
}
