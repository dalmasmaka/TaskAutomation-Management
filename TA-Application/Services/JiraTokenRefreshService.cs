using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TA_Application.Interfaces.Services;
using TA_Application.Interfaces.Repositories;

namespace TA_Application.Services
{

    public class JiraTokenRefreshService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public JiraTokenRefreshService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await RefreshTokens(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task RefreshTokens(CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetRequiredService<IProjectIntegrationRepository>();
            var http = scope.ServiceProvider.GetRequiredService<IJiraHttpClient>();
            var configRepo = scope.ServiceProvider.GetRequiredService<ICompanyConfigRepository>();

            var integrations = await repo.GetExpiringJiraTokens(ct);

            foreach (var integration in integrations)
            {
                try
                {
                    var config = await configRepo.GetByCompanyAndProviderAsync(
                        integration.Project.CompanyId, "Jira");

                    var token = await http.RefreshTokenAsync(
                        integration.RefreshToken!,
                        config.ClientId!,
                        config.ClientSecret!
                    );

                    integration.AccessToken = token.AccessToken;
                    integration.RefreshToken = token.RefreshToken;
                    integration.ExpiresAt = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

                    await repo.SaveAsync(integration, ct);
                }
                catch (Exception ex)
                {
                    // 🔴 VERY IMPORTANT
                    // mark as expired or log
                }
            }
        }
    }
}
