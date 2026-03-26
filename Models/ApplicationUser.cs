using Microsoft.AspNetCore.Identity;

namespace FCTournament.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? PhoneNumber { get; set; }
        public string? AvatarUrl { get; set; }

        // Một tài khoản có thể có nhiều hồ sơ khác nhau (hoặc không có)
        public virtual Player? PlayerProfile { get; set; }
        public virtual Manager? ManagerProfile { get; set; }
        public virtual Organizer? OrganizerProfile { get; set; }
    }
}