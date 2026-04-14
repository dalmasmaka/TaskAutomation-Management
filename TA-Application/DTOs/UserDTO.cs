namespace TA_Application.DTOs
{
    public class AuthDTO
    {
        public string? Id { get; set; }
        public required string Name { get; set; }
        public required string Lastname { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
    }
    public class UserDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? PasswordHash { get; set; }
    }
    public class LoginDTO
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
    public class RegisterDTO
    {
        public required string FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Password { get; set; }
        public string? WorkspaceName { get; set; }
        public string? InvitationCode { get; set; }

    }
    public class ChangePasswordDTO
    {
        public required string userId { get; set; }
        public required string oldPassword { get; set; }
        public required string newPassword { get; set; }
    }
    public class ChangeUserRoleDTO
    {
        public string UserId { get; set; }
        public string NewRole { get; set; }
    }
    public class RolesDTO
    {
        public string RoleId { get; set; }
        public string RoleName { get; set; }
    }


}
