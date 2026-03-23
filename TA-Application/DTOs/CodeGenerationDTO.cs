namespace TA_Application.DTOs
{
    public class CodeGenerationDTO
    {
        public int CodeGenId { get; set; }
        public required int TaskId { get; set; }
        public required TaskDTO Task { get; set; }
        public required string GeneratedCode { get; set; }
        public string? FilePath { get; set; }
        public required string Status { get; set; } 

        public PullRequestDTO? PullRequest { get; set; }
    }
}
