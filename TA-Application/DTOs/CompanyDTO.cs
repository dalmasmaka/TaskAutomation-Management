
using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class CompanyDTO
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required string OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; }
    }
}
