using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class ApplicationUserService
    {
        private readonly IApplicationUserRepository _repo;
        private readonly IPasswordHasherRepository _hasher;
        private readonly IJwtRepository _jwtservice;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApplicationUserService> _logger;

        public ApplicationUserService(IApplicationUserRepository repo, IPasswordHasherRepository hasher, IJwtRepository jwtservice,
            UserManager<ApplicationUser> userManager, ILogger<ApplicationUserService>logger)
        {
            _repo = repo;
            _hasher = hasher;
            _jwtservice = jwtservice;
            _userManager = userManager;
            _logger = logger;
        }
        public async Task<ApplicationUser> CheckUserExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Username cannot be null or empty.");

            var user = await _repo.GetByUsernameAsync(username);
            return user;
          
        }
        public async Task<AuthResponseDTO> RegisterUserAsync(RegisterDTO userDTO)
        {
            if (userDTO == null) throw new ArgumentNullException(nameof(userDTO));

            try
            {
                var existingUser = await _repo.GetByUsernameAsync(userDTO.UserName);
                if (existingUser != null)
                {
                    return new AuthResponseDTO { Success = false, Message = "Username is already taken." };
                }

                var user = new ApplicationUser
                {
                    FullName = userDTO.FullName,
                    Email = userDTO.Email,
                    UserName = userDTO.UserName,
                    PasswordHash = _hasher.Hash(userDTO.Password)
                };

                await _repo.AddAsync(user);

                var roleResult = await _userManager.AddToRoleAsync(user, userDTO.Role);
                if (!roleResult.Succeeded)
                {
                    return new AuthResponseDTO { Success = false, Message = "User created, but role assignment failed." };
                }

                return new AuthResponseDTO { Success = true, Message = "User registered successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error registering user {Username}", userDTO.UserName);
                return new AuthResponseDTO { Success = false, Message = "An unexpected error occurred." };
            }
        }
        public async Task<AuthResponseDTO> LoginUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Credentials must be provided.");

            try
            {
                var user = await _repo.GetByUsernameAsync(username);

                // 2. Security Best Practice: Unified Error Message
                //  "Invalid credentials" to prevent username harvesting.
                if (user == null || !_hasher.Verify(password, user.PasswordHash))
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    };
                }

                var token = _jwtservice.GenerateToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = token 
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", username);

                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "An internal error occurred during login."
                };
            }
        }
        public async Task<AuthResponseDTO> DeleteUserAsync(string id)
        {
            if(string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            try
            {
                var existingUser = await _repo.GetUserById(id);
                if(existingUser == null)
                {
                    return new AuthResponseDTO { Success = false, Message = "User does not exist." };
                }
                await _repo.DeleteUser(id);
                return new AuthResponseDTO { Success = true, Message = "User deleted successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user {UserId}.", id);
                return new AuthResponseDTO { Success = false, Message = "An unexpected error occurred." };
            }
        }
        public async Task<AuthResponseDTO> UpdateUserAsync(UserDTO userDTO)
        {
            if(userDTO==null) throw new ArgumentNullException(nameof(userDTO));
            try
            {
                var existingUser = await _repo.GetUserById(userDTO.Id);
                if (existingUser == null)
                {
                    return new AuthResponseDTO { Success = false, Message = "User does not exist." };
                }
                existingUser.FullName = userDTO.FullName;
                existingUser.Email = userDTO.Email;
                existingUser.UserName = userDTO.UserName;
                existingUser.PasswordHash = _hasher.Hash(userDTO.Password);
                await _repo.UpdateUser(existingUser);
                return new AuthResponseDTO { Success = true, Message = "User updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {UserId}.", userDTO);
                return new AuthResponseDTO { Success = false, Message = "An unexpected error occurred." };
            }
        }

    }
}
