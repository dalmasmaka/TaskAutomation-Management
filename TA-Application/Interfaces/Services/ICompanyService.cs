using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Enums;

namespace TA_Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ResponseDTO>CheckInvitationCode(string invitationCode, CancellationToken ct);
        Task<CompanyDTO> UpdateCompany(CompanyDTO companyDTO, CancellationToken ct);
        Task<CompanyConfigDTO> SaveCompanyConfig(CompanyConfigDTO companyConfigDTO, CancellationToken ct);
        Task<CompanyDTO> GetCompanyById(int id, CancellationToken ct);
        Task<List<CompanyConfigDTO>> GetCompanyConfig(int companyId, CancellationToken ct);
        Task<List<UserDTO>>GetTeamOfCompany(int companyId, CancellationToken ct);
        Task<List<InvitationsDTO>>GetInvitations(int companyId, CancellationToken ct);
        string GenerateSecureCode(int length = 8);
        Task<InvitationsDTO> GenerateCodeAsync(int companyId, DateTime expiryDate, CompanyRole role, CancellationToken ct);
        Task<ResponseDTO> DeleteConfig(int id, CancellationToken ct);
        Task<ResponseDTO> DeleteCompanyAsync(int companyId, CancellationToken ct);
        Task<InvitationsDTO> GetInvitationByCode(string code, CancellationToken ct);
    }
}
