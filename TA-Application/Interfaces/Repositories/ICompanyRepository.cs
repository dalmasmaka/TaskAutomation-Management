using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface ICompanyRepository
    {
        Task<CompanyDTO> AddWorkspace(string workspace, string userId);
        int GetCompanyByInvitationCode(string InvitationCode);
        Task<InvitationsDTO> GetInvitationsByInvitationCode(string invitationCode);
        Task UpdateCompanyAsync(CompanyDTO company);
        Task<CompanyConfigDTO> SaveCompanyConfigAsync(CompanyConfigDTO dto);
        Task<CompanyDTO> GetCompanyById(int id);
        Task<List<CompanyConfigDTO>> GetCompanyConfig(int companyId);
        Task<List<UserDTO>> GetTeamOfCompany(int companyId);
        Task<List<InvitationsDTO>> GetInvitations(int companyId);
        Task<InvitationsDTO> GenerateCodeAsync(InvitationsDTO dto);
        Task DeleteConfigAsync(int id);
        Task DeleteCompanyAsync(int companyId);

    }
}
