using System.Collections.Generic;
using System.Threading.Tasks;

namespace Taxjar.Data
{
    public interface ICachedKeys
    {
        Task AddCacheKey(int cacheKey);
        Task<List<int>> ListExpiredKeys();
        Task RemoveCacheKey(int cacheKey);
    }
}