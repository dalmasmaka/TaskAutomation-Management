using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly ApplicationDbContext _context;
        public CompanyRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<CompanyDTO> AddWorkspace(string workspace, string userId)
        {
            if (string.IsNullOrWhiteSpace(workspace))
                throw new ArgumentException("Workspace name cannot be null or empty.", nameof(workspace));

            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("User Id cannot be null or empty.", nameof(userId));

            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                    throw new Exception($"User with ID {userId} not found.");

                var company = new Company
                {
                    Name = workspace,
                    OwnerId = userId,
                    User = user,
                    CreatedBy = user,
                    UpdatedBy = user,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                var companyDto = new CompanyDTO
                {
                    Id = company.Id,
                    Name = company.Name,
                    OwnerId = company.OwnerId,


                };

                return companyDto;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the company: {ex.Message}", ex);
            }
        }
        public int GetCompanyByInvitationCode(string invitationCode)
        {
            try
            {
                var companyId = _context.Companies
                    .Where(c => c.WorkspaceInvitations.Any(i => i.InvitationCode == invitationCode))
                    .Select(c => c.Id)
                    .FirstOrDefault();

                return companyId;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving company by invitation code: {ex.Message}", ex);
            }
        }

        public async Task<InvitationsDTO> GetInvitationsByInvitationCode(string invitationCode)
        {
            var invitation = await _context.WorkspaceInvitations
                .Where(i => i.InvitationCode == invitationCode)
                .Select(i => new InvitationsDTO
                {
                    Id = i.Id,
                    CompanyId = i.CompanyId,
                    InvitationCode = i.InvitationCode,
                    ExpirationDate = i.ExpirationDate,

                })
                .FirstOrDefaultAsync();
            if (invitation == null) { throw new Exception($"Invitation with code '{invitationCode}' not found."); }
            return invitation;
        }

        public async System.Threading.Tasks.Task UpdateCompanyAsync(CompanyDTO companyDTO)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyDTO.Id);

            if (company == null)
                throw new Exception($"Company with ID {companyDTO.Id} not found.");

            var user = await _context.Users.FindAsync(companyDTO.OwnerId);
            if (user == null)
                throw new Exception($"User with ID {companyDTO.OwnerId} not found.");

            company.Name = companyDTO.Name;
            company.OwnerId = companyDTO.OwnerId;
            company.UpdatedBy = user;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }
        public async System.Threading.Tasks.Task SaveCompanyConfigAsync(CompanyConfigDTO dto)
        {
            var companyExists = await _context.Companies
                .AnyAsync(c => c.Id == dto.CompanyId);

            if (!companyExists)
                throw new ArgumentException("Company does not exist", nameof(dto.CompanyId));

            var existingConfig = await _context.CompanyIntegrationConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == dto.CompanyId);

            if (existingConfig == null)
            {
                var newConfig = new CompanyIntegrationConfig
                {
                    CompanyId = dto.CompanyId,
                    ClientId = dto.ClientId,
                    ClientSecret = dto.ClientSecret,
                    Provider = dto.Provider,
                    RedirectUri = dto.RedirectUri,
                    ExternalWorkspaceId = dto.ExternalWorkspaceId,
                };

                await _context.CompanyIntegrationConfigs.AddAsync(newConfig);
            }
            else
            {
                existingConfig.ClientId = dto.ClientId;
                existingConfig.ClientSecret = dto.ClientSecret;
                existingConfig.Provider = dto.Provider;
                existingConfig.RedirectUri = dto.RedirectUri;
                existingConfig.ExternalWorkspaceId = dto.ExternalWorkspaceId;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<CompanyDTO> GetCompanyById(int id)
        {
            var company = await _context.Companies
         .Include(c => c.User)
         .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return null;
            var dto = new CompanyDTO
            {
                Id = company.Id,
                Name = company.Name,
                OwnerId = company.OwnerId,
                OwnerName = company.User?.FullName,
            };
            return dto;
        }

        public async Task<CompanyConfigDTO> GetCompanyConfig(int companyId)
        {
            var companyConfig = await _context.CompanyIntegrationConfigs.Where(c => c.CompanyId == companyId).FirstOrDefaultAsync();
            if (companyConfig == null)
                return null;
            var dto = new CompanyConfigDTO
            {
                CompanyId = companyConfig.CompanyId,
                Id = companyConfig.Id,
                ClientId = companyConfig.ClientId,
                ClientSecret = companyConfig.ClientSecret,
                Provider = companyConfig.Provider,
                RedirectUri = companyConfig.RedirectUri,
                ExternalWorkspaceId = companyConfig.ExternalWorkspaceId,
            };
            return dto;
        }

        public async Task<List<UserDTO>> GetTeamOfCompany(int companyId)
        {
            var users = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where user.CompanyId == companyId && user.isDeleted == false
                select new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    Role = role.Name
                }
            ).ToListAsync();

            return users;
        }

        public async Task<List<InvitationsDTO>> GetInvitations(int companyId)
        {
            return await _context.WorkspaceInvitations
                .Where(i => i.CompanyId == companyId)
                .OrderByDescending(i => i.Id)
                .Select(i => new InvitationsDTO
                {
                    Id = i.Id,
                    InvitationCode = i.InvitationCode,
                    CompanyId = i.CompanyId,
                    ExpirationDate = i.ExpirationDate
                })
                .ToListAsync();
        }

        public async Task<InvitationsDTO> GenerateCodeAsync(InvitationsDTO dto)
        {
            var invitaton = new WorkspaceInvitations
            {
                CompanyId = dto.CompanyId,
                InvitationCode = dto.InvitationCode,
                ExpirationDate = dto.ExpirationDate,

            };
            _context.WorkspaceInvitations.Add(invitaton);
            await _context.SaveChangesAsync();
            return dto = new InvitationsDTO
            {
                CompanyId = dto.CompanyId,
                InvitationCode = dto.InvitationCode,
                ExpirationDate = dto.ExpirationDate,
            };
        }
    }
}
