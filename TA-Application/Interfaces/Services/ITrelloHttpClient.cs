using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface ITrelloHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId);
        Task<List<Board>> GetBoardsAsync(string accessToken, string cloudId);
        Task<List<TaskDTO>> GetTrelloTasks(string accessToken, string boardId, string apiKey, int projectId);
    }
}
