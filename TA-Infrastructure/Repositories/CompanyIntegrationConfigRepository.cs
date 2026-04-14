using TA_Domain.Entities;
using TA_Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TA_Application.Interfaces.Repositories;

namespace TA_Infrastructure.Repositories
{
    public class CompanyIntegrationConfigRepository : ICompanyConfigRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyIntegrationConfigRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CompanyIntegrationConfig?> GetByCompanyAndProviderAsync(int companyId, string provider)
        {
           return await _context.CompanyIntegrationConfigs
                .Where(c => c.CompanyId == companyId && c.Provider.ToLower() == provider.ToLower())
                .FirstOrDefaultAsync();
        }
    }
}
