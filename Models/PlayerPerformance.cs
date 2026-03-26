namespace FCTournament.Models
{
    public class PlayerPerformance
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public Match? Match { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public float ScoreRating { get; set; } // Avg 6.5, good 7-8, excellent 8.5-10
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int OwnGoals { get; set; }
        public int Shots { get; set; }
        public int? ShotsOnTarget { get; set; }
        public int? PassesCompleted { get; set; }
        public int? PassesAttempted { get; set; }
        public int? Tackles { get; set; }
        public int? Interceptions { get; set; }
        public int? Fouls { get; set; }
        public int? YellowCards { get; set; }
        public int? RedCards { get; set; }
    }
}
