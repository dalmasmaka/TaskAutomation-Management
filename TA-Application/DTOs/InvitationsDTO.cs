using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class InvitationsDTO
    {
        public int Id { get; set; }
        public  int CompanyId { get; set; }
        public  string InvitationCode { get; set; }
        public  DateTime ExpirationDate { get; set; }
        public string ExpirationStatus { get; set; }
    }
    public class GenerateCodeDTO { 
        public DateTime ExpirationDate { get; set; }
    }
}
