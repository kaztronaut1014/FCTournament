using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFTeamRepository : ITeamRepository
    {
        private readonly ApplicationDbContext _context;

        public EFTeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Team>> GetTeamsAsync()
        {
            return await _context.Teams.ToListAsync();
        }

        public async Task<IEnumerable<Team>> GetTeamsByUserIdAsync(string userId)
        {
            return await _context.Teams
                .Include(t => t.Location) // Lấy kèm tên khu vực
                .Where(t => t.ApplicationUserId == userId && !t.IsDeleted)
                .ToListAsync();
        }

        public async Task<Team?> GetTeamByIdAsync(int id)
        {
            return await _context.Teams
                .Include(t => t.Location)
                .Include(t => t.Players)
                    .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task AddTeamAsync(Team team)
        {
            _context.Teams.Add(team);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTeamAsync(Team team)
        {
            _context.Teams.Update(team);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTeamAsync(int id)
        {
            var team = await _context.Teams.FindAsync(id);
            if (team != null)
            {
                _context.Teams.Remove(team);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<bool> IsTeamRegisteredInTournamentAsync(int teamId, int tournamentId)
        {
            // Kiểm tra xem có dòng dữ liệu nào khớp với cả 2 ID này không
            return await _context.TournamentTeams
                .AnyAsync(tt => tt.TeamId == teamId && tt.TournamentId == tournamentId);
        }
    }
}
