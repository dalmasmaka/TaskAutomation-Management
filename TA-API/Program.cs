using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using TA_API.Seeding;
using TA_Application.Configurations;
using TA_Application.Interfaces;
using TA_Application.Services;
using TA_Domain.Entities;
using TA_Infrastructure.HttpClients;
using TA_Infrastructure.Persistence;
using TA_Infrastructure.Repositories;
using TA_Infrastructure.Services;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TA-API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: 12345abcde"
    });
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.AddHttpContextAccessor();
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<TaskProvidersSettings>(
    builder.Configuration.GetSection("Providers:Jira"));
//builder.Services.Configure<JiraSettings>(
//    builder.Configuration.GetSection("Providers:Trello"));
// 1. Register the HttpClients (using AddHttpClient is best for Jira/Trello)
builder.Services.AddHttpClient<IJiraHttpClient, JiraHttpClient>();
builder.Services.AddHttpClient<ITrelloHttpClient, TrelloHttpClient>();

// 2. Register your Infrastructure/Application Services
builder.Services.AddScoped<IProjectIntegrationService, ProjectIntegrationRepository>();
builder.Services.AddScoped<ICompanyIntegrationConfigService, CompanyIntegrationConfigRepository>();

// 3. Register the Factory itself
builder.Services.AddScoped<TaskProviderFactory>();
builder.Services.AddScoped<ApplicationUserService>();
builder.Services.AddScoped<IApplicationUserService, ApplicationUserRepository>();
builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
}
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

app.Run();