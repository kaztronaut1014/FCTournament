using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface ITeamRepository
    {
        Task<List<Team>> GetTeamsAsync();
        Task<Team> GetTeamByIdAsync(int id);
        Task<IEnumerable<Team>> GetTeamsByUserIdAsync(string userId);
        Task AddTeamAsync(Team team);
        Task UpdateTeamAsync(Team team);
        Task DeleteTeamAsync(int id);
        Task<bool> IsTeamRegisteredInTournamentAsync(int teamId, int tournamentId);
    }
}
