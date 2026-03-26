using FCTournament.Models;

namespace FCTournament.Repository
{
    public interface IFormatRepository
    {
        Task<IEnumerable<Format>> GetAllFormatsAsync();
        Task<Format> GetFormatByIdAsync(int id);
        Task AddFormatAsync(Format format);
        Task UpdateFormatAsync(Format format);
        Task DeleteFormatAsync(int id);
    }
}
