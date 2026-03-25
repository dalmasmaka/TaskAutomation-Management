using TA_Application.Interfaces;

namespace TA_Infrastructure.Repositories
{
    public class PasswordHasherRepository : IPasswordHasherRepository
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
