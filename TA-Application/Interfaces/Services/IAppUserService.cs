using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Domain.Entities;

namespace TA_Application.Interfaces.Services
{
    public interface IAppUserService
    {
        Task<ResponseDTO> CheckUserExistsAsync(string email);
        Task<ResponseDTO> RegisterUserAsync(RegisterDTO userDTO);
        Task<ResponseDTO> LoginUserAsync(string email, string password);
        Task<ResponseDTO> DeleteUserAsync(string id);
        Task<ApplicationUser> UpdateUserAsync(UserDTO userDTO);
        Task<ResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO);
        Task<UserDTO> GetCurrentUser(string userId);
        Task<ResponseDTO> ChangeUserRole(string userId, string roleId);
        Task<List<RolesDTO>> GetAllRoles(string userId);
        Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordRequestDTO request);
        Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequestDTO request);
    }
}
