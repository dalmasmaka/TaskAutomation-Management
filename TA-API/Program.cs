using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using System.Text;
using TA_API.Seeding;
using TA_Application.Configurations;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Application.Services;
using TA_Domain.Entities;
using TA_Infrastructure.HttpClients;
using TA_Infrastructure.Persistence;
using TA_Infrastructure.Repositories;
using TA_Infrastructure.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular",
        policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // optional but good
        });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
var jwtSecret = builder.Configuration["Jwt:Secret"];
if (string.IsNullOrWhiteSpace(jwtSecret))
{
    throw new InvalidOperationException("Configuration value 'Jwt:Secret' is required and cannot be null or empty.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.FromMinutes(5),
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),

        //RoleClaimType = "role",
        NameClaimType = "sub"
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            // Check the logs/console for the specific reason (Expired? Invalid Signature?)
            Console.WriteLine("Authentication Failed: " + context.Exception.Message);
            return System.Threading.Tasks.Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("Token Validated Successfully!");
            return System.Threading.Tasks.Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();
builder.Services.AddControllers();
//builder.Services.AddHostFiltering(options =>
//{
//    options.AllowedHosts.Clear();
//    options.AllowedHosts.Add("*");
//});
// In Program.cs
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
builder.Services.PostConfigure<Microsoft.AspNetCore.HostFiltering.HostFilteringOptions>(options =>
{
    options.AllowedHosts = new[] { "*" };
});
builder.Services.Configure<GroqSettings>(builder.Configuration.GetSection("Groq"));
builder.Services.AddEndpointsApiExplorer();

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
builder.Services.Configure<ProvidersSettings>(builder.Configuration.GetSection("Providers"));

builder.Services.AddHttpClient<IJiraHttpClient, JiraHttpClient>();
builder.Services.AddHttpClient<ITrelloHttpClient, TrelloHttpClient>();
builder.Services.AddScoped<IProjectIntegrationRepository, ProjectIntegrationRepository>();
builder.Services.AddScoped<ICompanyConfigRepository, CompanyIntegrationConfigRepository>();

builder.Services.AddScoped<TaskProviderFactory>();

builder.Services.AddScoped<IAppUserService, ApplicationUserService>();
builder.Services.AddScoped<IAppUserRepository, ApplicationUserRepository>();

builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

builder.Services.AddScoped<ITasksService, TasksService>(); 
builder.Services.AddScoped<ITasksRepository, TasksRepository>();

builder.Services.AddScoped<IGitHubClientFactory, GitHubClientFactory>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<IGitHubConfigRepository, GitHubConfigRepository>();
builder.Services.AddScoped<IPullRequestService, PullRequestService>();
builder.Services.AddScoped<IPullRequestRepository, PullRequestRepository>();

builder.Services.AddScoped<IDashboardService, DashboardService>();

builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
builder.Services.AddScoped<IJwtService, JwtService>();

builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddHostedService<JiraTokenRefreshService>();

builder.Services.AddScoped<WebhookServiceFactory>();

builder.Services.AddHttpClient<TrelloWebhookService>();
builder.Services.AddHttpClient<JiraWebhookService>();
builder.Services.AddHttpClient<ICodeGenerationService, CodeGenerationService>();

builder.Services.AddScoped<IWebhookRepository, WebhookRepository>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddOptions();

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
builder.Services.Configure<DataProtectionTokenProviderOptions>(options =>
{
    options.TokenLifespan = TimeSpan.FromMinutes(30);
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    await RoleSeeder.SeedAsync(roleManager);
}

//app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.UseCors("AllowAngular");

app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}




app.Run();