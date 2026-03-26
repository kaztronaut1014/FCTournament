namespace FCTournament.Models
{
    public class MatchReferee
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public Match? Match { get; set; }
        public int RefereeId { get; set; }
        public Referee? Referee { get; set; }
    }
}
