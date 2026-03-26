using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FCTournament.Models
{
    public class Player
    {
        public int Id { get; set; }
        public string? NickName { get; set; }
        public int? TeamId { get; set; }
        public Team? Team { get; set; }
        public string ApplicationUserId { get; set; } // Dùng string vì Id của Identity là string
        [ForeignKey("ApplicationUserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
