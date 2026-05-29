using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IAppUserService
    {
        Task<ResponseDTO> CheckUserExistsAsync(string email, CancellationToken ct);
        Task<ResponseDTO> RegisterUserAsync(RegisterDTO userDTO, CancellationToken ct);
        Task<ResponseDTO> LoginUserAsync(string email, string password, CancellationToken ct);
        Task<ResponseDTO> DeleteUserAsync(string id, CancellationToken ct);
        Task<UserDTO> UpdateUserAsync(UserDTO userDTO, CancellationToken ct);
        Task<ResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO, CancellationToken ct);
        Task<UserDTO> GetCurrentUser(string userId, CancellationToken ct);
        Task<UserGitHubDTO>GetUserGitHubInfo(string userId, CancellationToken ct);
        Task<UserGitHubDTO> SaveUserGitHubInfo(UserGitHubDTO userGitHubDTO, CancellationToken ct);
        Task<ResponseDTO> ChangeUserRole(string userId, string roleId, CancellationToken ct);
        Task<List<RolesDTO>> GetAllRoles(string userId, CancellationToken ct);
        Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordRequestDTO request, CancellationToken ct);
        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequestDTO request, CancellationToken ct);
    }
}
