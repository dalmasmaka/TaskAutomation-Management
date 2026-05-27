using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITaskProviderService
    {
        Task<string> GetAuthUrlAsync(int projectId, int companyId);
        System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId, CancellationToken ct);
        //Task<TaskDTO[]> GetTasksAsync(int projectId);
        Task<List<Board>> GetBoardsAsync(int projectId, CancellationToken ct);
        System.Threading.Tasks.Task SaveBoardSelectionAsync(int projectId, string boardId, string key, CancellationToken ct);
        Task<bool> SaveTasks(int projectId, CancellationToken ct);
    }
}