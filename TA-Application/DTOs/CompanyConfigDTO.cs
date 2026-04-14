using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using TA_Domain.Entities;

namespace TA_Application.DTOs
{
    public class CompanyConfigDTO
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public string? Provider { get; set; }
        public string? ClientId { get; set; }
        public string? ClientSecret { get; set; }
        public string? RedirectUri { get; set; }
        public string? ExternalWorkspaceId { get; set; }
    }
}
