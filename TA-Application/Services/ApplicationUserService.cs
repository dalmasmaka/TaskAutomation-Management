using System.Security.Principal;
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

        public ApplicationUserService(IApplicationUserRepository repo, IPasswordHasherRepository hasher, IJwtRepository jwtservice)
        {
            _repo = repo;
            _hasher = hasher;
            _jwtservice = jwtservice;
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
            if (userDTO == null)
                throw new ArgumentNullException(nameof(userDTO), "User data cannot be null.");

            if (string.IsNullOrWhiteSpace(userDTO.UserName))
                throw new ArgumentException("Username is required.");

            var existingUser = await _repo.GetByUsernameAsync(userDTO.UserName);

            if (existingUser != null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User already exists."
                };
            }

            var user = new ApplicationUser
            {
                FullName = userDTO.FullName,
                Email = userDTO.Email,
                UserName = userDTO.UserName,
                PasswordHash = _hasher.Hash(userDTO.Password) 
            };
            await _repo.AddAsync(user);

            return new AuthResponseDTO
            {
                Success = true,
                Message = "User registered successfully."
            };
        }
        public async Task<AuthResponseDTO> LoginUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Username and password cannot be null or empty.");
            var user = await _repo.GetByUsernameAsync(username);
            if (user == null)
            {
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "User not found."
                };
            }
            if (user == null || !_hasher.Verify(password, user.PasswordHash))
            {
                return new AuthResponseDTO { Success = false, Message = "Invalid username or password" };
            }

            var token = _jwtservice.GenerateToken(user);
            return new AuthResponseDTO
            {
                Success = true,
                Message = token
            };
        }
    }
}
