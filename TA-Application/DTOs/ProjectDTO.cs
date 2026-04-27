namespace TA_Application.DTOs
{
    public class ProjectDTO
    {
        public int ProjectId { get; set; }

        public string? Name { get; set; }
        public string? Platform { get; set; }
        public string? RepoURl { get; set; }
        public int CompanyId { get; set; }
        public string? CreatedBy { get; set; }
        public int TotalUsers { get; set; }
        public int TotalTasks { get; set; }
        public string? CreatedAt { get; set; }
        public bool IsAssigned { get; set; }
        public bool HasConfig { get; set; }
    }
    public class CreateProjectDTO
    {
        public int ProjectId { get; set; }
        public string? Name { get; set; }
        public string? Platform { get; set; }
        public string? RepoURl { get; set; }
        public int CompanyId { get; set; }
        public DateTime? CreatedTime { get; set; }
        public string? UserId { get; set; }

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

}
