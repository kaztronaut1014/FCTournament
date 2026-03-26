namespace FCTournament.Models
{
    public class Referee
    {
        public int Id { get; set; }
        public string ApplicationUserId { get; set; } // Dùng string vì Id của Identity là string
        public virtual ApplicationUser? User { get; set; }
    }
}
