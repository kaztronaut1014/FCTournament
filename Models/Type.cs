using System.ComponentModel.DataAnnotations;

namespace FCTournament.Models
{
    public class Type
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        //0: sân 5, 1: sân 7, 2: sân 11
        //0: 5-a-side, 1: 7-a-side, 2: 11-a-side
    }
}
