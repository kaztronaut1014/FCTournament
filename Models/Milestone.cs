namespace FCTournament.Models
{
    public class Milestone
    {
        public int Id { get; set; }
        public int? TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int? TeamId{ get; set; }
        public Team? Team { get; set; }
        public int? PlayerId { get; set; }
        public Player? Player { get; set; }
        public DateOnly? Date { get; set; }
        public int? Position { get; set; }
        public string? Description { get; set; }
        public int? Goals { get; set; }
        public int? Assists { get; set; }
        public int Type { get; set; } //0: team milestone, 1: player in team milestone, 2: player in tournament milestone
    }
}
