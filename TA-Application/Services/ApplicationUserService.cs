using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Application.Interfaces.Services;
using TA_Domain.Entities;

namespace TA_Application.Services
{
    public class ApplicationUserService: IAppUserService
    {
        private readonly IAppUserRepository _repo;
        private readonly ICompanyRepository _companyService;
        private readonly IPasswordHasherService _hasher;
        private readonly ICompanyService _appService;
        private readonly IJwtService _jwtservice;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApplicationUserService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserService(IAppUserRepository repo, IPasswordHasherService hasher, IJwtService jwtservice,
            UserManager<ApplicationUser> userManager, ILogger<ApplicationUserService> logger, ICompanyRepository companyService, ICompanyService layerService, RoleManager<IdentityRole> roleManager)
        {
            _repo = repo;
            _hasher = hasher;
            _jwtservice = jwtservice;
            _userManager = userManager;
            _logger = logger;
            _companyService = companyService;
            _appService = layerService;
            _roleManager = roleManager;
        }
        public async Task<ResponseDTO> CheckUserExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("email cannot be null or empty.");
            try
            {
                var result = await _repo.GetByEmailAsync(email);
                if (result != null)
                {
                    return new ResponseDTO { Success = false, Message = "User with this email already exists." };
                    
                }
                return new ResponseDTO { Success = true, Message = "User does not exist." };
            }
            catch(Exception ex)
            {
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
            
        }
        public async Task<ResponseDTO> RegisterUserAsync(RegisterDTO userDTO)
        {
            if (userDTO == null) throw new ArgumentNullException(nameof(userDTO));

            try
            {
                var existingUser = await CheckUserExistsAsync(userDTO.Email);
                if(existingUser.Success == false)
                {
                    return new ResponseDTO { Success = false, Message = existingUser.Message };
                }
                var user = new ApplicationUser
                {
                    FullName = userDTO.FullName,
                    Email = userDTO.Email,
                    UserName = userDTO.UserName,
                    PasswordHash = _hasher.Hash(userDTO.Password),
                };
                if (!string.IsNullOrEmpty(userDTO.InvitationCode))
                {
                    var checkInvitation = await _appService.CheckInvitationCode(userDTO.InvitationCode);
                    if (checkInvitation.Success == false)
                    {
                        return new ResponseDTO { Success = false, Message = checkInvitation.Message };
                    }
                    //e shikon a ekziston kodi 
                    //nese ekziston a eshte i skaduar 
                    //nese nuk plotesohen kushtet nuk mund te krijohet useri
                    var companyId =  _companyService.GetCompanyByInvitationCode(userDTO.InvitationCode);
                    if (companyId == null)
                    {
                        return new ResponseDTO { Success = false, Message = "Invalid invitation code." };
                    }

                    user.CompanyId = companyId;

                }
                await _repo.AddAsync(user);

                var roleResult = await _userManager.AddToRoleAsync(user, userDTO.Role);

                if (!roleResult.Succeeded)
                {
                    return new ResponseDTO { Success = false, Message = "User created, but role assignment failed." };
                }
                if (userDTO.Role == "owner")
                {
                    var workspaceResult = await _companyService.AddWorkspace(userDTO.WorkspaceName, user.Id);

                    user.CompanyId = workspaceResult.Id;
                    await _repo.UpdateUser(user);
                }

               
                return new ResponseDTO { Success = true, Message = "User registered successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error registering user {Username}", userDTO.UserName);
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDTO> LoginUserAsync(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Credentials must be provided.");

            try
            {
                var user = await _repo.GetByEmailAsync(email);

                // 2. Security Best Practice: Unified Error Message
                //  "Invalid credentials" to prevent username harvesting.
                if (user == null || !_hasher.Verify(password, user.PasswordHash))
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Invalid username or password."
                    };
                }
                var userRoles = await _userManager.GetRolesAsync(user);
                string userRole = userRoles.FirstOrDefault() ?? "User";
                var token = _jwtservice.GenerateToken(user, userRole);

                return new ResponseDTO
                {
                    Success = true,
                    Message = token
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for user: {Username}", email);

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An internal error occurred during login."
                };
            }
        }
        public async Task<ResponseDTO> DeleteUserAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id)) throw new ArgumentNullException(nameof(id));
            try
            {
                var existingUser = await _repo.GetUserById(id);
                if (existingUser == null)
                {
                    return new ResponseDTO { Success = false, Message = "User does not exist." };
                }
                await _repo.DeleteUser(id);
                return new ResponseDTO { Success = true, Message = "User deleted successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting user {UserId}.", id);
                return new ResponseDTO { Success = false, Message = "An unexpected error occurred." };
            }
        }
        public async Task<ApplicationUser> UpdateUserAsync(UserDTO userDTO)
        {
            if (userDTO == null) throw new ArgumentNullException(nameof(userDTO));

            try
            {
                var existingUser = await _repo.GetUserById(userDTO.Id);

                if (existingUser == null)
                {
                    throw new ArgumentNullException(nameof(existingUser));
                }

                if (!string.IsNullOrWhiteSpace(userDTO.FullName))
                    existingUser.FullName = userDTO.FullName;

                if (!string.IsNullOrWhiteSpace(userDTO.Email))
                    existingUser.Email = userDTO.Email;

                if (!string.IsNullOrWhiteSpace(userDTO.UserName))
                    existingUser.UserName = userDTO.UserName;

                if (!string.IsNullOrWhiteSpace(userDTO.Password))
                    existingUser.PasswordHash = _hasher.Hash(userDTO.Password);

                await _repo.UpdateUser(existingUser);
                return existingUser;
                //return new ResponseDTO { Success = true, Message = "User updated successfully." };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {UserId}.", userDTO.Id);
                throw ex;
                //return new ResponseDTO { Success = false, Message = "An unexpected error occurred." };
            }
        }
        public async Task<ResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            if (string.IsNullOrEmpty(changePasswordDTO.userId))
                throw new ArgumentException("UserId is null", nameof(changePasswordDTO.userId));

            if (string.IsNullOrEmpty(changePasswordDTO.oldPassword))
                throw new ArgumentException("Old Password is null", nameof(changePasswordDTO.oldPassword));

            if (string.IsNullOrEmpty(changePasswordDTO.newPassword))
                throw new ArgumentException("New Password is null", nameof(changePasswordDTO.newPassword));

            try
            {
                var storedHash = await _repo.GetPasswordByUserIdAsync(changePasswordDTO.userId);

                //  Check if old password is correct
                var isOldPasswordValid = _hasher.Verify(changePasswordDTO.oldPassword, storedHash);
                if (!isOldPasswordValid)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Current password is incorrect."
                    };
                }

                //  Prevent same password reuse
                var isSameAsOld = _hasher.Verify(changePasswordDTO.newPassword, storedHash);
                if (isSameAsOld)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "New password cannot be the same as the old password."
                    };
                }

                //  Hash new password
                var newHashedPassword = _hasher.Hash(changePasswordDTO.newPassword);

                // Save to DB
                await _repo.ChangePassword(changePasswordDTO.userId, newHashedPassword);

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Password changed successfully."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {UserId}.", changePasswordDTO.userId);

                return new ResponseDTO
                {
                    Success = false,
                    Message = "An unexpected error occurred."
                };
            }
        }
        public async Task<UserDTO> GetCurrentUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                throw new Exception("User not found");

            var role = (await _userManager.GetRolesAsync(user)).FirstOrDefault();

            return new UserDTO
            {
                Id = user.Id,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Role = role
            };
        }

        public async Task<ResponseDTO> ChangeUserRole(string userId, string roleId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("Invalid userId");

            try
            {
                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found");

                // 🔹 Get role by ID
                var role = await _roleManager.FindByIdAsync(roleId);
                if (role == null)
                    throw new Exception("Role not found");

                // 🔹 Remove existing roles
                var currentRoles = await _userManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }

                // 🔹 Add new role (by NAME internally)
                var result = await _userManager.AddToRoleAsync(user, role.Name);

                if (!result.Succeeded)
                    throw new Exception("Failed to assign role");

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Role updated successfully"
                };
            }
            catch (Exception ex)
            {
                return new ResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        public async Task<List<RolesDTO>> GetAllRoles(string userId)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentException("UserId is invalid");

            // 1. Get user
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found");

            // 2. Get roles assigned to user
            var userRoles = await _userManager.GetRolesAsync(user);

            // 3. Get all roles in system
            var allRoles = _roleManager.Roles.ToList();

            // 4. Filter roles NOT assigned to user
            var availableRoles = allRoles
                .Where(role => !userRoles.Contains(role.Name))
                .Select(role => new RolesDTO
                {
                    RoleId = role.Id,
                    RoleName = role.Name
                })
                .ToList();

            return availableRoles;
        }
    }
}
