using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IBillDetailsRepository
    {
        Task<IEnumerable<BillDetails>> GetAllAsync();
        Task<List<BillDetails>> GetByBillIdAsync(int billId);
        Task AddAsync(BillDetails billDetails);
        Task UpdateAsync(BillDetails billDetails);
        Task DeleteAsync(int id);
        Task<Dictionary<int, float>> GetPaidAmountsByUserIdAsync(string userId);
        Task<float> GetTotalPaidAmountByBillIdAsync(int billId);
    }
}
