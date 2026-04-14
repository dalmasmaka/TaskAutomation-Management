using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectIntegrationRepository
    {
        Task<ProjectIntegrationDTO?> GetByProjectIdAsync(int projectId);
        System.Threading.Tasks.Task SaveAsync(ProjectIntegrationDTO integration);
    }
}
