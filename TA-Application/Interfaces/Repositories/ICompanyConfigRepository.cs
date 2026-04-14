using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface ICompanyConfigRepository
    {
        Task<CompanyIntegrationConfig?> GetByCompanyAndProviderAsync(int companyId, string provider);
        //System.Threading.Tasks.Task SaveAsync(CompanyIntegrationConfig config);
    }
}
