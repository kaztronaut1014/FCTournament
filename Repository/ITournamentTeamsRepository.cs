using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface ITournamentTeamRepository
    {
        Task AddTournamentTeamAsync(TournamentTeam tournamentTeam);
        // Sau này có thể thêm các hàm như: GetTeamsInTournament, RemoveTeam... ở đây
        Task RemoveTournamentTeamAsync(int tournamentId, int teamId);
        Task ApproveTeamAsync(int tournamentId, int teamId);
    }
}
