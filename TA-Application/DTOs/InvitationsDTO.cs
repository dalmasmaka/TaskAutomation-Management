using TA_Domain.Enums;
namespace TA_Application.DTOs
{
    public class InvitationsDTO
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string? InvitationCode { get; set; }
        public string? StringRole { get; set; }
        public DateTime ExpirationDate { get; set; }
        public string? ExpirationStatus { get; set; }
        public CompanyRole Role { get; set; }
       
    }
    public class GenerateCodeDTO
    {
        public DateTime ExpirationDate { get; set; }
    }
}
