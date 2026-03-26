using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFMatchRepository : IMatchRepository
    {
        private readonly ApplicationDbContext _context;

        public EFMatchRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddMatchAsync(Match match)
        {
            _context.Matches.Add(match);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Match>> GetMatchesByTournamentAsync(int tournamentId)
        {
            return await _context.Matches
                .Include(m => m.Home).ThenInclude(t => t.Players).ThenInclude(p => p.User) // Lấy cầu thủ Đội Nhà
                .Include(m => m.Away).ThenInclude(t => t.Players).ThenInclude(p => p.User) // Lấy cầu thủ Đội Khách
                .Include(m => m.Status)
                .Include(m => m.MatchStats)
                .Include(m => m.MatchDetails).ThenInclude(md => md.Player).ThenInclude(p => p.User) // Lấy diễn biến + Tên cầu thủ
                .Where(m => m.TournamentId == tournamentId && !m.IsDeleted)
                .OrderBy(m => m.StartDate)
                .ToListAsync();
        }

        // HÀM MỚI: CẬP NHẬT TỈ SỐ VÀ TRẠNG THÁI TRẬN ĐẤU
        public async Task UpdateScoreAsync(int matchId, int homeGoals, int awayGoals, int statusId,
            List<int> homePlayerIds, List<int> homeMinutes, List<int> homeProgressIds,
            List<int> awayPlayerIds, List<int> awayMinutes, List<int> awayProgressIds)
        {
            var match = await _context.Matches
                .Include(m => m.MatchStats)
                .Include(m => m.MatchDetails)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match != null)
            {
                match.StatusId = statusId;

                // 1. Cập nhật Tổng tỉ số
                if (match.MatchStats != null && match.MatchStats.Any())
                    _context.MatchStats.RemoveRange(match.MatchStats);

                _context.MatchStats.Add(new MatchStats { MatchId = matchId, TeamId = match.HomeId, Goals = homeGoals });
                _context.MatchStats.Add(new MatchStats { MatchId = matchId, TeamId = match.AwayId, Goals = awayGoals });

                // 2. Xóa các diễn biến cũ (Chỉ xóa Bàn thắng, Phản lưới, Thẻ Vàng, Thẻ Đỏ - ID 3, 5, 6, 7)
                var oldEvents = match.MatchDetails.Where(md => new[] { 3, 5, 6, 7 }.Contains(md.ProgressId)).ToList();
                if (oldEvents.Any()) _context.MatchDetails.RemoveRange(oldEvents);

                // 3. Lưu diễn biến mới Đội Nhà
                if (homePlayerIds != null && homeMinutes != null && homeProgressIds != null)
                {
                    for (int i = 0; i < homePlayerIds.Count; i++)
                    {
                        if (homePlayerIds[i] > 0)
                            _context.MatchDetails.Add(new MatchDetail { MatchId = matchId, PlayerId = homePlayerIds[i], Time = homeMinutes[i], ProgressId = homeProgressIds[i] });
                    }
                }

                // 4. Lưu diễn biến mới Đội Khách
                if (awayPlayerIds != null && awayMinutes != null && awayProgressIds != null)
                {
                    for (int i = 0; i < awayPlayerIds.Count; i++)
                    {
                        if (awayPlayerIds[i] > 0)
                            _context.MatchDetails.Add(new MatchDetail { MatchId = matchId, PlayerId = awayPlayerIds[i], Time = awayMinutes[i], ProgressId = awayProgressIds[i] });
                    }
                }

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMatchAsync(int id)
        {
            var match = await _context.Matches.FindAsync(id);
            if (match != null)
            {
                _context.Matches.Remove(match);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteMatchesByTournamentAsync(int tournamentId)
        {
            var matches = await _context.Matches.Where(m => m.TournamentId == tournamentId).ToListAsync();

            if (matches.Any())
            {
                var matchIds = matches.Select(m => m.Id).ToList();

                // 1. TÌM TẤT CẢ DỮ LIỆU CON (Diễn biến và Tỉ số)
                var matchDetails = await _context.MatchDetails.Where(md => matchIds.Contains(md.MatchId)).ToListAsync();
                var matchStats = await _context.MatchStats.Where(ms => matchIds.Contains(ms.MatchId)).ToListAsync();

                // 2. XÓA DỮ LIỆU CON TRƯỚC
                if (matchDetails.Any())
                {
                    _context.MatchDetails.RemoveRange(matchDetails);
                }
                if (matchStats.Any())
                {
                    _context.MatchStats.RemoveRange(matchStats);
                }

                // 3. ÉP LƯU XUỐNG DATABASE NGAY LẬP TỨC (Đây là chốt chặn quan trọng nhất để phá vỡ Khóa ngoại)
                await _context.SaveChangesAsync();

                // 4. BÂY GIỜ TRẬN ĐẤU ĐÃ SẠCH SẼ, TIẾN HÀNH XÓA TRẬN ĐẤU (Dữ liệu Cha)
                _context.Matches.RemoveRange(matches);

                // 5. LƯU LẦN CUỐI
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Match> GetMatchByIdAsync(int id)
        {
            return await _context.Matches
                .Include(m => m.Home).ThenInclude(t => t.Players).ThenInclude(p => p.User) // Lấy cầu thủ Đội Nhà
                .Include(m => m.Away).ThenInclude(t => t.Players).ThenInclude(p => p.User) // Lấy cầu thủ Đội Khách
                .Include(m => m.Status)
                .Include(m => m.MatchStats)
                .Include(m => m.MatchDetails).ThenInclude(md => md.Player).ThenInclude(p => p.User) // Lấy diễn biến + Tên cầu thủ
                .FirstOrDefaultAsync(m => m.Id == id && !m.IsDeleted);
        }

        public async Task UpdateMatchAsync(Match match)
        {
            _context.Entry(match).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
    }
}
