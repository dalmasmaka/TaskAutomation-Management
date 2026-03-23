namespace TA_Application.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int PRId { get; set; }
        public required PullRequestDTO PullRequest { get; set; }
        public string? Comment { get; set; }
        public required string Decision { get; set; }
    }
}
