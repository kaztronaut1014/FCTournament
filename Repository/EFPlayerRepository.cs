using FCTournament.Repository;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Models
{
    public class EFPlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _context;
        public EFPlayerRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Player>> GetPlayersAsync()
        {
            return await _context.Players
                .Include(p => p.Team)
                .Include(p => p.User)
                .ToListAsync();
        }
        public async Task<Player> GetPlayerByIdAsync(int id)
        {
            return await _context.Players.FindAsync(id);
        }
        public async Task<Player> GetPlayerByUserIdAsync(string userid)
        {
            return await _context.Players.FirstOrDefaultAsync(p => p.ApplicationUserId == userid);
        }
        public async Task AddPlayerAsync(Player player)
        {
            _context.Players.Add(player);
            await _context.SaveChangesAsync();
        }
        public async Task UpdatePlayerAsync(Player player)
        {
            _context.Players.Update(player);
            await _context.SaveChangesAsync();
        }
        public async Task DeletePlayerAsync(int id)
        {
            var player = await _context.Players.FindAsync(id);
            if (player != null)
            {
                _context.Players.Remove(player);
                await _context.SaveChangesAsync();
            }
        }
    }
}
