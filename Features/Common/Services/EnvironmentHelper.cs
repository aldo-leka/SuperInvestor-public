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
    
    // Resend Email
    public static string ResendApiKey => GetEnvironmentVariable("RESEND_API_KEY");
    public static string ResendSenderEmail => GetEnvironmentVariable("RESEND_SENDER_EMAIL");
    public static string ResendSenderName => GetEnvironmentVariable("RESEND_SENDER_NAME");
    
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
            "RESEND_API_KEY",
            "RESEND_SENDER_EMAIL",
            "RESEND_SENDER_NAME",
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