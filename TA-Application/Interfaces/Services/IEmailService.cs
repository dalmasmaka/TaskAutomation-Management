using System;
using System.Collections.Generic;
using System.Text;

namespace TA_Application.Interfaces.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string htmlContent);
    }
}
