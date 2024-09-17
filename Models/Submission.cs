using System.Text.Json.Serialization;

namespace SuperInvestor.Models;

public class Submission
{
    public string Cik { get; set; }

    public string Name { get; set; }

    [JsonPropertyName("sicDescription")]
    public string Description { get; set; }

    public string[] Tickers { get; set; }

    public string[] Exchanges { get; set; }

    public Addresses Addresses { get; set; }

    public string Phone { get; set; }

    public Filings Filings { get; set; }
}

public class Addresses
{
    public Address Mailing { get; set; }
    public Address Business { get; set; }
}

public class Address
{
    public string Street1 { get; set; }
    public string Street2 { get; set; }
    public string City { get; set; }
    public string StateOrCountry { get; set; }
    public string ZipCode { get; set; }
}

public class Filings
{
    public FilingInfo Recent { get; set; }
}

public class FilingInfo
{
    public string[] AccessionNumber { get; set; }
    public string[] FilingDate { get; set; }
    public string[] Form { get; set; }
    public string[] PrimaryDocument { get; set; }
}