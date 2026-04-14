using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IJiraHttpClient
    {
        Task<string> GetAuthorizationUrlAsync(string clientId, string redirectUri);

        Task<string> ExchangeCodeForTokenAsync(
            string code,
            int projectId,
            string clientId,
            string clientSecret,
            string redirectUri);

        Task<string> GetCloudIdAsync(string accessToken);

        Task<TaskDTO[]> GetTasksAsync(string accessToken, string cloudId);
    }
}
