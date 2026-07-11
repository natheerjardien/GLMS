using Microsoft.AspNetCore.Authentication.Cookies;
using PROG7311_GLMS_ST10435542.Services.ApiClients;

// PART 3: this frontend is fully decoupled from the database.
// There is no DbContext, no Entity Framework and no SQL Server dependency here -
// every piece of data comes from the GLMS Web API over HTTP.
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Cookie auth for the browser session. The cookie carries the user's roles plus the JWT
// issued by the API; the BearerTokenHandler forwards that JWT on every API call.
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<BearerTokenHandler>();

// In Docker this becomes http://glms-backend-api:8080/ via the ApiSettings__BaseUrl env variable
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5187/";
if (!apiBaseUrl.EndsWith('/'))
{
    apiBaseUrl += "/";
}

void ConfigureApiClient(HttpClient client) => client.BaseAddress = new Uri(apiBaseUrl);

builder.Services.AddHttpClient<AuthApiClient>(ConfigureApiClient).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<ClientApiClient>(ConfigureApiClient).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<ContractApiClient>(ConfigureApiClient).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<ServiceRequestClient>(ConfigureApiClient).AddHttpMessageHandler<BearerTokenHandler>();
builder.Services.AddHttpClient<UsersApiClient>(ConfigureApiClient).AddHttpMessageHandler<BearerTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
