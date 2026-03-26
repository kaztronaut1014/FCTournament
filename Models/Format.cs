using System.ComponentModel.DataAnnotations;

namespace FCTournament.Models
{
    public class Format
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        //0: vòng tròn, 1: loại trực tiếp, 2: vòng bảng + loại trực tiếp
        //0: Round Robin, 1: Knockout, 2: Group Stage + Knockout
    }
}
