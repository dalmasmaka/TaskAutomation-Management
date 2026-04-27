using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Reflection.Emit;
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
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Company Owner relationship
            builder.Entity<Company>()
                .HasOne(c => c.User)
                .WithMany(u => u.OwnedCompanies)
                .HasForeignKey(c => c.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Company Employees relationship
            builder.Entity<Company>()
                .HasMany(c => c.Users)
                .WithOne(u => u.Company)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // AuditEntity CreatedBy relationship
            builder.Entity<Company>()
                .HasOne(c => c.CreatedBy)
                .WithMany() // Creator can have many entities or leave blank
                .HasForeignKey(c => c.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

            // AuditEntity UpdatedBy relationship
            builder.Entity<Company>()
                .HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<UserProject>(entity =>
            {
                // Define the Primary Key (if not using a composite key)
                entity.HasKey(up => up.Id);

                // Relationship: User -> UserProjects
                entity.HasOne(up => up.User)
                    .WithMany(u => u.UserProjects)
                    .HasForeignKey(up => up.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relationship: Project -> UserProjects
                entity.HasOne(up => up.Project)
                    .WithMany(p => p.UserProjects) // Ensure this matches the property name in Project.cs
                    .HasForeignKey(up => up.ProjectId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(c => c.CreatedBy)
              .WithMany() // Creator can have many entities or leave blank
              .HasForeignKey(c => c.CreatedById)
              .OnDelete(DeleteBehavior.Restrict);

                // AuditEntity UpdatedBy relationship
                entity.HasOne(c => c.UpdatedBy)
                    .WithMany()
                    .HasForeignKey(c => c.UpdatedById)
                    .OnDelete(DeleteBehavior.Restrict);

            });
            builder.Entity<TaskMember>()
        .HasOne(tm => tm.User)
        .WithMany(u => u.TaskMembers)
        .HasForeignKey(tm => tm.UserId)
        .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskMember>()
                .HasOne(tm => tm.Task)
                .WithMany(t => t.TaskMembers)
                .HasForeignKey(tm => tm.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TaskMember>().HasOne(c => c.CreatedBy)
              .WithMany() // Creator can have many entities or leave blank
              .HasForeignKey(c => c.CreatedById)
              .OnDelete(DeleteBehavior.Restrict);

            // AuditEntity UpdatedBy relationship

            builder.Entity<TaskMember>().HasOne(c => c.UpdatedBy)
                .WithMany()
                .HasForeignKey(c => c.UpdatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<TA_Domain.Entities.Task> Tasks { get; set; }
        public DbSet<CodeGeneration> CodeGenerations { get; set; }
        public DbSet<PullRequest> PullRequests { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<WorkspaceInvitations> WorkspaceInvitations { get; set; }
        public DbSet<CompanyIntegrationConfig> CompanyIntegrationConfigs { get; set; }
        public DbSet<ProjectIntegration> ProjectIntegrations { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<TaskMember> TaskMembers { get; set; }


    }
}
