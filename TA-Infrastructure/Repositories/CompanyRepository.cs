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
        public async Task<Company> AddWorkspace(string workspace, string userId, CancellationToken ct)
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
                return company;
               
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while adding the company: {ex.Message}", ex);
            }
        }
        public int GetCompanyByInvitationCode(string invitationCode, CancellationToken ct)
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

        public async Task<WorkspaceInvitations> GetInvitationByInvitationCode(string invitationCode, CancellationToken ct)
        {
            var invitation = await _context.WorkspaceInvitations
                .Where(i => i.InvitationCode == invitationCode).FirstOrDefaultAsync(ct);
            if (invitation == null) { throw new Exception($"Invitation with code '{invitationCode}' not found."); }
            return invitation;
        }

        public async Task<bool> UpdateCompanyAsync(Company companyDTO, CancellationToken ct)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(c => c.Id == companyDTO.Id);

            if (company == null)
                return false;

            var user = await _context.Users.FindAsync(companyDTO.OwnerId);
            if (user == null)
                throw new Exception($"User with ID {companyDTO.OwnerId} not found.");

            company.Name = companyDTO.Name;
            company.OwnerId = companyDTO.OwnerId;
            company.UpdatedBy = user;
            company.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<CompanyIntegrationConfig> SaveCompanyConfigAsync(CompanyIntegrationConfig entity,  CancellationToken ct)
        {
            var existingConfig = await _context.CompanyIntegrationConfigs
                .FirstOrDefaultAsync(c => c.CompanyId == entity.CompanyId, ct);

            if (existingConfig == null)
            {
                await _context.CompanyIntegrationConfigs.AddAsync(entity, ct);
                await _context.SaveChangesAsync(ct);
                return entity;
            }

            existingConfig.Provider = entity.Provider;
            existingConfig.ClientId = entity.ClientId;
            existingConfig.ClientSecret = entity.ClientSecret;
            existingConfig.RedirectUri = entity.RedirectUri;

            await _context.SaveChangesAsync(ct);

            return existingConfig;
        }
        public async Task<Company> GetCompanyById(int id, CancellationToken ct)
        {
            var dto = await _context.Companies
                 .Include(c => c.User)
                .Where(c => c.Id == id)
                .FirstOrDefaultAsync(ct);

            ArgumentNullException.ThrowIfNull(dto);

            return dto;
        }
        public async Task<List<CompanyIntegrationConfig>> GetCompanyConfig(int companyId, CancellationToken ct)
        {
            return await _context.CompanyIntegrationConfigs.Where(c => c.CompanyId == companyId && !c.IsDeleted).ToListAsync(ct);
        }

        public async Task<List<(ApplicationUser User, string RoleName)>> GetTeamOfCompany(int companyId, CancellationToken ct)
        {
            var result = await (
                from user in _context.Users
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where user.CompanyId == companyId && !user.isDeleted
                select new
                {
                    User = user,
                    RoleName = role.Name
                }
            ).ToListAsync(ct);

            return result
                .Select(x => (x.User, x.RoleName))
                .ToList();
        }

        public async Task<List<WorkspaceInvitations>> GetInvitations(int companyId, CancellationToken ct)
        {
            return await _context.WorkspaceInvitations
                .Where(i => i.CompanyId == companyId)
                .OrderByDescending(i => i.Id)
                .ToListAsync(ct); 

           
        }

        public async Task<WorkspaceInvitations> GenerateCodeAsync(WorkspaceInvitations dto, CancellationToken ct)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            _context.WorkspaceInvitations.Add(dto);
            await _context.SaveChangesAsync();
            return dto;
           
        }

        public async Task<bool> DeleteConfigAsync(int id, CancellationToken ct)
        {
            var existing = await _context.CompanyIntegrationConfigs
               .FirstOrDefaultAsync(p => p.Id == id, ct);

            if (existing == null)
            {
                return false;
            }
            existing.IsDeleted = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteCompanyAsync(int companyId, CancellationToken ct)
        {
            var company = await GetCompanyById(companyId, ct);
            if (company == null) {
                return false;
            }
            company.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<WorkspaceInvitations> GetInvitationByCode(string code, CancellationToken ct)
        {
            return await _context.WorkspaceInvitations.Where(t=> t.InvitationCode == code).FirstOrDefaultAsync(ct);
        }
    }
}
