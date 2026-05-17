using Microsoft.EntityFrameworkCore.Metadata.Internal;
using TA_Domain.Enums;

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
    public class CodeGenerationRequestDTO
    {
        public required string Description { get; set; }
    }
    public class GeneratedFileDTO
    {
        public string FileName { get; set; }
        public string Content { get; set; }
    }

    public class GeneratedFolderDTO
    {
        public string FolderName { get; set; }
        public List<GeneratedFileDTO> Files { get; set; }
    }

    public class GeneratedResponse
    {
        public List<GeneratedFileDTO> Files { get; set; }
    }
    public class GenerateInvitationDTO
    {
        public DateTime ExpiryDate { get; set; }
        public CompanyRole Role { get; set; }
    }
}