using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public interface IProjectIntegrationService
    {
        Task<ProjectIntegrationDTO?> GetByProjectIdAsync(int projectId);
        System.Threading.Tasks.Task SaveAsync(ProjectIntegrationDTO integration);
    }
}
