using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using SuperInvestor.Features.App.Components;
using SuperInvestor.Features.Common.Data;
using SuperInvestor.Features.Common.Services;
using SuperInvestor.Features.Identity.Data;
using SuperInvestor.Features.Companies.Services;
using SuperInvestor.Features.Identity.Services;
using SuperInvestor.Features.Notes.Services;
using SuperInvestor.Features.Research.Services;
using SuperInvestor.Features.Subscription.Services;
using SuperInvestor.Features.UI.Services;

var builder = WebApplication.CreateBuilder(args);

// Validate all required environment variables
EnvironmentHelper.ValidateEnvironmentVariables();

// Configure Serilog
var logLevel = builder.Environment.IsDevelopment() ? LogEventLevel.Information : LogEventLevel.Warning;
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Is(logLevel) // Information in dev, Warning in production
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error) // Override Microsoft logs to Error
    .WriteTo.Console() // Log to console
    .WriteTo.PostgreSQL(EnvironmentHelper.ConnectionString, "Logs", needAutoCreateTable: true) // Log to database
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = EnvironmentHelper.GoogleClientId;
        googleOptions.ClientSecret = EnvironmentHelper.GoogleClientSecret;
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseNpgsql(EnvironmentHelper.ConnectionString)
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddOptions();
builder.Services.AddScoped<IEmailSender<ApplicationUser>, EmailSender>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<YahooClient>();
builder.Services.AddScoped<IHtmlCleanerService, HtmlCleanerService>();
builder.Services.AddScoped<IFilingEventService, FilingEventService>();
builder.Services.AddScoped<INoteHighlightService, NoteHighlightService>();
builder.Services.AddScoped<ICompanyTickerService, CompanyTickerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
builder.Services.AddScoped<IToastService, ToastService>();
builder.Services.AddScoped<IShareService, ShareService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IResearchService, ResearchService>();
builder.Services.AddScoped<IFilingCategoryService, FilingCategoryService>();
builder.Services.AddHttpClient<TurnstileService>();
builder.Services.AddScoped<TurnstileService>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

Stripe.StripeConfiguration.ApiKey = EnvironmentHelper.StripeApiKey;

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    await using (var dbContext = await dbContextFactory.CreateDbContextAsync())
    {
        await dbContext.Database.MigrateAsync();
    }

    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
    await userService.MakeUserAdminAsync("aldo.leka@live.com");
}

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error/500", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/Error/{0}");
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();
app.MapControllers();
app.MapGet("/healthcheck", () => Results.Ok(new { status = "ok" }));

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

Log.Information("Application started. Listening on {Urls}", app.Urls);
app.Run();
