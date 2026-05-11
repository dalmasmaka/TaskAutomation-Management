using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Text;
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
        private readonly IEmailService _emailService;
        private readonly IJwtService _jwtService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ApplicationUserService> _logger;
        private readonly RoleManager<IdentityRole> _roleManager;

        public ApplicationUserService(IAppUserRepository repo, IPasswordHasherService hasher, IJwtService jwtService,
            UserManager<ApplicationUser> userManager, ILogger<ApplicationUserService> logger, ICompanyRepository companyService, ICompanyService layerService, RoleManager<IdentityRole> roleManager, IEmailService emailService)
        {
            _repo = repo;
            _hasher = hasher;
            _jwtService = jwtService;
            _userManager = userManager;
            _emailService = emailService;
            _logger = logger;
            _companyService = companyService;
            _appService = layerService;
            _roleManager = roleManager;
        }
        public async Task<ResponseDTO> CheckUserExistsAsync(string email)
        {
            ArgumentNullException.ThrowIfNull(email);
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
                _logger.LogError(ex, "Error while checking user!");
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
            
        }
        public async Task<ResponseDTO> RegisterUserAsync(RegisterDTO userDTO)
        {
            ArgumentNullException.ThrowIfNull(userDTO);

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
                    var companyId =  _companyService.GetCompanyByInvitationCode(userDTO.InvitationCode);
                    if (companyId <= 0)
                    {
                        return new ResponseDTO
                        {
                            Success = false,
                            Message = "Invalid invitation code."
                        };
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
                    var workspaceResult = await _companyService.AddWorkspace(userDTO.WorkspaceName!, user.Id);

                    user.CompanyId = workspaceResult.Id;
                    await _repo.UpdateUser(user);
                }
              
                var token = _jwtService.GenerateToken(user, userDTO.Role);

                return new ResponseDTO { Success = true, Message = token };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error registering user {Username}", userDTO.UserName);
                return new ResponseDTO { Success = false, Message = ex.Message };
            }
        }
        public async Task<ResponseDTO> LoginUserAsync(string email, string password)
        {
            ArgumentNullException.ThrowIfNull(email);
            ArgumentNullException.ThrowIfNull(password);

            try
            {
                var user = await _repo.GetByEmailAsync(email);

                // Security Best Practice: Unified Error Message
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
                var token = _jwtService.GenerateToken(user, userRole);

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
            ArgumentNullException.ThrowIfNull(id);
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
            ArgumentNullException.ThrowIfNull(userDTO);

            try
            {
                var existingUser = await _repo.GetUserById(userDTO.Id!)?? throw new KeyNotFoundException("User not found");

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error updating user {UserId}.", userDTO.Id);
                throw;
            }
        }
        public async Task<ResponseDTO> ChangePasswordAsync(ChangePasswordDTO changePasswordDTO)
        {
            ArgumentNullException.ThrowIfNull(changePasswordDTO);
           
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
            ArgumentNullException.ThrowIfNull(userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId)
               ?? throw new KeyNotFoundException("User not found");

                var roles = await _userManager.GetRolesAsync(user);
                var role = roles.FirstOrDefault();

                return new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName ?? string.Empty,
                    PhoneNumber = user.PhoneNumber ?? string.Empty,
                    Role = role
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Unexpected error fetching user!");
                throw;
            }
        }

        public async Task<ResponseDTO> ChangeUserRole(string userId, string roleId)
        {
            ArgumentNullException.ThrowIfNull(userId);
            ArgumentNullException.ThrowIfNull(roleId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId)
                    ?? throw new KeyNotFoundException("User not found");

                var role = await _roleManager.FindByIdAsync(roleId)
                    ?? throw new KeyNotFoundException("Role not found");

                var currentRoles = await _userManager.GetRolesAsync(user);

                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);

                    if (!removeResult.Succeeded)
                        return new ResponseDTO
                        {
                            Success = false,
                            Message = "Failed to remove current roles"
                        };
                }

                if (string.IsNullOrEmpty(role.Name))
                    throw new InvalidOperationException("Role name is invalid");

                var result = await _userManager.AddToRoleAsync(user, role.Name);

                if (!result.Succeeded)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "Failed to assign role"
                    };
                }

                return new ResponseDTO
                {
                    Success = true,
                    Message = "Role updated successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while changing user role!");

                return new ResponseDTO
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }
        public async Task<List<RolesDTO>> GetAllRoles(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);

            try
            {
                var user = await _userManager.FindByIdAsync(userId);

                if (user == null)
                    throw new KeyNotFoundException("User not found");

                var userRoles = await _userManager.GetRolesAsync(user);

                var allRoles = _roleManager.Roles.ToList();

                var availableRoles = allRoles
                    .Where(role => role.Name != null && !userRoles.Contains(role.Name))
                    .Select(role => new RolesDTO
                    {
                        RoleId = role.Id,
                        RoleName = role.Name! 
                    })
                    .ToList();

                return availableRoles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching roles.");
                throw;
            }
        }

        public async Task<ResponseDTO> ForgotPasswordAsync(ForgotPasswordRequestDTO request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if(user == null)
                {
                    return new ResponseDTO
                    {
                        Success = false,
                        Message = "A user with this email address does not exist."
                    };
                }

                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var encodedToken = WebEncoders.Base64UrlEncode(
                    Encoding.UTF8.GetBytes(token));

                var resetLink =
                    $"http://localhost:4200/reset-password?email={user.Email}&token={encodedToken}";

                var html = $@"
        <h2>Password Reset</h2>
        <p>You requested to reset your password.</p>
        <p>
            <a href='{resetLink}'>
                Click here to reset password
            </a>
        </p>
        <p>This link will expire in 30 minutes.</p>
    ";

                await _emailService.SendEmailAsync(
                    user.Email,
                    "Reset Password",
                    html
                );
                return new ResponseDTO
                {
                    Success = true,
                    Message = "Password reset link has been sent. Check your email!"
                };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync for email: {Email}", request.Email);
                return new ResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while processing your request."
                };
            }
          
        }

        public async Task<ResponseDTO> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email) ?? throw new KeyNotFoundException("User not found");

                var decodedToken = Encoding.UTF8.GetString(
                    WebEncoders.Base64UrlDecode(request.Token));

                user.PasswordHash = _hasher.Hash(request.NewPassword);

                await _repo.UpdateUser(user);
                return new ResponseDTO
                {
                    Success = true,
                    Message = "Password reset successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync for email: {Email}", request.Email);
                return new ResponseDTO
                {
                    Success = false,
                    Message = "An error occurred while processing your request."
                };
            }
        }
    }
}
