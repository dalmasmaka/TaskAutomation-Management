namespace TA_Application.DTOs
{
    public class PullRequestDTO
    {
        public int PRId { get; set; }
        public int CodeGenId { get; set; }
        public required CodeGenerationDTO CodeGeneration { get; set; }
        public required string BranchName { get; set; }
        public required string Status { get; set; } 
                                                   
        public ICollection<ReviewDTO>? Reviews { get; set; }
    }
}
