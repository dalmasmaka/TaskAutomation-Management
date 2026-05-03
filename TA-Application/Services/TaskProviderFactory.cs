using Microsoft.Extensions.DependencyInjection;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;

namespace TA_Application.Services
{
    public class TaskProviderFactory
    {
        private readonly IProjectIntegrationRepository _integrationRepo;
        private readonly ICompanyConfigRepository _configRepo;
        private readonly IServiceProvider _serviceProvider;
        private readonly WebhookServiceFactory _webhookFactory;

        public TaskProviderFactory(
            IProjectIntegrationRepository integrationRepo,
            ICompanyConfigRepository configRepo,
            IServiceProvider serviceProvider,
            WebhookServiceFactory webhookFactory)
        {
            _integrationRepo = integrationRepo;
            _configRepo = configRepo;
            _serviceProvider = serviceProvider;
            _webhookFactory = webhookFactory;
        }


        public async Task<ITaskProviderService> GetProvider(string provider, int companyId)
        {
            var companyConfig = await _configRepo.GetByCompanyAndProviderAsync(companyId, provider) ?? throw new KeyNotFoundException("Config not found");

            return provider.ToLower() switch
            {
                "jira" => new JiraProvider(
                    _serviceProvider.GetRequiredService<IJiraHttpClient>(),
                    _integrationRepo,
                    companyConfig,
                    _webhookFactory),
                "trello" => new TrelloProvider(
                    _serviceProvider.GetRequiredService<ITrelloHttpClient>(),
                    _integrationRepo,
                    companyConfig,
                    _webhookFactory),
                _ => throw new Exception("Provider not supported")
            };
        }
    }
}
