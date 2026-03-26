using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IMatchRepository
    {
        Task AddMatchAsync(Match match);
        Task<List<Match>> GetMatchesByTournamentAsync(int tournamentId);
        Task<Match> GetMatchByIdAsync(int matchId);
        Task DeleteMatchAsync(int matchId);
        Task DeleteMatchesByTournamentAsync(int tournamentId);
        Task UpdateScoreAsync(int matchId, int homeGoals, int awayGoals, int statusId,
            List<int> homePlayerIds, List<int> homeMinutes, List<int> homeProgressIds,
            List<int> awayPlayerIds, List<int> awayMinutes, List<int> awayProgressIds);
        Task UpdateMatchAsync(Match match);
    }
}
