using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFProgressRepository : IProgressRepository
    {
        private readonly ApplicationDbContext _context;
        public EFProgressRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Models.Progress>> GetAllProgressesAsync()
        {
            return await _context.Progresses.ToListAsync();
        }
        public async Task<Models.Progress> GetProgressByIdAsync(int id)
        {
            return await _context.Progresses.FindAsync(id);
        }
        public async Task AddProgressAsync(Models.Progress progress)
        {
            _context.Progresses.Add(progress);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateProgressAsync(Models.Progress progress)
        {
            _context.Entry(progress).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteProgressAsync(int id)
        {
            var progress = await _context.Progresses.FindAsync(id);
            if (progress != null)
            {
                _context.Progresses.Remove(progress);
                await _context.SaveChangesAsync();
            }
        }
    }
}
