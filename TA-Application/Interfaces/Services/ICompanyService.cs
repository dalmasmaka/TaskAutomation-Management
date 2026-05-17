using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Enums;

namespace TA_Application.Interfaces.Services
{
    public interface ICompanyService
    {
        Task<ResponseDTO>CheckInvitationCode(string invitationCode);
        Task<CompanyDTO> UpdateCompany(CompanyDTO companyDTO);
        Task<CompanyConfigDTO> SaveCompanyConfig(CompanyConfigDTO companyConfigDTO);
        Task<CompanyDTO> GetCompanyById(int id);
        Task<List<CompanyConfigDTO>> GetCompanyConfig(int companyId);
        Task<List<UserDTO>>GetTeamOfCompany(int companyId);
        Task<List<InvitationsDTO>>GetInvitations(int companyId);
        string GenerateSecureCode(int length = 8);
        Task<InvitationsDTO> GenerateCodeAsync(int companyId, DateTime expiryDate, CompanyRole role);
        Task<ResponseDTO> DeleteConfig(int id);
        Task<ResponseDTO> DeleteCompanyAsync(int companyId);
        Task<InvitationsDTO> GetInvitationByCode(string code);
    }
}
