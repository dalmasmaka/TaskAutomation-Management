namespace TA_Domain.Entities
{
    public class AuditEntity
    {
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public ApplicationUser? CreatedBy { get; set; }
        public ApplicationUser? UpdatedBy { get; set; }
        public bool IsDeleted { get; set; } = false;

    }
}
