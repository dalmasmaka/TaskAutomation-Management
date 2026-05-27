using TA_Domain.Entities;

namespace TA_Application.Interfaces.Repositories
{
    public interface IWebhookRepository
    {
        System.Threading.Tasks.Task SaveLogAsync(ProjectWebhook log);
    }
}
