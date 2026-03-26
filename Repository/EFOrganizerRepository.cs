using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFOrganizerRepository
    {
        private readonly ApplicationDbContext _context;

        public EFOrganizerRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Organizer?> GetOrganizerByUserIdAsync(string userId)
        {
            return await _context.Organizers.FirstOrDefaultAsync(o => o.ApplicationUserId == userId);
        }

        public async Task AddOrganizerAsync(Organizer organizer)
        {
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateSubscriptionAsync(Organizer organizer, int newSubscriptionId)
        {
            organizer.SubscriptionId = newSubscriptionId;
            _context.Organizers.Update(organizer);
            await _context.SaveChangesAsync();
        }
    }
}
