using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplication1.DataContext;
using WebApplication1.Models;
using WebApplication1.Services.Cookies.CarteItem;
using System.Text.Json;

namespace WebApplication1.Pages.Produits
{
    public class IndexModel : PageModel
    {
        private readonly ProduitDBContext _context;
        private readonly CarteItemCookiesManager _cookieCartService;
        private readonly IDistributedCache _cache;

        public IndexModel(
            ProduitDBContext context,
            CarteItemCookiesManager cookiesManager,
            IDistributedCache cache)
        {
            _context = context;
            _cookieCartService = cookiesManager;
            _cache = cache;
        }

        public IList<Produit> Produit { get; set; } = new List<Produit>();
        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public Guid? CategoryId { get; set; }

        private const string CATEGORIES_KEY = "categories:all";
        private const string PRODUITS_ALL_KEY = "produits:all";

        // =========================
        // GET (CACHED)
        // =========================
        public async Task OnGetAsync()
        {
            Categories = await GetCategoriesCachedAsync();
            Produit = await GetProduitsCachedAsync(CategoryId);
        }

        // =========================
        // CATEGORIES CACHE
        // =========================
        private async Task<List<Category>> GetCategoriesCachedAsync()
        {
            var cached = await _cache.GetStringAsync(CATEGORIES_KEY);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<List<Category>>(cached)!;

            var categories = await _context.Category
                .OrderBy(c => c.Name)
                .AsNoTracking()
                .ToListAsync();

            await _cache.SetStringAsync(
                CATEGORIES_KEY,
                JsonSerializer.Serialize(categories),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                });

            return categories;
        }

        // =========================
        // PRODUITS CACHE (FILTERED)
        // =========================
        private async Task<List<Produit>> GetProduitsCachedAsync(Guid? categoryId)
        {
            var cacheKey = categoryId.HasValue
                ? $"produits:category:{categoryId}"
                : PRODUITS_ALL_KEY;

            var cached = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cached))
                return JsonSerializer.Deserialize<List<Produit>>(cached)!;

            IQueryable<Produit> query = _context.Produit
                .Include(p => p.Category)
                .AsNoTracking();

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId);

            var produits = await query.ToListAsync();

            await _cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(produits),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                });

            return produits;
        }

        // =========================
        // ADD TO CART (UNCHANGED)
        // =========================
        public async Task<IActionResult> OnPostAddToCart(Guid productId, int quantity, int stock)
        {
            try
            {
                if (quantity <= 0)
                {
                    TempData["Error"] = "La quantité doit être supérieure à 0";
                    return RedirectToPage();
                }

                if (quantity > stock)
                {
                    TempData["Error"] = "La quantité demandée ne doit pas dépasser le stock disponible.";
                    return RedirectToPage();
                }

                var product = await _context.Produit
                    .Include(p => p.Category)
                    .FirstOrDefaultAsync(p => p.ProduitId == productId);

                if (product == null)
                {
                    TempData["Error"] = "Produit non trouvé";
                    return RedirectToPage();
                }

                var cartItems = _cookieCartService.LoadCartFromCookie()
                                ?? new List<CartItem>();

                var existingItem = cartItems
                    .FirstOrDefault(i => i.ProduitId == productId);

                if (existingItem != null)
                    existingItem.Quantity += quantity;
                else
                    cartItems.Add(new CartItem
                    {
                        ProduitId = product.ProduitId,
                        Quantity = quantity
                    });

                _cookieCartService.SaveCartToCookie(cartItems);

                TempData["Success"] = $"{quantity} x {product.Name} ajouté au panier !";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Erreur lors de l'ajout au panier";
                Console.WriteLine(ex);
            }

            return RedirectToPage();
        }
    }
}
