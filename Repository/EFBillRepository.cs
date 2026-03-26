using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFBillRepository : IBillRepository
    {
        private readonly ApplicationDbContext _context;

        public EFBillRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddBillAsync(Bill bill)
        {
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBillAsync(Bill bill)
        {
            _context.Bills.Update(bill);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBillAsync(int tournamentId, int teamId)
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.TournamentId == tournamentId && b.TeamId == teamId);
            if (bill != null)
            {
                _context.Bills.Remove(bill);
                await _context.SaveChangesAsync();
            }
        }

        public async Task PaidBillAsync(int tournamentId, int teamId)
        {
            var bill = await _context.Bills
                .FirstOrDefaultAsync(b => b.TournamentId == tournamentId && b.TeamId == teamId);
            if (bill != null)
            {
                bill.isPaid = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
