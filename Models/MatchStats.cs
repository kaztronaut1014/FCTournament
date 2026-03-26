namespace FCTournament.Models
{
    public class MatchStats
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public virtual Match? Match { get; set; }
        public int TeamId { get; set; }
        public virtual Team? Team { get; set; }
        public int Goals { get; set; }
        public int Shots { get; set; }
        public int? ShotsOnTarget { get; set; }
        public int? Corners { get; set; }
        public int? Possession { get; set; }
        public int? Tackles { get; set; }
        public int? Interceptions { get; set; }
        public int? Fouls { get; set; }
        public int? YellowCards { get; set; }
        public int? RedCards { get; set; }
    }
}
