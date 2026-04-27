using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface ITaskProviderService
    {
        Task<string> GetAuthUrlAsync(int projectId, int companyId);
        System.Threading.Tasks.Task AuthorizeAndSaveAsync(string code, int projectId);
        //Task<TaskDTO[]> GetTasksAsync(int projectId);
        Task<List<Board>> GetBoardsAsync(int projectId);
        System.Threading.Tasks.Task SaveBoardSelectionAsync(int projectId, string boardId, string key);
        Task<ResponseDTO> SaveTasks(int projectId);
    }
}