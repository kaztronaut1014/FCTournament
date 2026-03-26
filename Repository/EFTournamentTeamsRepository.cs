using FCTournament.Models;
using Microsoft.EntityFrameworkCore;

namespace FCTournament.Repository
{
    public class EFTournamentTeamRepository : ITournamentTeamRepository
    {
        private readonly ApplicationDbContext _context;

        public EFTournamentTeamRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddTournamentTeamAsync(TournamentTeam tournamentTeam)
        {
            _context.TournamentTeams.Add(tournamentTeam);
            await _context.SaveChangesAsync();
        }
        public async Task RemoveTournamentTeamAsync(int tournamentId, int teamId)
        {
            var tournamentTeam = await _context.TournamentTeams
                .FirstOrDefaultAsync(tt => tt.TournamentId == tournamentId && tt.TeamId == teamId);

            if (tournamentTeam != null)
            {
                _context.TournamentTeams.Remove(tournamentTeam);
                await _context.SaveChangesAsync();
            }
        }

        public async Task ApproveTeamAsync(int tournamentId, int teamId)
        {
            var tournamentTeam = await _context.TournamentTeams
                .FirstOrDefaultAsync(tt => tt.TournamentId == tournamentId && tt.TeamId == teamId);

            if (tournamentTeam != null)
            {
                tournamentTeam.IsApproved = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
