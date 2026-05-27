using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface ITrelloHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId);
        Task<List<Board>> GetBoardsAsync(string accessToken, string cloudId);
        Task<List<TA_Domain.Entities.Task>> GetTrelloTasks(string accessToken, string boardId, string apiKey, int projectId);
    }
}
