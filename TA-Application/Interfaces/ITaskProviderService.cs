using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces
{
    public interface ITaskProviderService
    {
        Task<string> GetAuthUrlAsync();
        System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId);
        Task<TaskDTO[]> GetTasksAsync(int projectId);
    }
}