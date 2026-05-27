using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Domain.Entities
{
    public class Project: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProjectId { get; set; }
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public  Company? Company { get; set; }
        [Required, MaxLength(200)]
        public required string Name { get; set; }
        public ICollection<Task>? Tasks { get; set; }
        public ICollection<ProjectIntegration>? ProjectIntegrations { get; set; }
        public ICollection<UserProject>? UserProjects { get; set; }
        public ICollection<GitHubConfig>? RepositoryConfigs { get; set; }
        public ICollection<ProjectTeam> ProjectTeams { get; set; } = new List<ProjectTeam>();

    }
}
