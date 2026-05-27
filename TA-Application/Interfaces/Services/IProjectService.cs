using TA_Application.DTOs;

namespace TA_Application.Interfaces.Services
{
    public interface IProjectService
    {
        Task<ResponseDTO> SaveProjectAsync(string userId, int companyId, CreateProjectDTO project, CancellationToken ct);
        Task<List<ProjectDTO>> GetAllAsync(int companyId, string userId, CancellationToken ct);
        Task<List<ProjectDTO>> GetUserProjectsAsync(int companyId, string userId, CancellationToken ct);
        //projektet ne te cilat nje user i caktum nuk eshte assigned ende ne to 
        Task<List<ProjectDTO>> GetProjectsAvailableToAssignToUser(int companyId, string userId, CancellationToken ct);
        //userat qe nuk jon te bom assign ne nje projekt te caktum 
        Task<List<UserDTO>> GetUsersAvailableToAssignToProject(int companyId, int projectId, CancellationToken ct);
        Task<ResponseDTO> AssignUserToProjects(AssignUserProjectsDTO dto, CancellationToken ct);
        Task<ResponseDTO> AssignUsersToProject(AssignProjectUsersDTO dto, CancellationToken ct);
        Task<List<UserDTO>> GetMembersAsync(int projectId, CancellationToken ct);
        Task<ResponseDTO> RemoveMemberAsync(string userId, int projectId, CancellationToken ct);
        Task<ProjectDTO>GetProjectByExternalId(string externalId, CancellationToken ct);
    }
}
