namespace TA_Application.DTOs
{
    public class ProjectDTO
    {
        public int ProjectId { get; set; }
        public required string Name { get; set; }
        public required string Platform { get; set; }
        public string? RepoURl { get; set; }
        public ICollection<TaskDTO>? Tasks { get; set; }

    }
}
