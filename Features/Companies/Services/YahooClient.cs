using Microsoft.Net.Http.Headers;
using System.Collections.Concurrent;

namespace SuperInvestor.Features.Companies.Services;

public class YahooClient
{
    private static readonly SemaphoreSlim _sessionLock = new SemaphoreSlim(1, 1);
    private static HttpClient cookieClient;
    private string crumb;
    private DateTime lastCookieRenewal = DateTime.MinValue;
    private static readonly TimeSpan CookieRenewalInterval = TimeSpan.FromHours(1);
    
    // Throttling to avoid rate limits
    private static readonly ConcurrentDictionary<string, DateTime> _lastRequestTimes = new ConcurrentDictionary<string, DateTime>();
    private static readonly TimeSpan _minRequestInterval = TimeSpan.FromSeconds(2);

    private async Task Init(bool forceRenewal = false)
    {
        // Use a semaphore to prevent multiple threads from initializing at the same time
        await _sessionLock.WaitAsync();
        try
        {
            // Skip if we've already initialized and don't need to renew yet
            if (cookieClient != null && !forceRenewal && DateTime.UtcNow - lastCookieRenewal < CookieRenewalInterval)
            {
                return;
            }

            var cookieClientHandler = new HttpClientHandler();
            cookieClient = new HttpClient(cookieClientHandler)
            {
                Timeout = TimeSpan.FromSeconds(30),
            };

            cookieClientHandler.AllowAutoRedirect = true;
            cookieClientHandler.UseCookies = true;
            cookieClientHandler.CookieContainer = new System.Net.CookieContainer();

            // Modern Chrome user agent (as of 2024)
            cookieClient.DefaultRequestHeaders.Add(
                HeaderNames.UserAgent,
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36");
            
            // Add common browser headers to appear more legitimate
            cookieClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.9");
            cookieClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8");
            cookieClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "same-origin");
            cookieClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            cookieClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            cookieClient.DefaultRequestHeaders.Add("Sec-Ch-Ua", "\"Chromium\";v=\"121\", \"Not A(Brand\";v=\"99\"");
            cookieClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Mobile", "?0");
            cookieClient.DefaultRequestHeaders.Add("Sec-Ch-Ua-Platform", "\"Windows\"");

            // First get the cookies from a Yahoo domain
            var response = await cookieClient.GetAsync("https://fc.yahoo.com/");
            if (!response.IsSuccessStatusCode)
            {
                // Try alternative Yahoo domains if the first one fails
                response = await cookieClient.GetAsync("https://finance.yahoo.com/");
            }

            // Then get the authentication crumb
            // await Task.Delay(500); // Small delay between requests
            response = await cookieClient.GetAsync("https://query1.finance.yahoo.com/v1/test/getcrumb");
            if (response.IsSuccessStatusCode)
            {
                crumb = await response.Content.ReadAsStringAsync();
            }
            else
            {
                // Try backup method to get crumb
                response = await cookieClient.GetAsync("https://finance.yahoo.com/quote/AAPL");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // Extract crumb using a simple method - in production you might use a more robust regex
                    var crumbIndex = content.IndexOf("\"CrumbStore\":{\"crumb\":\"");
                    if (crumbIndex > 0)
                    {
                        var startIndex = crumbIndex + 23;
                        var endIndex = content.IndexOf("\"}", startIndex);
                        if (endIndex > startIndex)
                        {
                            crumb = content.Substring(startIndex, endIndex - startIndex);
                        }
                    }
                }
            }

            lastCookieRenewal = DateTime.UtcNow;
        }
        finally
        {
            _sessionLock.Release();
        }
    }

    public async Task<YahooQuote> GetAsync(string ticker)
    {
        if (cookieClient is null || DateTime.UtcNow - lastCookieRenewal > CookieRenewalInterval)
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

        // Implement request throttling
        await ThrottleRequest(ticker);

        // Use primary endpoint
        var url = $"https://query2.finance.yahoo.com/v10/finance/quoteSummary/{ticker}?modules=summaryProfile,summaryDetail&corsDomain=finance.yahoo.com&formatted=false&symbol={ticker}&crumb={crumb}";
        
        try
        {
            var response = await cookieClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<YahooQuote>();
            }
            
            // If we got rate limited or auth issues, try refreshing cookies
            if ((int)response.StatusCode == 429 || (int)response.StatusCode == 401)
            {
                await Init(forceRenewal: true);
                
                // Try once more with renewed cookies
                response = await cookieClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<YahooQuote>();
                }
            }
        }
        catch (Exception)
        {
            // Fail silently
        }

        return default;
    }
    
    private async Task ThrottleRequest(string key)
    {
        // Get the time of the last request for this key
        if (_lastRequestTimes.TryGetValue(key, out var lastRequestTime))
        {
            var timeSinceLastRequest = DateTime.UtcNow - lastRequestTime;
            
            // If we've made a request too recently, wait until the minimum interval has passed
            if (timeSinceLastRequest < _minRequestInterval)
            {
                var delayTime = _minRequestInterval - timeSinceLastRequest;
                await Task.Delay(delayTime);
            }
        }
        
        // Update the last request time for this key
        _lastRequestTimes[key] = DateTime.UtcNow;
    } 
}

public class YahooQuote
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
