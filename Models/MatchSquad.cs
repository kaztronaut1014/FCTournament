namespace FCTournament.Models
{
    public class MatchSquad
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public Match? Match { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
    }
}
