namespace FCTournament.Models
{
    public class TournamentTeam
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int TeamId { get; set; }
        public Team? Team { get; set; }
        public bool IsApproved { get; set; } = false;
        public int MatchesPlayed { get; set; }
        public int Wins { get; set; }
        public int Draws { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
        public int GoalsFor { get; set; }
        public int GoalsAgainst { get; set; }
        public int RedCards { get; set; }
        public int YellowCards { get; set; }

    }
}
