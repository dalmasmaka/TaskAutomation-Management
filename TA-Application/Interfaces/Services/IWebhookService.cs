using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IWebhookService
    {
        Task RegisterAsync(ProjectIntegrationDTO integration);
        Task HandleAsync(string provider, object payload);
    }
}
