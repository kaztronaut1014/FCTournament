using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCTournament.Models
{
    public class Team
    {
        public int Id { get; set; }
        [Required, StringLength(50)]
        public string FullName { get; set; }
        [Required, StringLength(4), MinLength(2)]
        public string ShortName { get; set; }
        public int LocationId { get; set; }
        public Location? Location { get; set; }
        public string? Logo { get; set; }
        public string ManagerId { get; set; }
        public Manager? Manager { get; set; }
        public DateOnly EstablishDate { get; set; }
        public List<Player>? Players { get; set; }
        public int inTournament { get; set; }
        public bool IsDeleted { get; set; } = false;
        public virtual ICollection<TournamentTeam> TournamentTeams { get; set; } = new List<TournamentTeam>();
        public string? ApplicationUserId { get; set; }
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? ApplicationUser { get; set; }
    }
}
