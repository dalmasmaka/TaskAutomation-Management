namespace TA_Application.DTOs
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public string? ExternalId { get; set; }
        public string? ProjectName { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }    
        public string? Status { get; set; }
        public string? ImportStatus { get; set; }
        public int ProjectId { get; set; }
        public int? CompanyId { get; set; }
        public string? Priority { get; set; }
        public string? AssignedToUser { get; set; }
        public bool? isDeleted { get; set; }
        public List<UserDTO>? Assigners { get; set; }
        public ICollection<CodeGenerationDTO>? CodeGenerations { get; set; }
    }
    public class UpdateTaskDTO
    {
        public int TaskId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
        public List<string>? Members { get; set; }
    }
    public class TaskMembersResponseDTO
    {
        public List<UserDTO>? AssignedMembers { get; set; }
        public List<UserDTO>? AvailableMembers { get; set; }
    }
    public class TaskMembersDTO
    {
        public int TaskId { get; set; }
        public required string UserId { get; set; }
    }
    public class TrelloCardResponse
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Desc { get; set; }
        public List<string>? IdMembers { get; set; }
        public TrelloList? List { get; set; }
    }

    public class TrelloList
    {
        public string? Name { get; set; }
    }
    public class Board
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Url { get; set; }
        public bool? Closed { get; set; }
        public string? Key { get; set; }
        public string? Type { get; set; }
    }
    public class TaskQuery
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int? CompanyId { get; set; }
        public List<int>? ProjectIds { get; set; }
        public List<string>? AssignedToIds { get; set; }
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? Priority { get; set; }
    }
    public class CompanyAssetsDTO
    {
        public List<ProjectDTO>? Projects { get; set; }
        public List<UserDTO>? Assigners { get; set; }
    }
}
