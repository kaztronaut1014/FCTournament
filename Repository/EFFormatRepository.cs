using FCTournament.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FCTournament.Repository
{
    public class EFFormatRepository : IFormatRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string FormatCacheKey = "AllFormats";

        public EFFormatRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Format>> GetAllFormatsAsync()
        {
            if (!_cache.TryGetValue(FormatCacheKey, out IEnumerable<Format> formats))
            {
                formats = await _context.Formats.ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));

                _cache.Set(FormatCacheKey, formats, cacheEntryOptions);
            }

            return formats;
        }

        public async Task<Format> GetFormatByIdAsync(int id)
        {
            var allFormats = await GetAllFormatsAsync();
            return allFormats.FirstOrDefault(f => f.Id == id);
        }
        public async Task AddFormatAsync(Format format)
        {
            _context.Formats.Add(format);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateFormatAsync(Format format)
        {
            _context.Entry(format).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteFormatAsync(int id)
        {
            var format = await _context.Formats.FindAsync(id);
            if (format != null)
            {
                _context.Formats.Remove(format);
                await _context.SaveChangesAsync();
            }
        }
    }
}
