using Microsoft.EntityFrameworkCore;
using PROG7311_GLMS_ST10435542.Data;
using PROG7311_GLMS_ST10435542.Services;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection"); // Reads the connection string from appsettings.json

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString)); // Configures to use the ConnectionString

// this sets up the identity system for logins and makes the password rules easier for testing
builder.Services.AddDefaultIdentity<IdentityUser>(options => { // for the logins
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>() // adds role management
.AddEntityFrameworkStores<ApplicationDbContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IContractFactory, ContractFactory>(); // this registers the factory pattern service
builder.Services.AddHttpClient<ICurrencyConversionStrategy, LiveExchangeRateStrategy>(); // this registers the live currency api service and lets it use the internet
builder.Services.AddScoped<IContractStateManager, ContractStateManager>();// this registers the state pattern manager service
builder.Services.AddScoped<ICurrencyConversionStrategy, LiveExchangeRateStrategy>();
builder.Services.AddScoped<IPricingService, PricingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// vital for the login system to check who the user is and what they can do
app.UseAuthentication(); // for the logins
app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope()) // runs the dbinitializer to make sure the accounts are created at startup
{
    await DbInitializer.SeedRolesAndUsers(scope.ServiceProvider);
}

app.Run();
