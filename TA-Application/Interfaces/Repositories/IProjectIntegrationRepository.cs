using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IProjectIntegrationRepository
    {
        Task<ProjectIntegrationDTO?> GetByProjectIdAsync(int projectId);
        Task SaveAsync(ProjectIntegrationDTO integration);
        Task SaveTasksAsync(List<TaskDTO> tasks);
    }
}
