using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFManagerRepository
    {
        private readonly ApplicationDbContext _context;

        public EFManagerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Manager?> GetManagerByUserIdAsync(string userId)
        {
            return await _context.Managers.FirstOrDefaultAsync(o => o.ApplicationUserId == userId);
        }

        public async Task AddManagerAsync(Manager manager)
        {
            _context.Managers.Add(manager);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateManagerAsync(Manager manager)
        {
            _context.Managers.Update(manager);
            await _context.SaveChangesAsync();
        }
    }
}
