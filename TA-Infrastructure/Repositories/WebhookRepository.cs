using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using TA_Application.DTOs;
using TA_Application.Interfaces.Repositories;
using TA_Domain.Entities;
using TA_Infrastructure.Persistence;

namespace TA_Infrastructure.Repositories
{
    public class WebhookRepository : IWebhookRepository
    {
        private readonly ApplicationDbContext _context;
        public WebhookRepository(ApplicationDbContext context)
        {
            _context = context;
        }   
        public async System.Threading.Tasks.Task SaveLogAsync(ProjectWebhookDTO dto)
        {
            // 1. Check if this specific webhook already exists for this project/provider
            var existingWebhook = await _context.ProjectWebhooks
                .FirstOrDefaultAsync(w => w.ProjectId == dto.ProjectId && w.Provider == dto.Provider);

            if (existingWebhook != null)
            {
                existingWebhook.ExternalWebhookId = dto.ExternalWebhookId;
                existingWebhook.UpdatedAt = DateTime.UtcNow; 
            }
            else
            {
                // 3. Create new record
                var newWebhook = new ProjectWebhook
                {
                    ProjectId = dto.ProjectId,
                    Provider = dto.Provider,
                    ExternalWebhookId = dto.ExternalWebhookId,
                    CallbackUrl = dto.CallbackUrl,
                    CreatedAt = DateTime.UtcNow,
                };

                await _context.ProjectWebhooks.AddAsync(newWebhook);
            }

            await _context.SaveChangesAsync();
        }
    }
}
