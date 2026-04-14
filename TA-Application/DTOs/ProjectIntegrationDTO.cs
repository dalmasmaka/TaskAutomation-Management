using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class ProjectIntegrationDTO
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public Project? Project { get; set; }

        public string? Provider { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string? ExternalId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
