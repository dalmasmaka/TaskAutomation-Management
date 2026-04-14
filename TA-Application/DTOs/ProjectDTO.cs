namespace TA_Application.DTOs
{
    public class ProjectDTO
    {
        public int ProjectId { get; set; }

        public string? Name { get; set; }
        public string? Platform { get; set; }
        public string? RepoURl { get; set; }
        public int CompanyId { get; set; }
        public string CreatedBy { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTasks { get; set; }
        public string? CreatedAt { get; set; }

    }
    public class AssignProjectUsersDTO
    {
        public int ProjectId { get; set; }
        public List<string> Users { get; set; } = new();
    }
    public class AssignUserProjectsDTO
    {
        public string? UserId { get; set; }
        public List<int> Projects { get; set; } = new();
    }
    public class WholeProjectDTO

    {
        public int ProjectId { get; set; }
        public required string Name { get; set; }
        public required string Platform { get; set; }
        public string? RepoURl { get; set; }
        public string? Provider { get; set; }

        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }

        public string? ExternalId { get; set; }

        public DateTime? ExpiresAt { get; set; }
    }
}
