using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFTournamentRepository : ITournamentRepository
    {
        private readonly ApplicationDbContext _context;
        public EFTournamentRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Tournament>> GetAllTournamentsAsync()
        {
            return await _context.Tournaments
                .Include(t => t.Type)     // <-- Bắt buộc phải có dòng này để lấy thông tin Loại sân
                .Include(t => t.Format)   // <-- Bắt buộc phải có dòng này để lấy thông tin Thể thức
                .ToListAsync();
        }
        public async Task<Tournament> GetTournamentByIdAsync(int id)
        {
            return await _context.Tournaments
                .Include(t => t.Format)
                .Include(t => t.Type)
                .Include(t => t.Location)
                // Lấy danh sách các đội đã tham gia giải này
                .Include(t => t.TournamentTeams)
                    .ThenInclude(tt => tt.Team) // Lấy luôn thông tin chi tiết của Đội (Tên, Logo...)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
        public async Task AddTournamentAsync(Models.Tournament tournament)
        {
            _context.Tournaments.Add(tournament);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateTournamentAsync(Models.Tournament tournament)
        {
            _context.Entry(tournament).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteTournamentAsync(int id)
        {
            var tournament = await _context.Tournaments.FindAsync(id);
            if (tournament != null)
            {
                _context.Tournaments.Remove(tournament);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Tournament>> GetTournamentsByPlayerUserIdAsync(string userId)
        {
            // 1. Tìm thông tin Cầu thủ (Player) dựa vào tài khoản đang đăng nhập
            var player = await _context.Players.FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            // Nếu tài khoản này không phải là Cầu thủ, hoặc Cầu thủ chưa gia nhập Đội bóng nào -> Trả về rỗng
            if (player == null || player.TeamId == null)
            {
                return new List<Tournament>();
            }

            // 2. Tìm các giải đấu mà Đội bóng của cầu thủ này đang tham gia
            var tournaments = await _context.TournamentTeams
                .Where(tt => tt.TeamId == player.TeamId)
                .Include(tt => tt.Tournament)
                    .ThenInclude(t => t.Format) // Lấy kèm thông tin Thể thức
                .Include(tt => tt.Tournament)
                    .ThenInclude(t => t.Location) // Lấy kèm thông tin Địa điểm
                .Select(tt => tt.Tournament)
                .Where(t => !t.IsDeleted) // Không lấy các giải đã xóa
                .ToListAsync();

            return tournaments;
        }
    }
}
