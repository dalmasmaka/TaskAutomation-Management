
using TA_Domain.Enums;

namespace TA_Application.DTOs
{
    public class TeamDTO
    {
        public int TeamId { get; set; }
        public string? TeamName { get; set; }
        public int CompanyId { get; set; }
        public string? ActionByUserId { get; set; }
        public string? CreatedAt { get; set; }
        public string? CreatedByUser { get; set; }

    }

    public class MemberDTO
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public MembershipStatus PlainStatus { get; set; }
        public string Status { get; set; }
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string MembershipStatus { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
        public string InvitedBy { get; set; }
    }
    public class AssignMemberToTeamsDTO
    {
        public string UserId { get; set; }
        public List<int> TeamIds { get; set; } = new();
        public string ActionByUserId { get; set; }
    }
    public class AssignProjectWithTeamsDTO
    {
        public int ProjectId { get; set; }
        public List<int> TeamIds { get; set; } = new();
        public string ActionByUserId { get; set; }
    }
    public class TeamMembersDTO
    {
        public List<MemberDTO> ActiveMembers { get; set; }
        public List<MemberDTO> PendingMembers { get; set; }
        public List<MemberDTO> DeclinedMembers { get; set; }
    }
    public class MemberStatusUpdateDTO
    {
        public string UserId { get; set; }
        public int TeamId { get; set; }
        public MembershipStatus UpdateStatus { get; set; }
    }
    public class ProjectTeamDTO
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public int ProjectId { get; set; }
        public string AssignedAt { get; set; }
    }
    public class TeamInvitationsDTO
    {
        public List<int> TeamIDs { get; set; }
        public List<string> UserIDs { get; set; }
    }
    public class RemoveTeamMemberDTO
    {
        public int TeamId { get; set; }
        public string UserId { get; set; }
    }
    public class MemberTeamsDTO
    {
        public List<MemberDTO> PendingInvitations { get; set; }
        public List<MemberDTO> ActiveTeams { get; set; }
        public List<MemberDTO> TeamLogs { get; set; }
    }
}
