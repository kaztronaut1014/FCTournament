namespace FCTournament.Models
{
    public class Match
    {
        public int Id { get; set; }
        public int TournamentId { get; set; }
        public Tournament? Tournament { get; set; }
        public int HomeId { get; set; }
        public Team? Home { get; set; }
        public int AwayId { get; set; }
        public Team? Away { get; set; }
        public DateTime StartDate { get; set; }
        public int StatusId { get; set; } // 0: Not Started, 1: Ongoing, 2: Finished
        public Status? Status { get; set; }
        public int Round { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Code cũ: public virtual MatchStats? MatchStats { get; set; }
        // Code mới chuẩn ERD:
        public virtual ICollection<MatchStats> MatchStats { get; set; } = new List<MatchStats>();
        // THÊM DÒNG NÀY CHO DIỄN BIẾN TRẬN ĐẤU
        public virtual ICollection<MatchDetail> MatchDetails { get; set; } = new List<MatchDetail>();
        // Thêm cột này để lưu ID của đội được BTC chọn đi tiếp khi hòa Tổng tỉ số
        public int? AdvancingTeamId { get; set; }
    }
}
