using System.ComponentModel.DataAnnotations.Schema;

namespace TA_Application.DTOs
{
    public class ReviewDTO
    {
        public int ReviewId { get; set; }
        public int TaskId { get; set; }
        public int PRId { get; set; }
        public string? Comment { get; set; }
        public string? Decision { get; set; }
        public string? UserId { get; set; }
        public string? Reviewer {  get; set; }
        public string? Developer { get; set; }
        public DateTime ReviewDate { get; set; }
        public string? DeveloperId { get; set; }
    }
    public class ReviewRequestDTO
    {
        public int TaskId { get; set; }
        public int ProjectId { get; set; }
        public int PrID { get; set; }
        //public int ExternalId { get; set; }
        public string Comment { get; set; }
        public string Decision { get; set; }
        public string? UserId { get; set; }

    }

}
