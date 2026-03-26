using System.ComponentModel.DataAnnotations;

namespace FCTournament.Models
{
    public class Progress
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        //0: Sub-in, 1: Sub-out, 2: Goal, 3: Assist, 4: Own Goal, 5: Red Card, 6: Yellow Card
    }
}