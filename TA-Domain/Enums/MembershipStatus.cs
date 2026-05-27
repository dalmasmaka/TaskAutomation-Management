using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace TA_Domain.Enums
{
    public enum MembershipStatus
    {
        [Display(Name = "Pending Approval")]
        [Description("Pending Approval")]
        PendingApproval,  // Owner invited the user; waiting for user to click "Accept"
        Active,           // User accepted; fully part of the team
        Declined          // Track if they said no
    }
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            var displayAttribute = enumValue.GetType()
                .GetField(enumValue.ToString())?
                .GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.Name ?? enumValue.ToString();
        }
    }
}
