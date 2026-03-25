using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.Interfaces
{
    public interface IPasswordHasherRepository
    {
        string Hash(string password);
        bool Verify(string? password, string? hashedPassword);
    }
}
