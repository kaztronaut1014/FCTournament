namespace FCTournament.Models
{
    public class MatchDetail
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public Match? Match { get; set; }
        public int PlayerId { get; set; }
        public Player? Player { get; set; }
        public int Time { get; set; } // Time in minutes when the event occurred
        //0: Started, 1: Goal, 2: Assist, 3: Yellow Card, 4: Red Card, 5: Substitution In, 6: Substitution Out
        public int ProgressId { get; set; } 
        public Progress? Progress { get; set; }
    }
}
