using System.Collections.Concurrent;
using System.Text;

namespace SuperInvestor.Features.Common.Services;

public static class EnvironmentHelper
{
    // Google Authentication
    public static string GoogleClientId => GetEnvironmentVariable("GOOGLE_CLIENT_ID");
    public static string GoogleClientSecret => GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");
    
    // Turnstile (Cloudflare)
    public static string TurnstileSiteKey => GetEnvironmentVariable("TURNSTILE_SITE_KEY");
    public static string TurnstileSecretKey => GetEnvironmentVariable("TURNSTILE_SECRET_KEY");

    // Email
    public static string SenderEmailAddress => GetEnvironmentVariable("SENDER_EMAIL_ADDRESS");
    public static string SmtpHost => GetEnvironmentVariable("SMTP_HOST");
    public static int SmtpPort => int.Parse(GetEnvironmentVariable("SMTP_PORT"));
    public static string SmtpUsername => GetEnvironmentVariable("SMTP_USERNAME");
    public static string SmtpPassword => GetEnvironmentVariable("SMTP_PASSWORD");
    public static bool SmtpUseSsl => bool.Parse(GetEnvironmentVariable("SMTP_USE_SSL"));

    // Stripe
    public static string StripeApiKey => GetEnvironmentVariable("STRIPE_API_KEY");
    public static string StripePriceId => GetEnvironmentVariable("STRIPE_PRICE_ID");
    public static string StripeWebHookSecret => GetEnvironmentVariable("STRIPE_WEBHOOK_SECRET");
    
    // Database
    public static string ConnectionString => GetEnvironmentVariable("CONNECTION_STRING");
    
    // Cache of environment variables to avoid repeated lookups
    private static readonly ConcurrentDictionary<string, string> Cache = new();
    
    // Helper method to get environment variable with caching
    private static string GetEnvironmentVariable(string key)
    {
        return Cache.GetOrAdd(key, k => Environment.GetEnvironmentVariable(k) 
            ?? throw new InvalidOperationException($"Required environment variable '{k}' is not set."));
    }
    
    public static void ValidateEnvironmentVariables()
    {
        var missingVariables = new List<string>();
        var requiredVariables = new[]
        {
            "GOOGLE_CLIENT_ID",
            "GOOGLE_CLIENT_SECRET",
            "TURNSTILE_SITE_KEY",
            "TURNSTILE_SECRET_KEY",
            "SENDER_EMAIL_ADDRESS",
            "SMTP_HOST",
            "SMTP_PORT",
            "SMTP_USERNAME",
            "SMTP_PASSWORD",
            "SMTP_USE_SSL",
            "STRIPE_API_KEY",
            "STRIPE_PRICE_ID",
            "STRIPE_WEBHOOK_SECRET",
            "CONNECTION_STRING"
        };
        
        foreach (var variable in requiredVariables)
        {
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(variable)))
            {
                missingVariables.Add(variable);
            }
        }
        
        if (missingVariables.Count > 0)
        {
            var message = new StringBuilder("Missing required environment variables:");
            foreach (var variable in missingVariables)
            {
                message.AppendLine($"- {variable}");
            }
            
            throw new InvalidOperationException(message.ToString());
        }
    }
}