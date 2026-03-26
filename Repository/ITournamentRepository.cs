using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface ITournamentRepository
    {
        Task<IEnumerable<Models.Tournament>> GetAllTournamentsAsync();
        Task<Models.Tournament> GetTournamentByIdAsync(int id);
        Task AddTournamentAsync(Models.Tournament tournament);
        Task UpdateTournamentAsync(Models.Tournament tournament);
        Task DeleteTournamentAsync(int id);
        // Lấy danh sách giải đấu mà cầu thủ đang tham gia
        Task<List<Tournament>> GetTournamentsByPlayerUserIdAsync(string userId);
    }
}
