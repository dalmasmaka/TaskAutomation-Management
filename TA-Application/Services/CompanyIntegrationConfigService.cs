using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class CompanyIntegrationConfigService
    {
        private readonly ICompanyConfigRepository _repository;
        private readonly ILogger<CompanyIntegrationConfig> _logger;
        public CompanyIntegrationConfigService(ICompanyConfigRepository repository, ILogger<CompanyIntegrationConfig> logger)
        {
            _repository = repository;
            _logger = logger;
        }
        public async Task<CompanyIntegrationConfig?> GetByCompanyAndProvider(int companyId, string provider)
        {
            if(companyId < 0 && string.IsNullOrWhiteSpace(provider)) throw new ArgumentException("Invalid companyId or provider");
            try
            {
                var result = await _repository.GetByCompanyAndProviderAsync(companyId, provider);
                if(result == null) throw new Exception("CompanyIntegrationConfig not found");
                return result;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error getting company integration config for company {CompanyId}, provider {Provider}", companyId, provider);
                throw;
            }

        }
    }
}
