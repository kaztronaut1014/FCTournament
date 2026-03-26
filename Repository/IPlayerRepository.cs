using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IPlayerRepository
    {
        Task<List<Player>> GetPlayersAsync();
        Task<Player> GetPlayerByIdAsync(int id);
        Task<Player> GetPlayerByUserIdAsync(string userid);
        Task AddPlayerAsync(Player player);
        Task UpdatePlayerAsync(Player player);
        Task DeletePlayerAsync(int id);
    }
}
