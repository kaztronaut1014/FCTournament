using FCTournament.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace FCTournament.Repository
{
    public class EFLocationRepository : ILocationRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private const string LocationCacheKey = "AllLocations";

        public EFLocationRepository(ApplicationDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<IEnumerable<Location>> GetAllLocationsAsync()
        {
            // 1. Thử lấy dữ liệu từ cache
            if (!_cache.TryGetValue(LocationCacheKey, out IEnumerable<Location> locations))
            {
                // 2. Nếu cache không có (Lần đầu chạy ứng dụng hoặc cache hết hạn), mới vào DB lấy
                locations = await _context.Locations.ToListAsync();

                // 3. Tùy chọn: Đặt thời gian lưu cache. Ví dụ: 24 tiếng.
                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));

                // 4. Lưu dữ liệu lấy được vào cache
                _cache.Set(LocationCacheKey, locations, cacheEntryOptions);
            }

            return locations;
        }

        public async Task<Location> GetLocationByIdAsync(int id)
        {
            // Với hàm lấy theo ID, bạn vẫn có thể tận dụng toàn bộ danh sách từ cache
            var allLocations = await GetAllLocationsAsync();
            return allLocations.FirstOrDefault(l => l.Id == id);
        }
        public async Task AddLocationAsync(Models.Location location)
        {
            _context.Locations.Add(location);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateLocationAsync(Models.Location location)
        {
            _context.Entry(location).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }
        public async Task DeleteLocationAsync(int id)
        {
            var location = await _context.Locations.FindAsync(id);
            if (location != null)
            {
                _context.Locations.Remove(location);
                await _context.SaveChangesAsync();
            }
        }
    }
}
