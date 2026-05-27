using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IJiraHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId);
        Task<JiraTokenResponseDTO> ExchangeCodeForTokenAsync(string code, int projectId, string clientId, string clientSecret, string redirectUri);
        Task<string> GetCloudIdAsync(string accessToken);
        Task<List<TA_Domain.Entities.Task>> GetJiraTasksAsync(string accessToken, string cloudId, string projectKey, int localProjectId);
        Task<List<Board>> GetBoardsAsync(string accessToken, string cloudId);
        Task<JiraTokenResponseDTO> RefreshTokenAsync( string refreshToken, string clientId, string clientSecret);
    }
}
