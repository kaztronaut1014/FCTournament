using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFBillDetailsRepository : IBillDetailsRepository
    {
        private readonly ApplicationDbContext _context;
        public EFBillDetailsRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BillDetails>> GetAllAsync()
        {
            return await _context.BillDetails.ToListAsync();
        }

        public async Task<List<BillDetails>> GetByBillIdAsync(int billId)
        {
            return await _context.BillDetails
                .Where(bd => bd.BillId == billId)
                .ToListAsync();
        }
        public async Task AddAsync(BillDetails billDetails)
        {
            _context.BillDetails.Add(billDetails);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(BillDetails billDetails)
        {
            _context.BillDetails.Update(billDetails);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(int id)
        {
            var billDetails = await _context.BillDetails.FindAsync(id);
            if (billDetails != null)
            {
                _context.BillDetails.Remove(billDetails);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<Dictionary<int, float>> GetPaidAmountsByUserIdAsync(string userId)
        {
            return await _context.BillDetails
                .Where(bd => bd.Bill.Team.ApplicationUserId == userId)
                .GroupBy(bd => bd.BillId)
                .ToDictionaryAsync(g => g.Key, g => g.Sum(bd => bd.FeePaid));
        }

        public async Task<float> GetTotalPaidAmountByBillIdAsync(int billId)
        {
            return await _context.BillDetails
                .Where(bd => bd.BillId == billId)
                .SumAsync(bd => bd.FeePaid);
        }
    }
}
