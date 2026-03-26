using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public interface ICompanyIntegrationConfigService
    {
        Task<CompanyIntegrationConfig?> GetByCompanyAndProviderAsync(int companyId, string provider);
        //System.Threading.Tasks.Task SaveAsync(CompanyIntegrationConfig config);
    }
}
