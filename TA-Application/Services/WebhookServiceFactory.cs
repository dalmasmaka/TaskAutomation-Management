using Microsoft.Extensions.DependencyInjection;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class WebhookServiceFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ICompanyConfigRepository _configRepo;


        public WebhookServiceFactory(
            IServiceProvider serviceProvider,
             ICompanyConfigRepository configRepo
            )
        {
            _serviceProvider = serviceProvider;
            _configRepo = configRepo;
        }


        public async Task<IWebhookService> GetAsync(string provider, int companyId)
        {
            var companyConfig = await _configRepo
                .GetByCompanyAndProviderAsync(companyId, provider)
                ?? throw new KeyNotFoundException("Config not found");

            return provider switch
            {
                "Jira" => CreateJira(companyConfig),
                "Trello" => CreateTrello(companyConfig),
                _ => throw new NotSupportedException()
            };
        }
        private IWebhookService CreateTrello(CompanyIntegrationConfig config)
        {
            var service = new TrelloWebhookService(
                _serviceProvider.GetRequiredService<HttpClient>(),
                _serviceProvider.GetRequiredService<IWebhookRepository>()
            );

            service.Initialize(config);
            return service;
        }

        private IWebhookService CreateJira(CompanyIntegrationConfig config)
        {
            var service = new JiraWebhookService(
                _serviceProvider.GetRequiredService<HttpClient>(),
                _serviceProvider.GetRequiredService<IWebhookRepository>()
            );

            service.Initialize(config);
            return service;
        }
    }
}
