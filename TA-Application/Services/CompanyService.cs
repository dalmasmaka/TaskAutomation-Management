using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;
using TA_Domain.Enums;

namespace TA_Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyService;
        private readonly IAppUserRepository _appUserRepository;
        private readonly ILogger<CompanyService> _logger;
        public CompanyService(ICompanyRepository companyService, IAppUserRepository appUserRepository, ILogger<CompanyService> logger)
        {
            _companyService = companyService;
            _appUserRepository = appUserRepository;
            _logger = logger;

        }
        public async Task<ResponseDTO> CheckInvitationCode(string invitationCode, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(invitationCode))
            {
                return new ResponseDTO { Success = false, Message = "Invitation code cannot be empty." };
            }
            try
            {
                var invitation = await _companyService.GetInvitationByInvitationCode(invitationCode, ct);
                if (invitation == null)
                {
                    return new ResponseDTO { Success = false, Message = "Invalid invitation code. Cannot register your account" };
                }
                if (invitation.ExpirationDate < DateTime.UtcNow)
                {
                    return new ResponseDTO { Success = false, Message = "Invitation code has expired. Cannot register your account" };
                }
                return new ResponseDTO { Success = true, Message = "Invitation code is valid." };
            }
            catch (Exception ex)
            {
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }
        public async Task<CompanyDTO> UpdateCompany(CompanyDTO companyDTO, CancellationToken ct)
        {
            if (companyDTO == null)
                throw new ArgumentException("Company is null");

            try
            {
                var entity = new Company
                {
                    Id = companyDTO.Id,
                    Name = companyDTO.Name,
                    OwnerId = companyDTO.OwnerId,
                };
                await _companyService.UpdateCompanyAsync(entity, ct);
                return companyDTO;
               
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyDTO));
            }
        }
        public async Task<CompanyConfigDTO> SaveCompanyConfig(CompanyConfigDTO companyConfigDTO, CancellationToken ct)
        {
            if (companyConfigDTO == null)
                throw new ArgumentException("Company Configuration is null");
            try
            {
                var entity = new CompanyIntegrationConfig
                {
                    Id = companyConfigDTO.Id,
                    CompanyId = companyConfigDTO.CompanyId,
                    Provider = companyConfigDTO.Provider,
                    ClientId = companyConfigDTO.ClientId,
                    ClientSecret = companyConfigDTO.ClientSecret,
                    RedirectUri = companyConfigDTO.RedirectUri
                };
                await _companyService.SaveCompanyConfigAsync(entity, ct);
                return companyConfigDTO;
                
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyConfigDTO));
            }
        }
        public async Task<CompanyDTO> GetCompanyById(int id, CancellationToken ct)
        {
            try
            {
                var company = await _companyService.GetCompanyById(id, ct);
                return new CompanyDTO
                {
                    Id = company.Id,
                    Name = company.Name,
                    OwnerId = company.OwnerId,
                    OwnerName = company.User.FullName
                    
                };
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(id));
            }
        }

        public async Task<List<CompanyConfigDTO>> GetCompanyConfig(int companyId, CancellationToken ct)
        {
            if (companyId <= 0)
                throw new ArgumentException("Invalid Company Id", nameof(companyId));

            try
            {
                var companyConfigs = await _companyService.GetCompanyConfig(companyId, ct);

                return companyConfigs.Select(companyConfig => new CompanyConfigDTO
                {
                    Id = companyConfig.Id,
                    CompanyId = companyConfig.CompanyId,
                    Provider = companyConfig.Provider,
                    ClientId = companyConfig.ClientId,
                    ClientSecret = companyConfig.ClientSecret,
                    RedirectUri = companyConfig.RedirectUri
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving company configs", ex);
            }
        }

        public async Task<List<UserDTO>> GetTeamOfCompany(int companyId, CancellationToken ct)
        {
            if (companyId == 0) throw new ArgumentException(nameof(companyId));
            try
            {
                var result = await _companyService.GetTeamOfCompany(companyId, ct);
                return result.Select(user => new UserDTO
                {
                    Id = user.User.Id,
                    FullName = user.User.FullName,
                    UserName = user.User.UserName,   
                    PhoneNumber = user.User.PhoneNumber,
                    Role = user.RoleName,
                    Email = user.User.Email
                }).ToList();
            }
            catch (Exception ex)
            {
                throw new ArgumentException(ex.Message, nameof(companyId));
            }
        }
        public async Task<InvitationsDTO> GetInvitationByCode(string code, CancellationToken ct)
        {
            try
            {
                var result = await _companyService.GetInvitationByCode(code, ct);
                return new InvitationsDTO
                {
                    Id = result.Id,
                    InvitationCode = result.InvitationCode,
                    StringRole = result.Role.ToString(),
                    CompanyId = result.CompanyId,
                    Role = result.Role,
                    ExpirationDate = result.ExpirationDate,
                };
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company invitations");
                throw;
            }
        }

        public async Task<List<InvitationsDTO>> GetInvitations(int companyId, CancellationToken ct)
        {
            try
            {
                var result = await _companyService.GetInvitations(companyId, ct);

                return result.Select(inv => new InvitationsDTO
                {
                    CompanyId = inv.CompanyId,
                    InvitationCode = inv.InvitationCode ?? string.Empty,
                    ExpirationDate = inv.ExpirationDate,
                    ExpirationStatus = inv.ExpirationDate > DateTime.UtcNow
                        ? "Active"
                        : "Expired",
                    Role = inv.Role,
                    StringRole = inv.Role.ToString()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching company invitations");
                return new List<InvitationsDTO>();
            }
        }
        public string GenerateSecureCode(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var data = new byte[length];

            using var crypto = RandomNumberGenerator.Create();
            crypto.GetBytes(data);

            return new string(data.Select(b => chars[b % chars.Length]).ToArray());
        }

        public async Task<InvitationsDTO> GenerateCodeAsync(int companyId, DateTime expiryDate, CompanyRole role, CancellationToken ct)
        {
            try
            {
                var generatedCode = GenerateSecureCode();
                var dto = new WorkspaceInvitations
                {
                    CompanyId = companyId,
                    ExpirationDate = expiryDate,
                    InvitationCode = generatedCode,
                    Role = role
                    
                };
                var code =  await _companyService.GenerateCodeAsync(dto, ct);
                var updatedList = new InvitationsDTO
                {
                    CompanyId = code.CompanyId,
                    ExpirationDate = code.ExpirationDate,
                    InvitationCode = code.InvitationCode,
                    ExpirationStatus = code.ExpirationDate > DateTime.UtcNow
                        ? "Active"
                        : "Expired",
                    Role = code.Role
                };
                return updatedList;
            }
            catch (Exception ex) {
                _logger.LogError(ex, "Error fetching company invitations");
                throw;
            }
        }

        public async Task<ResponseDTO> DeleteConfig(int id, CancellationToken ct)
        {
            if (id == 0) throw new ArgumentNullException(nameof(id));
            try
            {
                var existingUser = await _companyService.GetCompanyConfig(id, ct);
                if (existingUser == null)
                {
                    return new ResponseDTO { Success = false, Message = "Company config does not exist." };
                }
                await _companyService.DeleteConfigAsync(id, ct);
                return new ResponseDTO { Success = true, Message = "Configuration deleted successfully." };
            }
            catch (Exception ex)
            {
               _logger.LogError(ex, "Unexpected error deleting user {UserId}.", id);
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }

        public async Task<ResponseDTO> DeleteCompanyAsync(int companyId, CancellationToken ct)
        {
            if (companyId == 0) throw new ArgumentNullException(nameof(companyId));

            try
            {
                var existingCom = await _companyService.GetCompanyById(companyId, ct);

                if (existingCom == null)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Company does not exist."
                    };
                }

                await _appUserRepository.DeleteUsersByCompanyIdAsync(companyId, ct);

                await _companyService.DeleteCompanyAsync(companyId, ct);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Company and related users deleted successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting company.");
                return new ResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                };
            }
        }
    }
}
