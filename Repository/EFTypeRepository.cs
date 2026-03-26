using FCTournament.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FCTournament.Repository
{
    public class EFTypeRepository : ITypeRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string TypeCacheKey = "AllTypes";

        public EFTypeRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Models.Type>> GetAllTypeAsync()
        {
            if (!_cache.TryGetValue(TypeCacheKey, out IEnumerable<Models.Type> types))
            {
                types = await _context.Types.ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));

                _cache.Set(TypeCacheKey, types, cacheEntryOptions);
            }

            return types;
        }

        public async Task<Models.Type> GetTypeByIdAsync(int id)
        {
            var allTypes = await GetAllTypeAsync();
            return allTypes.FirstOrDefault(t => t.Id == id);
        }

        public async Task AddTypeAsync(Models.Type type)
        {
            _context.Types.Add(type);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTypeAsync(Models.Type type)
        {
            _context.Entry(type).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTypeAsync(int id)
        {
            var type = await _context.Types.FindAsync(id);
            if (type != null)
            {
                _context.Types.Remove(type);
                await _context.SaveChangesAsync();
            }
        }
    }
}
