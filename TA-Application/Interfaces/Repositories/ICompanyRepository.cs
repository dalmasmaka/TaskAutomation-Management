using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface ICompanyRepository
    {
        Task<Company> AddWorkspace(string workspace, string userId, CancellationToken ct);
        int GetCompanyByInvitationCode(string invitationCode, CancellationToken ct);
        Task<WorkspaceInvitations> GetInvitationByInvitationCode(string invitationCode, CancellationToken ct);
        Task<bool> UpdateCompanyAsync(Company company, CancellationToken ct);
        Task<CompanyIntegrationConfig> SaveCompanyConfigAsync(CompanyIntegrationConfig dto, CancellationToken ct);
        Task<Company> GetCompanyById(int id, CancellationToken ct);
        Task<List<CompanyIntegrationConfig>> GetCompanyConfig(int companyId, CancellationToken ct);
        Task<List<(ApplicationUser User, string RoleName)>> GetTeamOfCompany(int companyId, CancellationToken ct);
        Task<List<WorkspaceInvitations>> GetInvitations(int companyId, CancellationToken ct);
        Task<WorkspaceInvitations> GenerateCodeAsync(WorkspaceInvitations dto, CancellationToken ct);
        Task<bool> DeleteConfigAsync(int id, CancellationToken ct);
        Task<bool> DeleteCompanyAsync(int companyId, CancellationToken ct);
        Task<WorkspaceInvitations> GetInvitationByCode(string code, CancellationToken ct);

    }
}
