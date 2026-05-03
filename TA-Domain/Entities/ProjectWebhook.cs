using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace TA_Domain.Entities
{
    public class ProjectWebhook: AuditEntity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public required int ProjectId { get; set; }
        public required string Provider { get; set; }
        public required string ExternalWebhookId { get; set; }
        public string? CallbackUrl { get; set; }
    }
}
