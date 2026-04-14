using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITaskProviderService
    {
        Task<string> GetAuthUrlAsync();
        System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId);
        Task<TaskDTO[]> GetTasksAsync(int projectId);
    }
}