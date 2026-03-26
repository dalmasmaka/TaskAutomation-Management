namespace TA_Application.DTOs
{
    public class TaskDTO
    {
        public int TaskId { get; set; }
        public required string ExternalId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public required string Status { get; set; }
        public string? ImportStatus { get; set; }
        //public int ProjectId { get; set; }
        //public required ProjectDTO Project { get; set; }
        public int? AssignedTo { get; set; }
        public UserDTO? AssignedToUser { get; set; }

        public ICollection<CodeGenerationDTO>? CodeGenerations { get; set; }
    }
}
