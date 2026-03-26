using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IBillRepository
    {
        Task AddBillAsync(Bill bill);
        Task UpdateBillAsync(Bill bill);
        Task RemoveBillAsync(int tournamentId, int teamId);
        Task PaidBillAsync(int tournamentId, int teamId);
    }
}
