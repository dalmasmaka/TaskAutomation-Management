namespace TA_Application.DTOs
{

    public class PullRequestDTO
    {
        public long Id { get; set; }
        public int TaskId { get; set; }
        public string BranchName { get; set; }
        public string PRUrl { get; set; }
        public string PRName { get; set; }
        public string? PRDescription { get; set; }
        public string? Status { get; set; }
        public long ExternalId { get; set; }

    }

    public class PullRequestListDTO
    {
        public int PrId { get; set; }
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public string CreatedById { get; set; }
        public string BranchName { get; set; }
        public string PRUrl { get; set; }
        public string PRName { get; set; }
        public string? PRDescription { get; set; }
        public string? Status { get; set; }
        public string? CreatedBy { get; set;  }
    }
    public class PullRequestCreationDTO
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public string RootFolder { get; set; }
        public List<GeneratedFileDTO> Files { get; set; }
    }
    public class RepoDTO
    {
        public string RepoUrl { get; set; } 
        public string RepoOwner { get; set; }
    }
    public class PRQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public List<int>? ProjectIds { get; set; }
        public List<int>? TaskIds { get; set; }
        public List<string>? CreatedByIds { get; set; }
        public string? Search { get; set; }
        public string? Status { get; set; }
    }
}
