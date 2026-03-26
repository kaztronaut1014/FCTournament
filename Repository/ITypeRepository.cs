namespace FCTournament.Repository
{
    public interface ITypeRepository
    {
        Task<IEnumerable<Models.Type>> GetAllTypeAsync();
        Task<Models.Type> GetTypeByIdAsync(int id);
        Task AddTypeAsync(Models.Type type);
        Task UpdateTypeAsync(Models.Type type);
        Task DeleteTypeAsync(int id);
    }
}
