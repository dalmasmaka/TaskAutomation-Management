using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface ITrelloHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri);
        Task<string> ExchangeTokenAsync(string token); // Trello returns token directly
        Task<TaskDTO[]> GetTasksAsync(string accessToken, string boardId, string clientId);
    }
}
