namespace FCTournament.Models
{
    public class Bill
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public DateTime DateCreate { get; set; } = DateTime.Now;    
        public DateTime DatePaid { get; set; }
        public float Fee { get; set; }
        public bool isPaid { get; set; }
    }
}
