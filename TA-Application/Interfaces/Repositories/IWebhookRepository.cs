using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;

namespace TA_Application.Interfaces.Repositories
{
    public interface IWebhookRepository
    {
        Task SaveLogAsync(ProjectWebhookDTO log);
    }
}
