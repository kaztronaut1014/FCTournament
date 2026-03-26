using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IStatusRepository
    {
        Task<IEnumerable<Models.Status>> GetAllStatusesAsync();
        Task<Models.Status> GetStatusByIdAsync(int id);
        Task AddStatusAsync(Models.Status status);
        Task UpdateStatusAsync(Status status);
        Task DeleteStatusAsync(int id);
    }
}
