using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCTournament.Models
{
    public class Tournament
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public DateOnly StartDate { get; set; }
        public int DurationDays { get; set; }
        public int TypeId { get; set; } //0: sân 5, 1: sân 7, 2: sân 11
        public Type? Type { get; set; }
        public string Description { get; set; }
        public int LocationId { get; set; }
        public Location? Location { get; set; }
        public string Organizer { get; set; }
        public string Rules { get; set; }
        public float Prize { get; set; }
        public float Fees { get; set; }
        public int Size { get; set; }
        public int FormatId { get; set; } //0: vòng tròn, 1: loại trực tiếp, 2: vòng bảng + loại trực tiếp
        public Format? Format { get; set; }
        public bool needApproved { get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
        public bool IsFinished { get; set; } = false;
        public bool IsStarted { get; set; } = false;
    }
}
