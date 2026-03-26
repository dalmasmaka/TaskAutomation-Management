using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using TA_Domain.Entities;

namespace TA_Infrastructure.Persistence
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TA_Domain.Entities.Task> Tasks { get; set; }
        public DbSet<CodeGeneration> CodeGenerations { get; set; }
        public DbSet<PullRequest> PullRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CompanyIntegrationConfig> CompanyIntegrationConfigs { get; set; }
        public DbSet<ProjectIntegration> ProjectIntegrations { get; set; }


    }
}
