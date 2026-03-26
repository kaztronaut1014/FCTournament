namespace FCTournament.Repository
{
    public interface IProgressRepository
    {
            Task<IEnumerable<Models.Progress>> GetAllProgressesAsync();
            Task<Models.Progress> GetProgressByIdAsync(int id);
            Task AddProgressAsync(Models.Progress progress);
            Task UpdateProgressAsync(Models.Progress progress);
            Task DeleteProgressAsync(int id);
    }
}
