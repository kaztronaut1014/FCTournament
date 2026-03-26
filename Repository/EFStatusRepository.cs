using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFStatusRepository : IStatusRepository
    {
        private readonly ApplicationDbContext _context;
        public EFStatusRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Models.Status>> GetAllStatusesAsync()
        {
            return await _context.Statuses.ToListAsync();
        }
        public async Task<Models.Status> GetStatusByIdAsync(int id)
        {
            return await _context.Statuses.FindAsync(id);
        }
        public async Task AddStatusAsync(Models.Status status)
        {
            _context.Statuses.Add(status);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateStatusAsync(Status status)
        {
            _context.Entry(status).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteStatusAsync(int id)
        {
            var status = await _context.Statuses.FindAsync(id);
            if (status != null)
            {
                _context.Statuses.Remove(status);
                await _context.SaveChangesAsync();
            }
        }
    }
}
