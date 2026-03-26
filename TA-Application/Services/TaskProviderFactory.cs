using Microsoft.Extensions.DependencyInjection;
using TA_Application.Interfaces;

namespace TA_Application.Services
{
    public class TaskProviderFactory
    {
        private readonly IProjectIntegrationService _integrationRepo;
        private readonly ICompanyIntegrationConfigService _configRepo;
        private readonly IServiceProvider _serviceProvider;

        public TaskProviderFactory(
            IProjectIntegrationService integrationRepo,
            ICompanyIntegrationConfigService configRepo,
            IServiceProvider serviceProvider)
        {
            _integrationRepo = integrationRepo;
            _configRepo = configRepo;
            _serviceProvider = serviceProvider;
        }

        public async Task<ITaskProviderService> GetProvider(string provider, int companyId)
        {
            var companyConfig = await _configRepo.GetByCompanyAndProviderAsync(companyId, provider);

            return provider.ToLower() switch
            {
                "jira" => new JiraProvider(
                    _serviceProvider.GetRequiredService<IJiraHttpClient>(),
                    _integrationRepo,
                    companyConfig),
                "trello" => new TrelloProvider(
                    _serviceProvider.GetRequiredService<ITrelloHttpClient>(),
                    _integrationRepo,
                    companyConfig),
                _ => throw new Exception("Provider not supported")
            };
        }
    }
}
