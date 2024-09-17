using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Resend;
using Serilog;
using Serilog.Events;
using Stripe;
using SuperInvestor.Components;
using SuperInvestor.Components.Account;
using SuperInvestor.Data;
using SuperInvestor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Warning() // Set minimum level to Warning
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error) // Override Microsoft logs to Error
    .WriteTo.PostgreSQL(builder.Configuration.GetConnectionString("DefaultConnection"), "Logs", needAutoCreateTable: true)
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
        googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
        googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
    })
    .AddIdentityCookies();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString), ServiceLifetime.Transient);
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(o =>
{
    o.ApiToken = builder.Configuration["Resend:ApiKey"];
});
builder.Services.AddTransient<IResend, ResendClient>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, ResendEmailSender>();

builder.Services.AddHttpClient();
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<YahooClient>();
builder.Services.AddScoped<HtmlCleanerService>();
builder.Services.AddScoped<FilingEventService>();
builder.Services.AddScoped<NoteHighlightService>();
builder.Services.AddScoped<CompanyTickerService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<SuperInvestor.Services.SubscriptionService>();
builder.Services.AddScoped<ToastService>();
builder.Services.AddScoped<ShareService>();
builder.Services.AddScoped<NoteService>();
builder.Services.AddScoped<ResearchService>();
builder.Services.AddScoped<FilingCategoryService>();

builder.Services.AddControllers();

StripeConfiguration.ApiKey = builder.Configuration["StripeApiKey"];

var app = builder.Build();

// Run this once to create the admin role and assign it to your user
using (var scope = app.Services.CreateScope())
{
    var userService = scope.ServiceProvider.GetRequiredService<UserService>();
    await userService.CreateAdminRole();
    await userService.MakeUserAdmin("aldo.leka@live.com");
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

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseStatusCodePagesWithRedirects("/Error/{0}");
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapAdditionalIdentityEndpoints();
app.MapControllers();

app.Lifetime.ApplicationStopped.Register(Log.CloseAndFlush);

app.Run();