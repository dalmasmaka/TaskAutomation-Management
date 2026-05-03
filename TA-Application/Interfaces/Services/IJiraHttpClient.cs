using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IJiraHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri, int projectId, int companyId);
        Task<JiraTokenResponseDTO> ExchangeCodeForTokenAsync(string code, int projectId, string clientId, string clientSecret, string redirectUri);
        Task<string> GetCloudIdAsync(string accessToken);
        Task<List<TaskDTO>> GetJiraTasksAsync(string accessToken, string cloudId, string projectKey, int localProjectId);
        Task<List<Board>> GetBoardsAsync(string accessToken, string cloudId);
        Task<JiraTokenResponseDTO> RefreshTokenAsync( string refreshToken, string clientId, string clientSecret);
    }
}
