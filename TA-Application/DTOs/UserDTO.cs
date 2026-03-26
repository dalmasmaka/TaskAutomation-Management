
namespace TA_Application.DTOs
{
    public class UserDTO
    {
        public string? Id { get; set; }
        public string? FullName { get; set; }
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Password { get; set; }
        public string? PasswordHash { get; set; }
    }
    public class LoginDTO
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
    public class RegisterDTO
    {
        public required string FullName { get; set; }
        public required string UserName { get; set; }
        public required string Email { get; set; }
        public required string Role { get; set; }
        public required string Password { get; set; }

    }
}
