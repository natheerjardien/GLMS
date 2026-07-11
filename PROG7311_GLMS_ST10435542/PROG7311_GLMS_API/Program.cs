using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using PROG7311_GLMS_API.Data;
using PROG7311_GLMS_API.Repositories;
using PROG7311_GLMS_API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity lives in the API now - the frontend authenticates over HTTP and gets a JWT back
builder.Services.AddIdentityCore<IdentityUser>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// JWT bearer authentication: every protected endpoint expects "Authorization: Bearer <token>"
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

// Repository pattern: the only layer that touches the DbContext
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IContractRepository, ContractRepository>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();

// Service layer: business rules, reusing the GoF patterns from Parts 1 & 2
builder.Services.AddScoped<IContractFactory, ContractFactory>();          // Factory pattern
builder.Services.AddScoped<IContractStateManager, ContractStateManager>();// State pattern
builder.Services.AddHttpClient<ICurrencyConversionStrategy, LiveExchangeRateStrategy>(); // Strategy pattern
builder.Services.AddScoped<IPricingService, PricingService>();
builder.Services.AddScoped<IContractService, ContractService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<ITokenService, TokenService>();

// Swagger/OpenAPI with JWT support so the API is self-documenting AND testable when secured
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GLMS Backend API",
        Version = "v1",
        Description = "Service layer for the Global Logistics Management System. The MVC frontend consumes this API over HTTP - it is the only component that talks to the database."
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste the JWT from POST /api/auth/login here."
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer", document, null), new List<string>() }
    });
});

var app = builder.Build();

// Apply migrations and seed the default logins on startup. The retry loop matters in Docker,
// where the SQL Server container can take a while to accept connections.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    if (db.Database.IsRelational())
    {
        var retries = 10;
        while (true)
        {
            try
            {
                db.Database.Migrate();
                break;
            }
            catch (Exception ex) when (retries-- > 0)
            {
                app.Logger.LogWarning("Database not ready yet ({Message}). Retrying in 5s ({Retries} attempts left)...", ex.Message, retries);
                await Task.Delay(TimeSpan.FromSeconds(5));
            }
        }
    }

    await DbInitializer.SeedRolesAndUsers(scope.ServiceProvider);
}

// Swagger stays enabled in every environment so the containerised API is easy to demonstrate
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "GLMS API V1");
    c.RoutePrefix = string.Empty; // Swagger UI served at the root of the API
});

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();

namespace PROG7311_GLMS_API
{
    public partial class Program { }
}
