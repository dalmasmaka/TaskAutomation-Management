using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;
using TA_Infrastructure.Persistence.Migrations;

namespace TA_Infrastructure.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;
        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectDTO> SaveAsync(ProjectDTO project)
        {
            var existing = await _context.Projects
                .FirstOrDefaultAsync(pi => pi.ProjectId == project.ProjectId);

            if (existing != null)
            {
                // Update existing record
                existing.Name = project.Name;
                existing.Platform = project.Platform;
                existing.RepoURl = project.RepoURl;
                existing.CompanyId = project.CompanyId;
                existing.UpdatedAt = DateTime.Now;

                // No need to call .Update() if the entity was tracked via FirstOrDefaultAsync
                await _context.SaveChangesAsync();

                // Map back to DTO to return
                project.ProjectId = existing.ProjectId;
                return project;
            }
            else
            {
                // Create new record
                var newProject = new Project
                {
                    Name = project.Name,
                    Platform = project.Platform,
                    RepoURl = project.RepoURl,
                    CompanyId = project.CompanyId,
                    CreatedAt = DateTime.Now // Good practice to set this here
                };

                await _context.Projects.AddAsync(newProject);
                await _context.SaveChangesAsync();

                // After SaveChangesAsync, newProject.ProjectId is populated by the DB
                project.ProjectId = newProject.ProjectId;
                return project;
            }
        }

        public async System.Threading.Tasks.Task DeleteAsync(int id)
        {
            var existing = await _context.Projects
                 .FirstOrDefaultAsync(p => p.ProjectId == id);

            if (existing != null)
            {
                existing.IsDeleted = true;

                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<ProjectDTO>> GetAllAsync(int companyId)
        {
            return await _context.Projects
                .Where(p => !p.IsDeleted && p.CompanyId == companyId)
                .OrderByDescending(d => d.ProjectId)
                .Select(p => new ProjectDTO
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                    Platform = p.Platform,
                    RepoURl = p.RepoURl,
                    CreatedAt = p.CreatedAt.ToString("dd MMMM yyyy"),
                    CreatedBy = p.CreatedBy.FullName,
                    // Navigate to the join table and count the records
                    TotalUsers = p.UserProjects.Count(up => !up.IsDeleted),
                    TotalTasks = p.Tasks.Count(t => !t.IsDeleted)
                })
                .ToListAsync();
        }

        public async Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId)
        {
            return await _context.Projects
                .Where(p => !p.IsDeleted && p.CompanyId == companyId)
                .Where(p => !p.UserProjects.Any(up => up.UserId == userId))
                .Select(p => new ProjectDTO
                {
                    ProjectId = p.ProjectId,
                    Name = p.Name,
                })
                .ToListAsync();
        }
        public async Task<List<UserDTO>> GetUsersAvailableToAssignToProjects(int companyId, int projectId)
        {
            var users = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                where u.CompanyId == companyId
                      && !u.isDeleted
                      && r.Name != "Owner"
                      && !u.UserProjects.Any(up => up.ProjectId == projectId && !up.IsDeleted)
                select new UserDTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email
                }
            ).Distinct().ToListAsync();
            return users;
        }

        public async System.Threading.Tasks.Task AssignUserToProjects(string userId, List<int> projectIds)
        {

            foreach (var projectId in projectIds)
            {
                var exists = await _context.UserProjects
                    .AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);

                if (!exists)
                {
                    _context.UserProjects.Add(new UserProject
                    {
                        UserId = userId,
                        ProjectId = projectId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }
        public async System.Threading.Tasks.Task AssignUsersToProject(int projectId, List<string> userIds)
        {

            foreach (var userId in userIds)
            {
                var exists = await _context.UserProjects
                    .AnyAsync(up => up.UserId == userId && up.ProjectId == projectId);

                if (!exists)
                {
                    _context.UserProjects.Add(new UserProject
                    {
                        UserId = userId,
                        ProjectId = projectId
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<UserDTO>> GetMembersAsync(int projectId)
        {
            var users = await (
                from up in _context.UserProjects
                join user in _context.Users on up.UserId equals user.Id
                join userRole in _context.UserRoles on user.Id equals userRole.UserId
                join role in _context.Roles on userRole.RoleId equals role.Id
                where up.ProjectId == projectId && up.IsDeleted == false && user.isDeleted == false
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

        public async System.Threading.Tasks.Task RemoveMemberAsync(string userId, int projectId)
        {
            var existingMember = await _context.UserProjects.Where(m => m.UserId == userId && m.ProjectId == projectId).FirstOrDefaultAsync();
            if (existingMember != null)
            {
                existingMember.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
