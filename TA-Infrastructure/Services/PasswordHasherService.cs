using TA_Application.Interfaces;

namespace TA_Infrastructure.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);

        }
        public bool Verify(string? password, string? hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
