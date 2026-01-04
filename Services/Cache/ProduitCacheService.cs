using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using WebApplication1.DataContext;
using WebApplication1.Models;


namespace DOTNETPanier.Services.Cache
{
    public class ProduitCacheService
    {
        private readonly ProduitDBContext _context;
        private readonly IDistributedCache _cache;

        private const string PRODUIT_LIST_KEY = "produits:all";

        public ProduitCacheService(
            ProduitDBContext context,
            IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        // 🔹 Get all products (CACHED)
        public async Task<List<Produit>> GetAllAsync()
        {
            var cachedData = await _cache.GetStringAsync(PRODUIT_LIST_KEY);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<List<Produit>>(cachedData)!;
            }

            var produits = await _context.Produit
                .Include(p => p.Category)
                .AsNoTracking()
                .ToListAsync();

            var json = JsonSerializer.Serialize(produits);

            await _cache.SetStringAsync(
                PRODUIT_LIST_KEY,
                json,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return produits;
        }

        // 🔹 Get product by ID (CACHED)
        public async Task<Produit?> GetByIdAsync(Guid id)
        {
            var key = $"produit:{id}";

            var cached = await _cache.GetStringAsync(key);
            if (cached != null)
                return JsonSerializer.Deserialize<Produit>(cached);

            var produit = await _context.Produit
                .Include(p => p.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.ProduitId == id);

            if (produit == null)
                return null;

            await _cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(produit),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return produit;
        }

        // 🔹 Invalidate cache (VERY IMPORTANT)
        public async Task ClearCacheAsync(Guid? produitId = null)
        {
            await _cache.RemoveAsync(PRODUIT_LIST_KEY);

            if (produitId.HasValue)
                await _cache.RemoveAsync($"produit:{produitId}");
        }
    }
}
