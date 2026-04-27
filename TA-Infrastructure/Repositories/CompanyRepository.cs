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
                    .Where(c => c.WorkspaceInvitations!.Any(i => i.InvitationCode == invitationCode))
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
        public async Task<CompanyConfigDTO> SaveCompanyConfigAsync(CompanyConfigDTO dto)
        {
            var existingConfig = await _context.CompanyIntegrationConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == dto.CompanyId && c.Id == dto.Id);

            if (existingConfig == null)
            {
                var newConfig = new CompanyIntegrationConfig
                {
                    CompanyId = dto.CompanyId,
                    ClientId = dto.ClientId,
                    ClientSecret = dto.ClientSecret,
                    Provider = dto.Provider,
                    RedirectUri = dto.RedirectUri,
                };

                await _context.CompanyIntegrationConfigs.AddAsync(newConfig);
                await _context.SaveChangesAsync();
                dto.Id = newConfig.Id;
            }
            else
            {
                existingConfig.ClientId = dto.ClientId;
                existingConfig.ClientSecret = dto.ClientSecret;
                existingConfig.Provider = dto.Provider;
                existingConfig.RedirectUri = dto.RedirectUri;
                dto.Id = existingConfig.Id;
                await _context.SaveChangesAsync();
            }
           
            return dto;

        }
        public async Task<CompanyDTO> GetCompanyById(int id)
        {
            var dto = await _context.Companies
                .Where(c => c.Id == id)
                .Select(c => new CompanyDTO
                {
                    Id = c.Id,
                    Name = c.Name,
                    OwnerId = c.OwnerId,
                    OwnerName = c.User != null ? c.User.FullName : null
                })
                .FirstOrDefaultAsync();

            ArgumentNullException.ThrowIfNull(dto);

            return dto;
        }
        public async Task<List<CompanyConfigDTO>> GetCompanyConfig(int companyId)
        {
            return await _context.CompanyIntegrationConfigs.Where(c => c.CompanyId == companyId && !c.IsDeleted)
                .Select(p => new CompanyConfigDTO
                {
                    CompanyId = p.CompanyId,
                    Id = p.Id,
                    ClientId = p.ClientId,
                    ClientSecret = p.ClientSecret,
                    Provider = p.Provider,
                    RedirectUri = p.RedirectUri,
                }).ToListAsync();
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
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            var invitation = new WorkspaceInvitations
            {
                CompanyId = dto.CompanyId,
                InvitationCode = dto.InvitationCode,
                ExpirationDate = dto.ExpirationDate
            };

            _context.WorkspaceInvitations.Add(invitation);
            await _context.SaveChangesAsync();

            return new InvitationsDTO
            {
                CompanyId = invitation.CompanyId,
                InvitationCode = invitation.InvitationCode,
                ExpirationDate = invitation.ExpirationDate
            };
        }

        public async System.Threading.Tasks.Task DeleteConfigAsync(int id)
        {
            var existing = await _context.CompanyIntegrationConfigs
               .FirstOrDefaultAsync(p => p.Id == id);

            if (existing != null)
            {
                existing.IsDeleted = true;

                await _context.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task DeleteCompanyAsync(int companyId)
        {
            var company = await GetCompanyById(companyId);
            if (company != null) {
                company.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
